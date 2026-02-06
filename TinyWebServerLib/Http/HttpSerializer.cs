using System.Text;

namespace TinyWebServerLib.Http;

public static class HttpSerializer
{
    public static string Serialize(HttpResponse response)
    {
        // Auto-add Content-Length if not already present and body is non-empty
        if (!response.Headers.ContainsKey("Content-Length") && !string.IsNullOrEmpty(response.Body))
        {
            response.Headers["Content-Length"] = Encoding.UTF8.GetByteCount(response.Body).ToString();
        }

        StringBuilder sb = new();
        // Use explicit \r\n (HTTP requires CRLF, not platform-dependent line endings)
        sb.Append($"HTTP/1.1 {response.StatusCode} {GetReasonPhrase(response.StatusCode)}\r\n");
        foreach (var header in response.Headers)
        {
            sb.Append($"{header.Key}: {header.Value}\r\n");
        }
        sb.Append("\r\n");
        sb.Append(response.Body);
        return sb.ToString();
    }

    private static string GetReasonPhrase(int statusCode) =>
        statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            301 => "Moved Permanently",
            302 => "Found",
            304 => "Not Modified",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            409 => "Conflict",
            415 => "Unsupported Media Type",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => ""
        };
}
