using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    public async Task Invoke(HttpContext context, InternalRwConfig config)
    {
        if (config.NetworkType == NetworkType.Client && config.StrictNetworkCheck())
        {
            var targetUri = await RewriteUri(context, new Uri(config.ServerAddress));

            if (targetUri != null)
            {
                var requestMessage = GenerateProxiedRequest(context, targetUri);
                await SendAsync(context, requestMessage);

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

    private async Task SendAsync(HttpContext context, HttpRequestMessage requestMessage)
    {
        Console.WriteLine(requestMessage);
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
    }

    private HttpRequestMessage GenerateProxiedRequest(HttpContext context, Uri targetUri)
    {
        var requestMessage = new HttpRequestMessage();
        CopyRequestContentAndHeaders(context, requestMessage);

        requestMessage.RequestUri = targetUri;
        requestMessage.Headers.Host = targetUri.Host;
        requestMessage.Method = GetMethod(context.Request.Method);

        return requestMessage;
    }

    private void CopyRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
    {
        var requestMethod = context.Request.Method;
        if (!HttpMethods.IsGet(requestMethod) &&
            !HttpMethods.IsHead(requestMethod) &&
            !HttpMethods.IsDelete(requestMethod) &&
            !HttpMethods.IsTrace(requestMethod))
        {
            var streamContent = new StreamContent(context.Request.Body);
            requestMessage.Content = streamContent;
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
