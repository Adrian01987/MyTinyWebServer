using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using TinyWebServerLib.Attributes;
using TinyWebServerLib.Extensions;
using TinyWebServerLib.Http;

namespace TinyWebServerLib.Mapper;

/// <summary>
/// Provides extension methods for automatically mapping controller classes to routes.
/// Uses reflection to discover action methods and their HTTP attributes.
/// </summary>
public static class ControllerMapper
{
    /// <summary>
    /// Maps all action methods from a controller class to the server's routing table.
    /// Automatically handles model binding for route parameters and request bodies.
    /// </summary>
    /// <typeparam name="T">The controller type, which must be decorated with <see cref="ApiControllerAttribute"/>.</typeparam>
    /// <param name="builder">The server builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static TinyWebServerBuilder MapController<T>(this TinyWebServerBuilder builder) where T : class
    {
        var controllerType = typeof(T);
        if (controllerType.GetCustomAttribute<ApiControllerAttribute>() == null)
        {
            return builder; // Or throw an exception
        }

        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute));

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<HttpMethodAttribute>();
            if (attribute != null)
            {
                builder.Map(attribute.HttpMethod, attribute.Route, async request =>
                {
                    var controller = request.RequestServices.GetRequiredService<T>();

                    // Model Binding Logic
                    var parameters = method.GetParameters();
                    var arguments = new object?[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        if (request.RouteParameters.TryGetValue(param.Name!, out var routeValue))
                        {
                            arguments[i] = Convert.ChangeType(routeValue, param.ParameterType);
                        }
                        else if (param.ParameterType == typeof(HttpRequest))
                        {
                            arguments[i] = request;
                        }
                        else if (request.Body.Length > 0 && (attribute.HttpMethod == "POST" || attribute.HttpMethod == "PUT"))
                        {
                            try
                            {
                                arguments[i] = JsonSerializer.Deserialize(request.Body, param.ParameterType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            catch
                            {
                                // Could lead to a 400 Bad Request response
                                arguments[i] = null;
                            }
                        }
                    }

                    var result = method.Invoke(controller, arguments);
                    if (result is Task<HttpResponse> task)
                    {
                        return await task;
                    }
                    throw new InvalidOperationException("Controller action must return a Task<HttpResponse>.");
                });
            }
        }

        return builder;
    }
}
