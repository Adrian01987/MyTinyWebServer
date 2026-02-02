using Microsoft.Extensions.DependencyInjection;
using TinyWebServerLib.Attributes;
using TinyWebServerLib.Extensions;
using TinyWebServerLib.Http;
using TinyWebServerLib.Mapper;

namespace TinyWebServerLib.Tests.Mapper;

/// <summary>
/// Unit tests for the <see cref="ControllerMapper"/> class.
/// </summary>
public class ControllerMapperTests
{
  [ApiController]
  public class TestController
  {
    [HttpGet("test")]
    public Task<HttpResponse> GetTest()
    {
      return Task.FromResult(new HttpResponse(200, [], "Test"));
    }

    [HttpGet("test/{id}")]
    public Task<HttpResponse> GetById(int id)
    {
      return Task.FromResult(new HttpResponse(200, [], $"Id: {id}"));
    }

    [HttpPost("test")]
    public Task<HttpResponse> Create(TestModel model)
    {
      return Task.FromResult(new HttpResponse(201, [], $"Created: {model.Name}"));
    }

    [HttpPut("test/{id}")]
    public Task<HttpResponse> Update(int id, TestModel model)
    {
      return Task.FromResult(new HttpResponse(200, [], $"Updated {id}: {model.Name}"));
    }

    [HttpDelete("test/{id}")]
    public Task<HttpResponse> Delete(int id)
    {
      return Task.FromResult(new HttpResponse(204, [], string.Empty));
    }
  }

  public class TestModel
  {
    public string Name { get; set; } = string.Empty;
  }

  public class NotAController
  {
    [HttpGet("ignored")]
    public Task<HttpResponse> Ignored() => Task.FromResult(new HttpResponse(200, [], ""));
  }

  [Fact]
  public void MapController_WithApiControllerAttribute_ReturnsBuilder()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<TestController>();
    var serviceProvider = services.BuildServiceProvider();

    var builder = new TinyWebServerBuilder();
    builder.UseServiceProvider(serviceProvider);

    // Act
    var result = builder.MapController<TestController>();

    // Assert
    Assert.Same(builder, result);
  }

  [Fact]
  public void MapController_WithoutApiControllerAttribute_ReturnsBuilderWithoutMapping()
  {
    // Arrange
    var builder = new TinyWebServerBuilder();

    // Act - Should not throw, just return builder
    var result = builder.MapController<NotAController>();

    // Assert
    Assert.Same(builder, result);
  }

  [Fact]
  public void MapController_WithApiControllerAttribute_BuildsServer()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<TestController>();
    var serviceProvider = services.BuildServiceProvider();

    var builder = new TinyWebServerBuilder();
    builder.UseServiceProvider(serviceProvider);
    builder.MapController<TestController>();

    // Act
    var server = builder.Build();

    // Assert
    Assert.NotNull(server);
  }

  [Fact]
  public void MapController_WithMultipleHttpMethods_BuildsSuccessfully()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<TestController>();
    var serviceProvider = services.BuildServiceProvider();

    var builder = new TinyWebServerBuilder();
    builder.UseServiceProvider(serviceProvider);

    // Act - Controller has GET, POST, PUT, DELETE methods
    builder.MapController<TestController>();
    var server = builder.Build();

    // Assert
    Assert.NotNull(server);
  }
}
