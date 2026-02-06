namespace TinyWebServerLib.Http;

/// <summary>
/// Represents an HTTP response with a status code, headers, and body content.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// Creates a new HTTP response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code (e.g., 200, 404, 500).</param>
    /// <param name="headers">The HTTP response headers.</param>
    /// <param name="body">The response body content.</param>
    /// <exception cref="ArgumentNullException">Thrown when headers or body is null.</exception>
    public HttpResponse(int statusCode, Dictionary<string, string> headers, string body)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(body);

        StatusCode = statusCode;
        Headers = headers;
        Body = body;
    }

    /// <summary>Gets or sets the HTTP status code.</summary>
    public int StatusCode { get; set; }

    /// <summary>Gets or sets the HTTP response headers.</summary>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>Gets or sets the response body content.</summary>
    public string Body { get; set; }
}
