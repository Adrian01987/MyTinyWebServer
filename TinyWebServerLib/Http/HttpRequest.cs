namespace TinyWebServerLib.Http;

/// <summary>
/// Represents an incoming HTTP request with its method, path, headers, body, and request-scoped services.
/// </summary>
/// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
/// <param name="path">The request path (e.g., "/customers/123").</param>
/// <param name="headers">The HTTP headers as key-value pairs.</param>
/// <param name="body">The request body content.</param>
public class HttpRequest(string method, string path, Dictionary<string, string> headers, string body)
{
    private Dictionary<string, string>? queryParameters;

    /// <summary>Gets or sets the HTTP method.</summary>
    public string Method { get; set; } = method ?? string.Empty;

    /// <summary>Gets or sets the request path (including query string if present).</summary>
    public string Path { get; set; } = path ?? string.Empty;

    /// <summary>Gets or sets the HTTP headers.</summary>
    public Dictionary<string, string> Headers { get; set; } = headers ?? [];

    /// <summary>Gets or sets the request body content.</summary>
    public string Body { get; set; } = body ?? string.Empty;

    /// <summary>Gets or sets the route parameters extracted from the URL path.</summary>
    public Dictionary<string, object> RouteParameters { get; set; } = [];

    /// <summary>Gets or sets the request-scoped dependency injection service provider.</summary>
    public IServiceProvider RequestServices { get; set; } = default!;

    /// <summary>
    /// Gets the path without the query string.
    /// </summary>
    public string PathBase
    {
        get
        {
            var queryIndex = Path.IndexOf('?');
            return queryIndex >= 0 ? Path[..queryIndex] : Path;
        }
    }

    /// <summary>
    /// Gets the query string parameters parsed from the URL.
    /// Example: For path "/search?q=hello&page=1", returns {"q": "hello", "page": "1"}
    /// </summary>
    public Dictionary<string, string> QueryParameters
    {
        get
        {
            if (queryParameters != null) return queryParameters;

            queryParameters = [];
            var queryIndex = Path.IndexOf('?');
            if (queryIndex < 0) return queryParameters;

            var queryString = Path[(queryIndex + 1)..];
            foreach (var pair in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                queryParameters[key] = value;
            }

            return queryParameters;
        }
    }
}
