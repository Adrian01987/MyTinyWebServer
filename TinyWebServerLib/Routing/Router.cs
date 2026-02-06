using System.Text.RegularExpressions;
using TinyWebServerLib.Http;

namespace TinyWebServerLib.Routing;

/// <summary>
/// Handles URL routing by matching incoming requests to registered route handlers.
/// Supports parameterized routes using <c>{parameter}</c> syntax.
/// </summary>
public partial class Router
{
    private readonly List<(string Method, Regex Pattern, RequestHandler Handler)> routes = [];

    /// <summary>
    /// Registers a route pattern with a handler for a specific HTTP method.
    /// </summary>
    /// <param name="httpMethod">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="route">The route pattern. Use <c>{paramName}</c> for route parameters.</param>
    /// <param name="handler">The async function to handle matching requests.</param>
    public void Map(string httpMethod, string route, RequestHandler handler)
    {
        // Normalize the route to ensure it starts with a slash
        var normalizedRoute = route.StartsWith('/') ? route : "/" + route;
        var pattern = new Regex(
            $"^{Regex.Replace(normalizedRoute, @"\{(\w+)\}", @"(?<$1>[^/]+)")}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        routes.Add((httpMethod, pattern, handler));
    }

    /// <summary>
    /// Routes an incoming request to the appropriate handler based on method and path.
    /// Extracts route parameters and attaches them to the request.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>The HTTP response from the matched handler, or a 404 response if no route matches.</returns>
    public async Task<HttpResponse> RouteAsync(HttpRequest request)
    {
        // Remove query string for route matching
        var path = request.Path;
        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        foreach (var (method, pattern, handler) in routes)
        {
            if (request.Method.Equals(method, StringComparison.OrdinalIgnoreCase))
            {
                // Case-insensitive matching is handled by RegexOptions.IgnoreCase
                var match = pattern.Match(path);
                if (match.Success)
                {
                    foreach (var groupName in pattern.GetGroupNames())
                    {
                        if (match.Groups[groupName].Success)
                        {
                            request.RouteParameters[groupName] = match.Groups[groupName].Value;
                        }
                    }
                    return await handler(request);
                }
            }
        }

        var response = new HttpResponse(404, [], "Not Found");
        response.Headers["Content-Type"] = "text/plain";
        return response;
    }
}
