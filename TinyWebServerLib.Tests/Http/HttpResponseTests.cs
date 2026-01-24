using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Http;

/// <summary>
/// Unit tests for the <see cref="HttpResponse"/> class.
/// </summary>
public class HttpResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var statusCode = 200;
        var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };
        var body = "{\"message\":\"success\"}";

        // Act
        var response = new HttpResponse(statusCode, headers, body);

        // Assert
        response.StatusCode.Should().Be(statusCode);
        response.Headers.Should().BeEquivalentTo(headers);
        response.Body.Should().Be(body);
    }

    [Fact]
    public void Constructor_WithNullHeaders_SetsEmptyDictionary()
    {
        // Act
        var response = new HttpResponse(200, null!, "");

        // Assert
        response.Headers.Should().NotBeNull();
        response.Headers.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullBody_SetsEmptyString()
    {
        // Act
        var response = new HttpResponse(200, [], null!);

        // Assert
        response.Body.Should().BeEmpty();
    }

    [Fact]
    public void StatusCode_CanBeModified()
    {
        // Arrange
        var response = new HttpResponse(200, [], "");

        // Act
        response.StatusCode = 404;

        // Assert
        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Headers_CanBeModified()
    {
        // Arrange
        var response = new HttpResponse(200, new Dictionary<string, string>(), "");

        // Act
        response.Headers["X-Custom"] = "value";

        // Assert
        response.Headers.Should().ContainKey("X-Custom");
        response.Headers["X-Custom"].Should().Be("value");
    }

    [Fact]
    public void Body_CanBeModified()
    {
        // Arrange
        var response = new HttpResponse(200, [], "");

        // Act
        response.Body = "New Body";

        // Assert
        response.Body.Should().Be("New Body");
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    public void Constructor_AcceptsVariousStatusCodes(int statusCode)
    {
        // Act
        var response = new HttpResponse(statusCode, [], "");

        // Assert
        response.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void Headers_CanContainMultipleValues()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Content-Length"] = "100",
            ["Cache-Control"] = "no-cache"
        };

        // Act
        var response = new HttpResponse(200, headers, "");

        // Assert
        response.Headers.Should().HaveCount(3);
    }
}
