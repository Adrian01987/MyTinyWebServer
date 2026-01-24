using TinyWebServerLib.Http;
using TinyWebServerLib.Routing;

namespace TinyWebServerLib.Tests.Routing;

/// <summary>
/// Unit tests for the <see cref="Router"/> class.
/// </summary>
public class RouterTests
{
  [Fact]
  public async Task RouteAsync_MatchingRoute_ReturnsHandlerResponse()
  {
    // Arrange
    var router = new Router();
    var expectedResponse = new HttpResponse(200, [], "Success");
    router.Map("GET", "/api/test", _ => Task.FromResult(expectedResponse));

    var request = new HttpRequest("GET", "/api/test", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    response.Body.Should().Be("Success");
  }

  [Fact]
  public async Task RouteAsync_NoMatchingRoute_Returns404()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/test", _ => Task.FromResult(new HttpResponse(200, [], "Success")));

    var request = new HttpRequest("GET", "/api/nonexistent", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(404);
    response.Body.Should().Be("Not Found");
  }

  [Fact]
  public async Task RouteAsync_WrongHttpMethod_Returns404()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/test", _ => Task.FromResult(new HttpResponse(200, [], "Success")));

    var request = new HttpRequest("POST", "/api/test", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(404);
  }

  [Fact]
  public async Task RouteAsync_RouteWithParameter_ExtractsParameter()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/customers/{id}", request =>
    {
      var id = request.RouteParameters["id"];
      return Task.FromResult(new HttpResponse(200, [], $"Customer {id}"));
    });

    var request = new HttpRequest("GET", "/api/customers/123", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    response.Body.Should().Be("Customer 123");
    request.RouteParameters["id"].Should().Be("123");
  }

  [Fact]
  public async Task RouteAsync_RouteWithMultipleParameters_ExtractsAllParameters()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/customers/{customerId}/orders/{orderId}", request =>
    {
      var customerId = request.RouteParameters["customerId"];
      var orderId = request.RouteParameters["orderId"];
      return Task.FromResult(new HttpResponse(200, [], $"Customer {customerId}, Order {orderId}"));
    });

    var request = new HttpRequest("GET", "/api/customers/42/orders/99", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    request.RouteParameters["customerId"].Should().Be("42");
    request.RouteParameters["orderId"].Should().Be("99");
  }

  [Theory]
  [InlineData("GET")]
  [InlineData("POST")]
  [InlineData("PUT")]
  [InlineData("DELETE")]
  [InlineData("PATCH")]
  public async Task RouteAsync_DifferentHttpMethods_RoutesCorrectly(string method)
  {
    // Arrange
    var router = new Router();
    router.Map(method, "/api/test", _ => Task.FromResult(new HttpResponse(200, [], $"{method} Success")));

    var request = new HttpRequest(method, "/api/test", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    response.Body.Should().Be($"{method} Success");
  }

  [Fact]
  public async Task RouteAsync_CaseInsensitiveMethodMatching_RoutesCorrectly()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/test", _ => Task.FromResult(new HttpResponse(200, [], "Success")));

    var request = new HttpRequest("get", "/api/test", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
  }

  [Fact]
  public async Task RouteAsync_MultipleRoutes_MatchesCorrectOne()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/customers", _ => Task.FromResult(new HttpResponse(200, [], "All Customers")));
    router.Map("GET", "/api/orders", _ => Task.FromResult(new HttpResponse(200, [], "All Orders")));
    router.Map("POST", "/api/customers", _ => Task.FromResult(new HttpResponse(201, [], "Customer Created")));

    var request = new HttpRequest("GET", "/api/orders", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    response.Body.Should().Be("All Orders");
  }

  [Fact]
  public async Task RouteAsync_SamePathDifferentMethods_RoutesToCorrectHandler()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/customers", _ => Task.FromResult(new HttpResponse(200, [], "GET")));
    router.Map("POST", "/api/customers", _ => Task.FromResult(new HttpResponse(201, [], "POST")));

    var getRequest = new HttpRequest("GET", "/api/customers", [], "");
    var postRequest = new HttpRequest("POST", "/api/customers", [], "");

    // Act
    var getResponse = await router.RouteAsync(getRequest);
    var postResponse = await router.RouteAsync(postRequest);

    // Assert
    getResponse.Body.Should().Be("GET");
    postResponse.Body.Should().Be("POST");
  }

  [Fact]
  public void Map_RouteWithoutLeadingSlash_NormalizesRoute()
  {
    // Arrange
    var router = new Router();

    // Act - This should not throw
    router.Map("GET", "api/test", _ => Task.FromResult(new HttpResponse(200, [], "Success")));

    // Assert - Route should be accessible with leading slash
    var request = new HttpRequest("GET", "/api/test", [], "");
    var response = router.RouteAsync(request).Result;
    response.StatusCode.Should().Be(200);
  }

  [Fact]
  public async Task RouteAsync_RootPath_MatchesCorrectly()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/", _ => Task.FromResult(new HttpResponse(200, [], "Home")));

    var request = new HttpRequest("GET", "/", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(200);
    response.Body.Should().Be("Home");
  }

  [Fact]
  public async Task RouteAsync_PartialPathMatch_DoesNotMatch()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/customers", _ => Task.FromResult(new HttpResponse(200, [], "Success")));

    var request = new HttpRequest("GET", "/api/customers/extra", [], "");

    // Act
    var response = await router.RouteAsync(request);

    // Assert
    response.StatusCode.Should().Be(404);
  }

  [Fact]
  public async Task RouteAsync_HandlerThrowsException_PropagatesException()
  {
    // Arrange
    var router = new Router();
    router.Map("GET", "/api/error", _ => throw new InvalidOperationException("Test exception"));

    var request = new HttpRequest("GET", "/api/error", [], "");

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() => router.RouteAsync(request));
  }
}
