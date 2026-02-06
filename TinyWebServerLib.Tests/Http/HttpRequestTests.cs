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
    public void Constructor_WithNullMethod_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new HttpRequest(null!, "/test", new Dictionary<string, string>(), "");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("method");
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new HttpRequest("GET", null!, new Dictionary<string, string>(), "");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("path");
    }

    [Fact]
    public void Constructor_WithNullHeaders_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new HttpRequest("GET", "/test", null!, "");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("headers");
    }

    [Fact]
    public void Constructor_WithNullBody_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new HttpRequest("GET", "/test", new Dictionary<string, string>(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("body");
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

    [Fact]
    public void PathBase_WithoutQueryString_ReturnsFullPath()
    {
        // Arrange
        var request = new HttpRequest("GET", "/customers/123", [], "");

        // Act & Assert
        request.PathBase.Should().Be("/customers/123");
    }

    [Fact]
    public void PathBase_WithQueryString_ReturnsPathWithoutQuery()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?q=hello&page=1", [], "");

        // Act & Assert
        request.PathBase.Should().Be("/search");
    }

    [Fact]
    public void QueryParameters_WithNoQueryString_ReturnsEmptyDictionary()
    {
        // Arrange
        var request = new HttpRequest("GET", "/customers", [], "");

        // Act & Assert
        request.QueryParameters.Should().BeEmpty();
    }

    [Fact]
    public void QueryParameters_WithSingleParameter_ParsesCorrectly()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?q=hello", [], "");

        // Act & Assert
        request.QueryParameters.Should().ContainKey("q");
        request.QueryParameters["q"].Should().Be("hello");
    }

    [Fact]
    public void QueryParameters_WithMultipleParameters_ParsesAll()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?q=hello&page=2&sort=desc", [], "");

        // Act & Assert
        request.QueryParameters.Should().HaveCount(3);
        request.QueryParameters["q"].Should().Be("hello");
        request.QueryParameters["page"].Should().Be("2");
        request.QueryParameters["sort"].Should().Be("desc");
    }

    [Fact]
    public void QueryParameters_WithEncodedValues_DecodesCorrectly()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?q=hello%20world&name=John%26Jane", [], "");

        // Act & Assert
        request.QueryParameters["q"].Should().Be("hello world");
        request.QueryParameters["name"].Should().Be("John&Jane");
    }

    [Fact]
    public void QueryParameters_WithEmptyValue_ReturnsEmptyString()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?flag=&name=test", [], "");

        // Act & Assert
        request.QueryParameters["flag"].Should().BeEmpty();
        request.QueryParameters["name"].Should().Be("test");
    }

    [Fact]
    public void QueryParameters_WithNoValue_ReturnsEmptyString()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?flag", [], "");

        // Act & Assert
        request.QueryParameters["flag"].Should().BeEmpty();
    }

    [Fact]
    public void QueryParameters_IsCachedOnSubsequentAccess()
    {
        // Arrange
        var request = new HttpRequest("GET", "/search?q=test", [], "");

        // Act
        var first = request.QueryParameters;
        var second = request.QueryParameters;

        // Assert - Should return the same dictionary instance
        first.Should().BeSameAs(second);
    }
}
