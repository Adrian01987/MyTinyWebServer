using TinyWebServerLib.Http;
using TinyWebServerLib.Middleware;

namespace TinyWebServerLib.Tests.Middleware;

/// <summary>
/// Unit tests for the <see cref="MiddlewareBuilder"/> class.
/// </summary>
public class MiddlewareBuilderTests
{
    [Fact]
    public async Task Build_NoMiddleware_CallsTerminalDirectly()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        var terminalCalled = false;
        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
        {
            terminalCalled = true;
            return Task.FromResult(new HttpResponse(200, [], "Terminal"));
        };

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        var response = await pipeline(request);

        // Assert
        terminalCalled.Should().BeTrue();
        response.Body.Should().Be("Terminal");
    }

    [Fact]
    public async Task Build_SingleMiddleware_ExecutesBeforeTerminal()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        var executionOrder = new List<string>();

        builder.Use(next => async request =>
        {
            executionOrder.Add("Middleware");
            var response = await next(request);
            return response;
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
        {
            executionOrder.Add("Terminal");
            return Task.FromResult(new HttpResponse(200, [], "Done"));
        };

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        await pipeline(request);

        // Assert
        executionOrder.Should().Equal("Middleware", "Terminal");
    }

    [Fact]
    public async Task Build_MultipleMiddleware_ExecutesInCorrectOrder()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        var executionOrder = new List<string>();

        builder.Use(next => async request =>
        {
            executionOrder.Add("First");
            var response = await next(request);
            executionOrder.Add("First-After");
            return response;
        });

        builder.Use(next => async request =>
        {
            executionOrder.Add("Second");
            var response = await next(request);
            executionOrder.Add("Second-After");
            return response;
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
        {
            executionOrder.Add("Terminal");
            return Task.FromResult(new HttpResponse(200, [], "Done"));
        };

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        await pipeline(request);

        // Assert
        // First middleware added should execute first (wraps outer)
        executionOrder.Should().Equal("First", "Second", "Terminal", "Second-After", "First-After");
    }

    [Fact]
    public async Task Build_MiddlewareCanShortCircuit_DoesNotCallNext()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        var terminalCalled = false;

        builder.Use(_ => _ =>
        {
            // Short-circuit - don't call next
            return Task.FromResult(new HttpResponse(401, [], "Unauthorized"));
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
        {
            terminalCalled = true;
            return Task.FromResult(new HttpResponse(200, [], "Done"));
        };

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        var response = await pipeline(request);

        // Assert
        terminalCalled.Should().BeFalse();
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Build_MiddlewareCanModifyRequest_ModificationPersists()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        string? capturedPath = null;

        builder.Use(next => async request =>
        {
            request.Path = "/modified";
            return await next(request);
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = request =>
        {
            capturedPath = request.Path;
            return Task.FromResult(new HttpResponse(200, [], "Done"));
        };

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/original", [], "");

        // Act
        await pipeline(request);

        // Assert
        capturedPath.Should().Be("/modified");
    }

    [Fact]
    public async Task Build_MiddlewareCanModifyResponse_ModificationReturned()
    {
        // Arrange
        var builder = new MiddlewareBuilder();

        builder.Use(next => async request =>
        {
            var response = await next(request);
            response.Headers["X-Modified"] = "true";
            return response;
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
            Task.FromResult(new HttpResponse(200, [], "Done"));

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        var response = await pipeline(request);

        // Assert
        response.Headers.Should().ContainKey("X-Modified");
        response.Headers["X-Modified"].Should().Be("true");
    }

    [Fact]
    public async Task Build_ReturnsNewPipelineEachTime()
    {
        // Arrange
        var builder = new MiddlewareBuilder();
        builder.Use(next => next); // Pass-through middleware

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
            Task.FromResult(new HttpResponse(200, new Dictionary<string, string>(), "Done"));

        // Act
        var pipeline1 = builder.Build(terminal);
        var pipeline2 = builder.Build(terminal);

        // Assert - Build is idempotent; both pipelines should work identically
        pipeline1.Should().NotBeNull();
        pipeline2.Should().NotBeNull();
        // Both should produce the same result since middleware is preserved
        var request = new HttpRequest("GET", "/test", new Dictionary<string, string>(), "");
        var result1 = await pipeline1(request);
        var result2 = await pipeline2(request);
        result1.Body.Should().Be("Done");
        result2.Body.Should().Be("Done");
    }

    [Fact]
    public async Task Build_MiddlewareCanHandleExceptions()
    {
        // Arrange
        var builder = new MiddlewareBuilder();

        builder.Use(next => async request =>
        {
            try
            {
                return await next(request);
            }
            catch (Exception ex)
            {
                return new HttpResponse(500, [], $"Error: {ex.Message}");
            }
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
            throw new InvalidOperationException("Test error");

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        var response = await pipeline(request);

        // Assert
        response.StatusCode.Should().Be(500);
        response.Body.Should().Contain("Test error");
    }

    [Fact]
    public async Task Build_MiddlewareCanAddHeaders_HeadersArePreserved()
    {
        // Arrange
        var builder = new MiddlewareBuilder();

        builder.Use(next => async request =>
        {
            var response = await next(request);
            response.Headers["X-Request-Id"] = "12345";
            response.Headers["X-Processing-Time"] = "100ms";
            return response;
        });

        Func<HttpRequest, Task<HttpResponse>> terminal = _ =>
            Task.FromResult(new HttpResponse(200, new Dictionary<string, string> { ["Content-Type"] = "application/json" }, "{}"));

        var pipeline = builder.Build(terminal);
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        var response = await pipeline(request);

        // Assert
        response.Headers.Should().HaveCount(3);
        response.Headers["Content-Type"].Should().Be("application/json");
        response.Headers["X-Request-Id"].Should().Be("12345");
        response.Headers["X-Processing-Time"].Should().Be("100ms");
    }

    [Fact]
    public void Use_ReturnsBuilder_AllowsMethodChaining()
    {
        // Arrange
        var builder = new MiddlewareBuilder();

        // Act
        var result = builder
            .Use(next => next)
            .Use(next => next)
            .Use(next => next);

        // Assert
        result.Should().BeSameAs(builder);
    }
}
