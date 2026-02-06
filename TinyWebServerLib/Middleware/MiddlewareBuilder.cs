using TinyWebServerLib.Http;

namespace TinyWebServerLib.Middleware;

/// <summary>
/// Builds a middleware pipeline using the "Russian doll" pattern.
/// Each middleware wraps the next, allowing pre- and post-processing of requests.
/// </summary>
public class MiddlewareBuilder
{
    private readonly List<Func<RequestHandler, RequestHandler>> components = [];

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <param name="middleware">A function that receives the next delegate and returns a wrapped delegate.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MiddlewareBuilder Use(Func<RequestHandler, RequestHandler> middleware)
    {
        components.Add(middleware);
        return this;
    }

    /// <summary>
    /// Builds the middleware pipeline, composing all registered middleware around a terminal handler.
    /// The first middleware added is the outermost (executes first).
    /// This method is idempotent — calling it multiple times returns equivalent pipelines.
    /// </summary>
    /// <param name="terminal">The final handler that processes the request after all middleware.</param>
    /// <returns>A composed delegate representing the entire pipeline.</returns>
    public RequestHandler Build(RequestHandler terminal)
    {
        RequestHandler app = terminal;
        // Iterate in reverse so that the first middleware added is the outermost
        for (int i = components.Count - 1; i >= 0; i--)
        {
            app = components[i](app);
        }
        return app;
    }
}
