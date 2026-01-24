namespace TinyWebServerLib.Http;

/// <summary>
/// Represents an HTTP response with a status code, headers, and body content.
/// </summary>
/// <param name="statusCode">The HTTP status code (e.g., 200, 404, 500).</param>
/// <param name="headers">The HTTP response headers.</param>
/// <param name="body">The response body content.</param>
public class HttpResponse(int statusCode, Dictionary<string, string> headers, string body)
{
    /// <summary>Gets or sets the HTTP status code.</summary>
    public int StatusCode { get; set; } = statusCode;

    /// <summary>Gets or sets the HTTP response headers.</summary>
    public Dictionary<string, string> Headers { get; set; } = headers ?? [];

    /// <summary>Gets or sets the response body content.</summary>
    public string Body { get; set; } = body ?? string.Empty;
}
