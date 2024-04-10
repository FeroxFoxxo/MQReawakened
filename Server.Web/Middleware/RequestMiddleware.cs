using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Web.Extensions;
using Server.Web.Models;
using System.Text;

namespace Server.Web.Middleware;

public class RequestMiddleware
{
    private readonly HttpClient _client;
    private readonly RequestDelegate _next;

    public RequestMiddleware(RequestDelegate next)
    {
        _client = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false
        });
        _next = next;
    }

    public async Task Invoke(HttpContext context, ILogger<RequestMiddleware> logger, WebRConfig webRConfig,
        WebRwConfig webRwConfig, InternalRwConfig config, FileLogger fileLogger)
    {
        context.Request.EnableBuffering();

        var method = context.Request.Method;

        method = method switch
        {
            "DELETE" => "DEL",
            "OPTIONS" => "OPT",
            "PATCH" => "PAT",
            "TRACE" => "TRC",
            "CONNECT" => "CON",
            "HEAD" => "HED",
            "POST" => "POS",
            _ => method
        };

        var postData = string.Empty;

        if (context.Request.HasFormContentType)
        {
            postData = $" | Post Data: {string.Join(", ", context.Request.Form.Select(x => $"{x.Key}:{x.Value}"))}";
            var split = postData.Split('\n');

            if (split.Length > 1 && webRwConfig.ShouldConcat)
                postData = $"{string.Join('\n', split.Take(1))}\n" +
                           "More data found, but was concatenated. To view the full log, disable WebConfig.ShouldConcat JSON file.";
        }

        var ip = GetIp(context, logger);

        var sb = new StringBuilder();

        var queryValue = context.Request.QueryString.HasValue;
        var postValue = !string.IsNullOrEmpty(postData);

        var path = $"Path: {context.Request.Path.Value}";

        if (queryValue || postValue)
            sb.AppendLine(path);
        else
            sb.Append(path);

        try
        {
            if (config.NetworkType == NetworkType.Client &&
                config.StrictNetworkCheck() &&
                !webRConfig.IgnorePaths.Any(
                    p => context.Request.Path.ToString().StartsWith(p)
                )
               )
            {
                var baseUrl = new Uri(config.GetHostAddress());
                var url = new Uri(baseUrl, context.Request.Path + context.Request.QueryString);
                logger.LogTrace("[PROXIED TO {Address}]", url);
                var request = context.CreateProxyHttpRequest(url);
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    context.RequestAborted);
                await context.CopyProxyHttpResponse(response);
            }
            else
            {
                await _next(context);
            }

            var logType = context.Response.StatusCode switch
            {
                StatusCodes.Status418ImATeapot => LoggerType.Unknown,
                StatusCodes.Status404NotFound => LoggerType.Warning,
                _ => LoggerType.Trace
            };

            if (postValue)
            {
                var post = $"Post: {postData}";

                if (queryValue)
                    sb.AppendLine(post);
                else
                    sb.Append(post);
            }

            if (queryValue)
                sb.Append($"Query: {context.Request.QueryString}");

            LogRequest(fileLogger, context.Response.StatusCode, method, sb, ip, logType);
        }
        catch (Exception ex)
        {
            sb.AppendLine("Unable to run web request");
            sb.Append($"Error: {ex}");

            LogRequest(fileLogger, 500, method, sb, ip, LoggerType.Error);
            throw;
        }
    }

    public static void LogRequest(FileLogger fileLogger, int statusCode, string method, StringBuilder info, string ip,
        LoggerType loggerType)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Address: {ip}")
            .AppendLine(method);

        sb.Append(info);

        fileLogger.WriteGenericLog<Controller>("http-requests", $"Status {statusCode}", sb.ToString(), loggerType);
    }

    private static string GetIp(HttpContext context, ILogger logger)
    {
        try
        {
            string ip = context.Request.Headers["X-Forwarded-For"]!;

            if (string.IsNullOrEmpty(ip))
                ip = context.Request.Headers["REMOTE_ADDR"]!;
            else
                // Using X-Forwarded-For last address
                ip = ip.Split(',').Last().Trim();

            if (context.Connection.RemoteIpAddress != null)
                return string.IsNullOrEmpty(ip) ? context.Connection.RemoteIpAddress.ToString() : ip;
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting IP Address");
            logger.LogError(ex, "Unable to find IP");
        }

        return string.Empty;
    }
}
