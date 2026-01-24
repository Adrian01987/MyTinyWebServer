using System.Net;
using System.Net.Sockets;
using System.Text;
using TinyWebServerLib.Extensions;
using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Integration;

/// <summary>
/// Integration tests for the TinyWebServer.
/// Tests the full request/response cycle through the server.
/// </summary>
public class TinyWebServerIntegrationTests : IAsyncLifetime
{
    private Server.TinyWebServer? _server;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private int _port;

    public async Task InitializeAsync()
    {
        // Find an available port
        _port = GetAvailablePort();
        _cts = new CancellationTokenSource();

        var builder = new TinyWebServerBuilder();
        builder
            .UseUrl(IPAddress.Loopback, _port)
            .Map("GET", "/", _ => Task.FromResult(new HttpResponse(200, new Dictionary<string, string> { ["Content-Type"] = "text/plain" }, "Hello World")))
            .Map("GET", "/api/test", _ => Task.FromResult(new HttpResponse(200, new Dictionary<string, string> { ["Content-Type"] = "application/json" }, "{\"status\":\"ok\"}")))
            .Map("GET", "/api/users/{id}", request =>
            {
                var id = request.RouteParameters["id"];
                return Task.FromResult(new HttpResponse(200, new Dictionary<string, string> { ["Content-Type"] = "application/json" }, $"{{\"id\":{id}}}"));
            })
            .Map("POST", "/api/users", request =>
            {
                return Task.FromResult(new HttpResponse(201, new Dictionary<string, string> { ["Content-Type"] = "application/json" }, request.Body));
            });

        _server = builder.Build();
        _serverTask = _server.StartAsync(_cts.Token);

        // Give the server time to start
        await Task.Delay(100);
    }

    public async Task DisposeAsync()
    {
        _cts?.Cancel();
        if (_serverTask != null)
        {
            try
            {
                await _serverTask.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
        _cts?.Dispose();
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task<(int StatusCode, string Body, Dictionary<string, string> Headers)> SendRequestAsync(
        string method, string path, string? body = null, Dictionary<string, string>? headers = null)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port);

        await using var stream = client.GetStream();
        var requestBuilder = new StringBuilder();
        requestBuilder.AppendLine($"{method} {path} HTTP/1.1");
        requestBuilder.AppendLine($"Host: localhost:{_port}");

        if (headers != null)
        {
            foreach (var header in headers)
            {
                requestBuilder.AppendLine($"{header.Key}: {header.Value}");
            }
        }

        if (!string.IsNullOrEmpty(body))
        {
            requestBuilder.AppendLine($"Content-Length: {body.Length}");
        }

        requestBuilder.AppendLine();

        if (!string.IsNullOrEmpty(body))
        {
            requestBuilder.Append(body);
        }

        var requestBytes = Encoding.UTF8.GetBytes(requestBuilder.ToString());
        await stream.WriteAsync(requestBytes);

        // Read response
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var response = await reader.ReadToEndAsync();

        // Parse response
        var lines = response.Split("\r\n");
        var statusLine = lines[0].Split(' ');
        var statusCode = int.Parse(statusLine[1]);

        var responseHeaders = new Dictionary<string, string>();
        var bodyStartIndex = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                bodyStartIndex = i + 1;
                break;
            }
            var headerParts = lines[i].Split(": ", 2);
            if (headerParts.Length == 2)
            {
                responseHeaders[headerParts[0]] = headerParts[1];
            }
        }

        var responseBody = string.Join("\r\n", lines.Skip(bodyStartIndex));

        return (statusCode, responseBody, responseHeaders);
    }

    [Fact]
    public async Task GetRequest_RootPath_ReturnsHelloWorld()
    {
        // Act
        var (statusCode, body, _) = await SendRequestAsync("GET", "/");

        // Assert
        statusCode.Should().Be(200);
        body.Should().Be("Hello World");
    }

    [Fact]
    public async Task GetRequest_ApiEndpoint_ReturnsJson()
    {
        // Act
        var (statusCode, body, headers) = await SendRequestAsync("GET", "/api/test");

        // Assert
        statusCode.Should().Be(200);
        body.Should().Be("{\"status\":\"ok\"}");
        headers["Content-Type"].Should().Be("application/json");
    }

    [Fact]
    public async Task GetRequest_WithRouteParameter_ExtractsParameter()
    {
        // Act
        var (statusCode, body, _) = await SendRequestAsync("GET", "/api/users/42");

        // Assert
        statusCode.Should().Be(200);
        body.Should().Be("{\"id\":42}");
    }

    [Fact]
    public async Task PostRequest_WithBody_ReturnsBody()
    {
        // Arrange
        var requestBody = "{\"name\":\"John\"}";

        // Act
        var (statusCode, body, _) = await SendRequestAsync("POST", "/api/users", requestBody);

        // Assert
        statusCode.Should().Be(201);
        body.Should().Be(requestBody);
    }

    [Fact]
    public async Task GetRequest_NonExistentPath_Returns404()
    {
        // Act
        var (statusCode, body, _) = await SendRequestAsync("GET", "/nonexistent");

        // Assert
        statusCode.Should().Be(404);
        body.Should().Be("Not Found");
    }

    [Fact]
    public async Task WrongMethod_ExistingPath_Returns404()
    {
        // Act - DELETE is not mapped for /api/test
        var (statusCode, _, _) = await SendRequestAsync("DELETE", "/api/test");

        // Assert
        statusCode.Should().Be(404);
    }
}
