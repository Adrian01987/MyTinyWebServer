namespace TinyWebServerLib.Attributes;

/// <summary>
/// Marks a class as an API controller, enabling automatic route discovery and mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
}

/// <summary>
/// Base class for HTTP method attributes that map controller actions to routes.
/// </summary>
/// <param name="httpMethod">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
/// <param name="route">The route template for this action.</param>
public abstract class HttpMethodAttribute(string httpMethod, string? route = null) : Attribute
{
    /// <summary>Gets the HTTP method for this action.</summary>
    public string HttpMethod { get; } = httpMethod;

    /// <summary>Gets the route template for this action.</summary>
    public string Route { get; } = route ?? string.Empty;
}

/// <summary>
/// Maps a controller action to HTTP GET requests.
/// </summary>
/// <param name="route">The route template (e.g., "customers/{id}").</param>
[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute(string? route = null) : HttpMethodAttribute("GET", route)
{
}

/// <summary>
/// Maps a controller action to HTTP POST requests.
/// </summary>
/// <param name="route">The route template (e.g., "customers").</param>
[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute(string? route = null) : HttpMethodAttribute("POST", route)
{
}
