using Microsoft.AspNetCore.Http;

namespace Server.Web.Extensions;

public static class Proxy
{
    public static HttpRequestMessage CreateProxyHttpRequest(this HttpContext context, Uri uri)
    {
        var request = context.Request;

        var requestMessage = new HttpRequestMessage();
        var requestMethod = request.Method;
        if (!HttpMethods.IsGet(requestMethod) &&
            !HttpMethods.IsHead(requestMethod) &&
            !HttpMethods.IsDelete(requestMethod) &&
            !HttpMethods.IsTrace(requestMethod))
        {
            var streamContent = new StreamContent(request.Body);
            requestMessage.Content = streamContent;
        }

        // Copy the request headers, excluding hop-by-hop headers
        var hopByHop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade"
        };

        foreach (var header in request.Headers)
        {
            if (hopByHop.Contains(header.Key))
                continue;

            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        requestMessage.Headers.Host = uri.Authority;
        requestMessage.RequestUri = uri;
        requestMessage.Method = new HttpMethod(request.Method);

        return requestMessage;
    }
    public static async Task CopyProxyHttpResponse(this HttpContext context, HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        var response = context.Response;

        response.StatusCode = (int)responseMessage.StatusCode;

        // Copy response headers excluding hop-by-hop
        var hopByHop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade"
        };

        foreach (var header in responseMessage.Headers)
        {
            if (hopByHop.Contains(header.Key))
                continue;
            response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            if (hopByHop.Contains(header.Key))
                continue;
            response.Headers[header.Key] = header.Value.ToArray();
        }

        // SendAsync removes chunking from the response.
        // This removes the header so it doesn't expect a chunked response.
        response.Headers.Remove("transfer-encoding");

        await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(response.Body, context.RequestAborted);
    }
}
