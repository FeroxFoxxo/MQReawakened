using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Web.Models;
using System.Text;

namespace Server.Web.Middleware;

public class RequestLogger
{
    private readonly RequestDelegate _next;

    public RequestLogger(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, ILogger<RequestLogger> logger, WebConfig webConfig, FileLogger fileLogger)
    {
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

        if (context.Request.HasFormContentType & webConfig.ShouldConcat)
        {
            postData = $" | Post Data: {string.Join(", ", context.Request.Form.Select(x => $"{x.Key}:{x.Value}"))}";
            var split = postData.Split('\n');

            if (split.Length > 1)
                postData = $"{string.Join('\n', split.Take(1))}\n" +
                           "More data found, but was concatenated. To view the full log, enable WebConfig.ShouldConcat.";
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
            await _next(context);

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

    public static void LogRequest(FileLogger fileLogger, int statusCode, string method, StringBuilder info, string ip, LoggerType loggerType)
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
                ip = ip.Split(',')
                    .Last()
                    .Trim();

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
