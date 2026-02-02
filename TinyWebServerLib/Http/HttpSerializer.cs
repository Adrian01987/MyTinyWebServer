using System.Text;

namespace TinyWebServerLib.Http;

public static class HttpSerializer
{
    public static string Serialize(HttpResponse response)
    {
        StringBuilder sb = new();
        sb.AppendLine($"HTTP/1.1 {response.StatusCode} {GetReasonPhrase(response.StatusCode)}");
        foreach (var header in response.Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }
        sb.AppendLine();
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
