using TinyWebServerLib.Http;
using TinyWebServerLib.Middleware;
using TinyWebServerLib.Routing;
using TinyWebServerLib.Server;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace TinyWebServerLib.Extensions;

/// <summary>
/// Provides a fluent API for configuring and building a <see cref="TinyWebServer"/> instance.
/// </summary>
public class TinyWebServerBuilder
{
    private readonly MiddlewareBuilder middlewareBuilder = new();
    private readonly Router router = new();
    private IPAddress address = IPAddress.Any;
    private int port = 4221;
    private IServiceProvider? serviceProvider;

    /// <summary>
    /// Adds a middleware component to the request pipeline.
    /// Middleware components are executed in the order they are added.
    /// </summary>
    /// <param name="middleware">A function that receives the next middleware delegate and returns a new delegate that wraps it.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TinyWebServerBuilder Use(Func<Func<HttpRequest, Task<HttpResponse>>, Func<HttpRequest, Task<HttpResponse>>> middleware)
    {
        middlewareBuilder.Use(middleware);
        return this;
    }

    /// <summary>
    /// Maps a route pattern and HTTP method to a request handler.
    /// </summary>
    /// <param name="httpMethod">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="route">The route pattern, which may contain parameters like <c>{id}</c>.</param>
    /// <param name="handler">The async function that handles matching requests.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TinyWebServerBuilder Map(string httpMethod, string route, Func<HttpRequest, Task<HttpResponse>> handler)
    {
        router.Map(httpMethod, route.ToLower(), handler);
        return this;
    }

    /// <summary>
    /// Configures the IP address and port that the server will listen on.
    /// </summary>
    /// <param name="address">The IP address to bind to. Use <see cref="IPAddress.Any"/> to listen on all interfaces.</param>
    /// <param name="port">The port number to listen on.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TinyWebServerBuilder UseUrl(IPAddress address, int port)
    {
        this.address = address;
        this.port = port;
        return this;
    }

    /// <summary>
    /// Sets the dependency injection service provider used to resolve controllers and services.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TinyWebServerBuilder UseServiceProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured <see cref="TinyWebServer"/> instance.
    /// </summary>
    /// <returns>A new <see cref="TinyWebServer"/> ready to be started.</returns>
    public TinyWebServer Build()
    {
        Func<HttpRequest, Task<HttpResponse>> terminal = router.RouteAsync;
        var pipelineDelegate = middlewareBuilder.Build(terminal);
        RequestPipeline pipeline = new(pipelineDelegate);
        return new TinyWebServer(address, port, pipeline, serviceProvider ?? new ServiceCollection().BuildServiceProvider());
    }
}
