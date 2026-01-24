using TinyWebServerLib.Http;

namespace TinyWebServerLib.Middleware;

/// <summary>
/// Builds a middleware pipeline using the "Russian doll" pattern.
/// Each middleware wraps the next, allowing pre- and post-processing of requests.
/// </summary>
public class MiddlewareBuilder
{
    private readonly Stack<Func<Func<HttpRequest, Task<HttpResponse>>, Func<HttpRequest, Task<HttpResponse>>>> components
        = new();

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <param name="middleware">A function that receives the next delegate and returns a wrapped delegate.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MiddlewareBuilder Use(Func<Func<HttpRequest, Task<HttpResponse>>, Func<HttpRequest, Task<HttpResponse>>> middleware)
    {
        components.Push(middleware);
        return this;
    }

    /// <summary>
    /// Builds the middleware pipeline, composing all registered middleware around a terminal handler.
    /// </summary>
    /// <param name="terminal">The final handler that processes the request after all middleware.</param>
    /// <returns>A composed delegate representing the entire pipeline.</returns>
    public Func<HttpRequest, Task<HttpResponse>> Build(Func<HttpRequest, Task<HttpResponse>> terminal)
    {
        Func<HttpRequest, Task<HttpResponse>> app = terminal;
        while (components.Count > 0)
        {
            app = components.Pop()(app);
        }
        return app;
    }
}
