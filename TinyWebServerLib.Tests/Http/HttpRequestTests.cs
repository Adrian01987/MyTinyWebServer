using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Http;

/// <summary>
/// Unit tests for the <see cref="HttpRequest"/> class.
/// </summary>
public class HttpRequestTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var method = "GET";
        var path = "/api/test";
        var headers = new Dictionary<string, string> { ["Host"] = "localhost" };
        var body = "test body";

        // Act
        var request = new HttpRequest(method, path, headers, body);

        // Assert
        request.Method.Should().Be(method);
        request.Path.Should().Be(path);
        request.Headers.Should().BeEquivalentTo(headers);
        request.Body.Should().Be(body);
    }

    [Fact]
    public void Constructor_WithNullMethod_SetsEmptyString()
    {
        // Act
        var request = new HttpRequest(null!, "/test", [], "");

        // Assert
        request.Method.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullPath_SetsEmptyString()
    {
        // Act
        var request = new HttpRequest("GET", null!, [], "");

        // Assert
        request.Path.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullHeaders_SetsEmptyDictionary()
    {
        // Act
        var request = new HttpRequest("GET", "/test", null!, "");

        // Assert
        request.Headers.Should().NotBeNull();
        request.Headers.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullBody_SetsEmptyString()
    {
        // Act
        var request = new HttpRequest("GET", "/test", [], null!);

        // Assert
        request.Body.Should().BeEmpty();
    }

    [Fact]
    public void RouteParameters_DefaultsToEmptyDictionary()
    {
        // Act
        var request = new HttpRequest("GET", "/test", [], "");

        // Assert
        request.RouteParameters.Should().NotBeNull();
        request.RouteParameters.Should().BeEmpty();
    }

    [Fact]
    public void RouteParameters_CanBeModified()
    {
        // Arrange
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        request.RouteParameters["id"] = 123;
        request.RouteParameters["name"] = "test";

        // Assert
        request.RouteParameters.Should().HaveCount(2);
        request.RouteParameters["id"].Should().Be(123);
        request.RouteParameters["name"].Should().Be("test");
    }

    [Fact]
    public void Method_CanBeModified()
    {
        // Arrange
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        request.Method = "POST";

        // Assert
        request.Method.Should().Be("POST");
    }

    [Fact]
    public void Path_CanBeModified()
    {
        // Arrange
        var request = new HttpRequest("GET", "/original", [], "");

        // Act
        request.Path = "/modified";

        // Assert
        request.Path.Should().Be("/modified");
    }

    [Fact]
    public void Headers_CanBeModified()
    {
        // Arrange
        var request = new HttpRequest("GET", "/test", new Dictionary<string, string>(), "");

        // Act
        request.Headers["Content-Type"] = "application/json";

        // Assert
        request.Headers.Should().ContainKey("Content-Type");
        request.Headers["Content-Type"].Should().Be("application/json");
    }

    [Fact]
    public void Body_CanBeModified()
    {
        // Arrange
        var request = new HttpRequest("GET", "/test", [], "");

        // Act
        request.Body = "{\"key\":\"value\"}";

        // Assert
        request.Body.Should().Be("{\"key\":\"value\"}");
    }
}
