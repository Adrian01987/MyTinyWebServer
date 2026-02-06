using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Http;

/// <summary>
/// Unit tests for the <see cref="HttpSerializer"/> class.
/// </summary>
public class HttpSerializerTests
{
  [Fact]
  public void Serialize_Response200_ContainsCorrectStatusLine()
  {
    // Arrange
    var response = new HttpResponse(200, [], "Hello World");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().StartWith("HTTP/1.1 200 OK");
  }

  [Fact]
  public void Serialize_Response201_ContainsCorrectStatusLine()
  {
    // Arrange
    var response = new HttpResponse(201, [], "Created");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().StartWith("HTTP/1.1 201 Created");
  }

  [Fact]
  public void Serialize_Response400_ContainsCorrectStatusLine()
  {
    // Arrange
    var response = new HttpResponse(400, [], "Bad Request");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().StartWith("HTTP/1.1 400 Bad Request");
  }

  [Fact]
  public void Serialize_Response404_ContainsCorrectStatusLine()
  {
    // Arrange
    var response = new HttpResponse(404, [], "Not Found");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().StartWith("HTTP/1.1 404 Not Found");
  }

  [Fact]
  public void Serialize_ResponseWithHeaders_ContainsAllHeaders()
  {
    // Arrange
    var headers = new Dictionary<string, string>
    {
      ["Content-Type"] = "application/json",
      ["X-Custom-Header"] = "custom-value"
    };
    var response = new HttpResponse(200, headers, "{}");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().Contain("Content-Type: application/json");
    result.Should().Contain("X-Custom-Header: custom-value");
  }

  [Fact]
  public void Serialize_ResponseWithBody_ContainsBody()
  {
    // Arrange
    var body = "{\"id\":1,\"name\":\"Test\"}";
    var response = new HttpResponse(200, [], body);

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().EndWith(body);
  }

  [Fact]
  public void Serialize_ResponseWithEmptyBody_EndsWithEmptyLine()
  {
    // Arrange
    var response = new HttpResponse(200, [], "");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().Contain("\r\n\r\n");
  }

  [Theory]
  [InlineData(200, "OK")]
  [InlineData(201, "Created")]
  [InlineData(400, "Bad Request")]
  [InlineData(404, "Not Found")]
  public void Serialize_KnownStatusCodes_ReturnsCorrectReasonPhrase(int statusCode, string expectedPhrase)
  {
    // Arrange
    var response = new HttpResponse(statusCode, [], "");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().Contain($"HTTP/1.1 {statusCode} {expectedPhrase}");
  }

  [Fact]
  public void Serialize_UnknownStatusCode_ReturnsEmptyReasonPhrase()
  {
    // Arrange
    var response = new HttpResponse(418, [], "I'm a teapot");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().StartWith("HTTP/1.1 418 ");
  }

  [Fact]
  public void Serialize_CompleteResponse_HasCorrectStructure()
  {
    // Arrange
    var headers = new Dictionary<string, string>
    {
      ["Content-Type"] = "application/json"
    };
    var body = "{\"message\":\"success\"}";
    var response = new HttpResponse(200, headers, body);

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    var lines = result.Split("\r\n");
    lines[0].Should().Be("HTTP/1.1 200 OK");
    lines[1].Should().Be("Content-Type: application/json");
    // Content-Length is auto-added
    lines[2].Should().StartWith("Content-Length:");
    lines[3].Should().BeEmpty(); // Empty line separating headers from body
    lines[4].Should().Be(body);
  }

  [Fact]
  public void Serialize_ResponseWithMultipleHeaders_PreservesOrder()
  {
    // Arrange
    var headers = new Dictionary<string, string>
    {
      ["Content-Type"] = "application/json",
      ["Content-Length"] = "100",
      ["Cache-Control"] = "no-cache"
    };
    var response = new HttpResponse(200, headers, "");

    // Act
    var result = HttpSerializer.Serialize(response);

    // Assert
    result.Should().Contain("Content-Type: application/json");
    result.Should().Contain("Content-Length: 100");
    result.Should().Contain("Cache-Control: no-cache");
  }
}
