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
            400 => "Bad Request",
            404 => "Not Found",
            _ => ""
        };
}
