using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TinyWebServerLib.Http;
using TinyWebServerLib.Routing;

namespace TinyWebServerLib.Server;

/// <summary>
/// A lightweight HTTP server that handles incoming TCP connections and processes HTTP requests.
/// Implements <see cref="IAsyncDisposable"/> for proper resource cleanup.
/// </summary>
public class TinyWebServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly RequestPipeline pipeline;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger? logger;
    private readonly List<Task> activeConnections = [];
    private bool disposed;

    /// <summary>
    /// Creates a new instance of the TinyWebServer.
    /// </summary>
    /// <param name="address">The IP address to listen on.</param>
    /// <param name="port">The port to listen on.</param>
    /// <param name="pipeline">The request processing pipeline.</param>
    /// <param name="serviceProvider">The dependency injection service provider.</param>
    public TinyWebServer(IPAddress address, int port, RequestPipeline pipeline, IServiceProvider serviceProvider)
    {
        listener = new TcpListener(address, port);
        this.pipeline = pipeline;
        this.serviceProvider = serviceProvider;
        this.logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<TinyWebServer>();
    }

    /// <summary>
    /// Starts the server and begins accepting connections.
    /// </summary>
    /// <param name="token">A cancellation token to stop the server.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the server has been disposed.</exception>
    public async Task StartAsync(CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        listener.Start();
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(token);
                    var task = HandleClientAsync(client, token);
                    activeConnections.Add(task);
                    // Clean up completed tasks periodically
                    activeConnections.RemoveAll(t => t.IsCompleted);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown - don't log as error
                }
            }
        }
        finally
        {
            // Wait for all in-flight requests to complete
            await Task.WhenAll(activeConnections);
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        await using NetworkStream stream = client.GetStream();
        await using var scope = serviceProvider.CreateAsyncScope();
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var requestText = new StringBuilder();
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(token)))
            {
                requestText.AppendLine(line);
            }

            HttpRequest request = HttpParser.Parse(requestText.ToString());
            request.RequestServices = scope.ServiceProvider;

            if (request.Headers.TryGetValue("Content-Length", out var contentLengthStr) && int.TryParse(contentLengthStr, out var contentLength))
            {
                var bodyBuffer = new char[contentLength];
                await reader.ReadBlockAsync(bodyBuffer, 0, contentLength);
                request.Body = new string(bodyBuffer);
            }

            HttpResponse response = await pipeline.RequestDelegate(request);
            response.Headers["Connection"] = "close";
            string responseText = HttpSerializer.Serialize(response);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            await stream.WriteAsync(responseBytes, token);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error handling request");
            var response = new HttpResponse(500, new Dictionary<string, string>
            {
                ["Content-Type"] = "text/plain",
                ["Connection"] = "close"
            }, "Internal Server Error");
            string responseText = HttpSerializer.Serialize(response);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            await stream.WriteAsync(responseBytes, token);
        }
        finally
        {
            client.Close();
        }
    }

    /// <summary>
    /// Disposes the server and releases all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;

        listener.Stop();

        if (serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
