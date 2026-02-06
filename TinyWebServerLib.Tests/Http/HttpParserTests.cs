using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Http;

/// <summary>
/// Unit tests for the <see cref="HttpParser"/> class.
/// </summary>
public class HttpParserTests
{
    [Fact]
    public void Parse_ValidGetRequest_ReturnsCorrectMethod()
    {
        // Arrange
        var requestText = "GET /api/customers HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Method.Should().Be("GET");
    }

    [Fact]
    public void Parse_ValidGetRequest_ReturnsCorrectPath()
    {
        // Arrange
        var requestText = "GET /api/customers HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Path.Should().Be("/api/customers");
    }

    [Fact]
    public void Parse_ValidPostRequest_ReturnsCorrectMethod()
    {
        // Arrange
        var requestText = "POST /api/customers HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\n\r\n{\"name\":\"John\"}";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Method.Should().Be("POST");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void Parse_DifferentHttpMethods_ReturnsCorrectMethod(string method)
    {
        // Arrange
        var requestText = $"{method} /api/test HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Method.Should().Be(method);
    }

    [Fact]
    public void Parse_RequestWithHeaders_ParsesAllHeaders()
    {
        // Arrange
        var requestText = "GET /api/customers HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\nAuthorization: Bearer token123\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Headers.Should().HaveCount(3);
        request.Headers["Host"].Should().Be("localhost");
        request.Headers["Content-Type"].Should().Be("application/json");
        request.Headers["Authorization"].Should().Be("Bearer token123");
    }

    [Fact]
    public void Parse_RequestWithBody_ReturnsEmptyBody()
    {
        // Arrange - Body parsing is handled by the server via Content-Length,
        // not by the parser. The parser returns an empty body.
        var body = "{\"id\":1,\"name\":\"Test Customer\"}";
        var requestText = $"POST /api/customers HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\n\r\n{body}";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Body.Should().BeEmpty();
    }

    [Fact]
    public void Parse_RequestWithoutBody_ReturnsEmptyBody()
    {
        // Arrange
        var requestText = "GET /api/customers HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Body.Should().BeEmpty();
    }

    [Fact]
    public void Parse_RequestWithMultilineBody_ReturnsEmptyBody()
    {
        // Arrange - Body parsing is handled by the server, not the parser
        var body = "line1\r\nline2\r\nline3";
        var requestText = $"POST /api/test HTTP/1.1\r\nHost: localhost\r\n\r\n{body}";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Body.Should().BeEmpty();
    }

    [Fact]
    public void Parse_PathWithQueryString_PreservesQueryString()
    {
        // Arrange
        var requestText = "GET /api/customers?name=john&active=true HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Path.Should().Be("/api/customers?name=john&active=true");
    }

    [Fact]
    public void Parse_RootPath_ReturnsSlash()
    {
        // Arrange
        var requestText = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Path.Should().Be("/");
    }

    [Fact]
    public void Parse_ValidRequest_InitializesEmptyRouteParameters()
    {
        // Arrange
        var requestText = "GET /api/customers HTTP/1.1\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.RouteParameters.Should().NotBeNull();
        request.RouteParameters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptyRequest_ThrowsHttpParseException()
    {
        // Arrange
        var requestText = "";

        // Act
        Action act = () => HttpParser.Parse(requestText);

        // Assert
        act.Should().Throw<HttpParseException>()
            .WithMessage("Empty request");
    }

    [Fact]
    public void Parse_WhitespaceOnlyRequest_ThrowsHttpParseException()
    {
        // Arrange
        var requestText = "   \r\n  ";

        // Act
        Action act = () => HttpParser.Parse(requestText);

        // Assert
        act.Should().Throw<HttpParseException>()
            .WithMessage("Empty request");
    }

    [Fact]
    public void Parse_InvalidRequestLine_ThrowsHttpParseException()
    {
        // Arrange
        var requestText = "INVALID\r\n\r\n";

        // Act
        Action act = () => HttpParser.Parse(requestText);

        // Assert
        act.Should().Throw<HttpParseException>()
            .WithMessage("Invalid request line: 'INVALID'");
    }

    [Fact]
    public void Parse_NullRequest_ThrowsHttpParseException()
    {
        // Act
        Action act = () => HttpParser.Parse(null!);

        // Assert
        act.Should().Throw<HttpParseException>();
    }

    [Fact]
    public void Parse_MalformedHeader_SkipsHeader()
    {
        // Arrange - Header without ": " separator
        var requestText = "GET /test HTTP/1.1\r\nBadHeader\r\nHost: localhost\r\n\r\n";

        // Act
        var request = HttpParser.Parse(requestText);

        // Assert
        request.Headers.Should().ContainKey("Host");
        request.Headers.Should().NotContainKey("BadHeader");
    }
}
