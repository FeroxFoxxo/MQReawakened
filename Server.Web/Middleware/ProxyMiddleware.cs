using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using System.Text;

namespace Server.Web.Middleware;

public class ProxyMiddleware
{
    private readonly HttpClient _httpClient;

    private readonly string _cdnHeaderName;
    private readonly string[] _notForwardedHttpHeaders;

    private readonly RequestDelegate _next;

    public ProxyMiddleware(RequestDelegate next)
    {
        _next = next;

        _cdnHeaderName = "Cache-Control";
        _notForwardedHttpHeaders = new []{ "Connection", "Host" };

        _httpClient = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = int.MaxValue,
            UseCookies = false,
        });
    }

    public async Task Invoke(HttpContext context, InternalRwConfig config, FileLogger fLogger)
    {
        if (config.NetworkType == NetworkType.Client && config.StrictNetworkCheck())
        {
            var targetUri = await RewriteUri(context, new Uri(config.ServerAddress));

            if (targetUri != null)
            {
                var requestMessage = await GenerateProxiedRequest(context, targetUri);
                var response = await SendAsync(context, requestMessage, fLogger);

                return;
            }

            await _next(context);
        }
        else
        {
            await _next(context);
        }
    }

    public Task<Uri> RewriteUri(HttpContext context, Uri baseUrl)
    {
        var url = context.Request.Path + context.Request.QueryString;
        var newUri = new Uri(baseUrl, url).ToString();

        return Uri.TryCreate(newUri, UriKind.Absolute, out var targetUri) ?
            Task.FromResult(targetUri) :
            Task.FromResult((Uri)null);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpContext context, HttpRequestMessage requestMessage, FileLogger fLogger)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[PROXIED]");

        if (requestMessage.Content == null) 
            sb.Append(requestMessage);
        else
        {
            sb.AppendLine(requestMessage.ToString())
                .AppendLine("[CONTENT]")
                .Append(await new StreamReader(await requestMessage.Content.ReadAsStreamAsync()).ReadToEndAsync());
        }

        fLogger.WriteGenericLog<ProxyMiddleware>("proxied-requests", "Received Web Request", sb.ToString(), LoggerType.Trace);

        using var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        context.Response.StatusCode = (int)responseMessage.StatusCode;

        foreach (var header in responseMessage.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();

        foreach (var header in responseMessage.Content.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();

        context.Response.Headers.Remove("transfer-encoding");

        if (!context.Response.Headers.ContainsKey(_cdnHeaderName))
            context.Response.Headers.Add(_cdnHeaderName, "no-cache, no-store");

        await responseMessage.Content.CopyToAsync(context.Response.Body);

        return responseMessage;
    }

    private async Task<HttpRequestMessage> GenerateProxiedRequest(HttpContext context, Uri targetUri)
    {
        var requestMessage = new HttpRequestMessage();
        await CopyRequestContentAndHeaders(context, requestMessage);

        requestMessage.RequestUri = targetUri;
        requestMessage.Headers.Host = targetUri.Host;
        requestMessage.Method = GetMethod(context.Request.Method);

        return requestMessage;
    }

    private async Task CopyRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
    {
        var requestMethod = context.Request.Method;

        if (!HttpMethods.IsGet(requestMethod) &&
            !HttpMethods.IsHead(requestMethod) &&
            !HttpMethods.IsDelete(requestMethod) &&
            !HttpMethods.IsTrace(requestMethod))
        {
            var content = await new StreamReader(context.Request.Body).ReadToEndAsync();
            requestMessage.Content = new StringContent(content);
        }

        foreach (var header in context.Request.Headers)
        {
            if (_notForwardedHttpHeaders.Contains(header.Key))
                continue;

            if (header.Key != "User-Agent")
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            else
            {
                var userAgent = header.Value.Count > 0 ? header.Value[0] + " " + context.TraceIdentifier : string.Empty;

                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, userAgent) && requestMessage.Content != null)
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, userAgent);
            }
        }
    }

    private static HttpMethod GetMethod(string method) =>
        HttpMethods.IsDelete(method) ? HttpMethod.Delete :
        HttpMethods.IsGet(method) ? HttpMethod.Get :
        HttpMethods.IsHead(method) ? HttpMethod.Head :
        HttpMethods.IsOptions(method) ? HttpMethod.Options :
        HttpMethods.IsPost(method) ? HttpMethod.Post :
        HttpMethods.IsPut(method) ? HttpMethod.Put :
        HttpMethods.IsTrace(method) ? HttpMethod.Trace :
        new HttpMethod(method);
}
