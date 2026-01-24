using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TinyWebServerLib.Extensions;
using TinyWebServerLib.Http;

namespace TinyWebServerLib.Tests.Extensions;

/// <summary>
/// Unit tests for the <see cref="TinyWebServerBuilder"/> class.
/// </summary>
public class TinyWebServerBuilderTests
{
  [Fact]
  public void Use_ReturnsBuilder_AllowsMethodChaining()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();

    // Act
    var result = builder
        .Use(next => next)
        .Use(next => next);

    // Assert
    result.Should().BeSameAs(builder);
  }

  [Fact]
  public void Map_ReturnsBuilder_AllowsMethodChaining()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();

    // Act
    var result = builder
        .Map("GET", "/test1", _ => Task.FromResult(new HttpResponse(200, [], "")))
        .Map("POST", "/test2", _ => Task.FromResult(new HttpResponse(200, [], "")));

    // Assert
    result.Should().BeSameAs(builder);
  }

  [Fact]
  public void UseUrl_ReturnsBuilder_AllowsMethodChaining()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();

    // Act
    var result = builder.UseUrl(IPAddress.Loopback, 8080);

    // Assert
    result.Should().BeSameAs(builder);
  }

  [Fact]
  public void UseServiceProvider_ReturnsBuilder_AllowsMethodChaining()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();
    var services = new ServiceCollection().BuildServiceProvider();

    // Act
    var result = builder.UseServiceProvider(services);

    // Assert
    result.Should().BeSameAs(builder);
  }

  [Fact]
  public void Build_ReturnsServer()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();
    builder.UseUrl(IPAddress.Loopback, 0);

    // Act
    var server = builder.Build();

    // Assert
    server.Should().NotBeNull();
  }

  [Fact]
  public void Build_WithAllConfiguration_ReturnsServer()
  {
    // Arrange
    var services = new ServiceCollection().BuildServiceProvider();
    var builder = new TinyWebServerBuilder();

    builder
        .UseUrl(IPAddress.Loopback, 0)
        .UseServiceProvider(services)
        .Use(next => next)
        .Map("GET", "/test", _ => Task.FromResult(new HttpResponse(200, [], "")));

    // Act
    var server = builder.Build();

    // Assert
    server.Should().NotBeNull();
  }

  [Fact]
  public void FluentApi_AllMethodsChainable()
  {
    // Arrange & Act
    var services = new ServiceCollection().BuildServiceProvider();

    var server = new TinyWebServerBuilder()
        .UseUrl(IPAddress.Loopback, 0)
        .UseServiceProvider(services)
        .Use(next => async request =>
        {
          request.Headers["X-Processed"] = "true";
          return await next(request);
        })
        .Map("GET", "/", _ => Task.FromResult(new HttpResponse(200, [], "Home")))
        .Map("GET", "/api/test", _ => Task.FromResult(new HttpResponse(200, [], "Test")))
        .Map("POST", "/api/data", _ => Task.FromResult(new HttpResponse(201, [], "Created")))
        .Build();

    // Assert
    server.Should().NotBeNull();
  }
}
