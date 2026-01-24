# MyTinyWebServer: An Educational Web Server in C#

Welcome to `MyTinyWebServer`! This project is a lightweight, from-scratch web server built using modern C# and .NET 8. It is designed to be an educational tool that demonstrates the core concepts behind web frameworks like ASP.NET Core, including middleware, routing, dependency injection, and model binding.

## Project Structure

The solution is divided into four distinct projects, each with a clear responsibility:

- **`MyTinyWebServer`**: The main executable project that hosts and runs the web server. It composes the services and wires up the application.
- **`TinyWebServerLib`**: The core web server library. It contains all the essential components for handling HTTP requests, routing, and managing the server lifecycle.
- **`CustomerApi`**: A sample API project containing a `CustomersController`. It demonstrates how to build an API using the `TinyWebServerLib` framework.
- **`TinyLogger`**: A reusable, aspect-oriented logging library. It uses `Castle.Core`'s DynamicProxy to intercept method calls and provide automatic logging, demonstrating powerful metaprogramming concepts.

## Core Concepts & Design Patterns

This project is built on several key architectural patterns that are fundamental to modern web development.

### 1. The Builder Pattern (`TinyWebServerBuilder`)

The server is configured and created using a **builder pattern**. The `TinyWebServerBuilder` provides a fluent API to chain configuration calls for setting up the server's URL, registering middleware, and mapping controllers.

```csharp
// In Program.cs
var services = new ServiceCollection();
services.AddLogging()
        .AddProxiedControllers(typeof(CustomersController).Assembly);
var serviceProvider = services.BuildServiceProvider();

var builder = new TinyWebServerBuilder();
builder.UseServiceProvider(serviceProvider)
       .Use(loggingMiddleware)
       .MapController<CustomersController>()
       .UseUrl(IPAddress.Any, 4221);

TinyWebServer server = builder.Build();
```

### 2. Middleware Pipeline

The server features a middleware pipeline, just like in ASP.NET Core. Each middleware component is a function that processes a request and can either short-circuit the pipeline or pass the request to the next component (`next`).

Middleware is registered with the `Use` method. This example shows a simple request logging middleware:

```csharp
builder.Use(next => async request =>
{
    Console.WriteLine($"{request.Method} {request.Path}");
    return await next(request); // Pass to the next middleware
});
```

### 3. Routing Engine

The `Router` is responsible for mapping an incoming HTTP request's method and path to a specific handler function. It supports **parameterized routes** (e.g., `/customers/{id}`) by compiling route templates into regular expressions to extract values.

### 4. Dependency Injection (DI)

The server integrates with `Microsoft.Extensions.DependencyInjection`.

- **Request Scope:** For every incoming HTTP request, a new DI **scope** is created. This ensures that "scoped" services (like controllers) are created once per request and disposed of afterward.
- **Service Availability:** The request-specific `IServiceProvider` is attached to the `HttpRequest` object, making it available throughout the request pipeline for use by the framework.

### 5. Attribute-Based Routing & Controller Mapping

Controllers and their actions are mapped using attributes, which provides a declarative and clean way to define your API endpoints.

- `[ApiController]`: A class-level attribute that marks a class as a controller.
- `[HttpGet("...")]` & `[HttpPost("...")]`: Method-level attributes that map an action to an HTTP method and route template.

The `ControllerMapper` uses reflection to find these attributes and wire up the routes automatically.

### 6. Model Binding

To keep controllers clean and focused on business logic, the framework provides automatic **model binding**. The `ControllerMapper` inspects the parameters of a controller action and automatically populates them from the request:

- **From Route:** Parameters like `int id` are matched with route values from the URL (e.g., `/customers/123`).
- **From Body:** A complex object parameter (e.g., `Customer customer`) is automatically deserialized from the JSON request body in a POST or PUT request.

This transforms a controller action from this:

```csharp
// Before: Manual parsing
public Task<HttpResponse> GetCustomerById(HttpRequest request)
{
    if (!request.RouteParameters.TryGetValue("id", out var idValue) || !int.TryParse(idValue.ToString(), out var id))
    {
        // ... handle error
    }
    // ...
}
```

...into the much cleaner and more expressive:

```csharp
// After: Automatic model binding
public Task<HttpResponse> GetCustomerById(int id)
{
    // The 'id' parameter is already parsed and available.
    var customer = new Customer(id, $"Customer {id}");
    // ...
}
```

### 7. Aspect-Oriented Programming (AOP) for Logging

The `TinyLogger` project is a powerful example of AOP. It uses `Castle.Core`'s `ProxyGenerator` to create a dynamic proxy around a registered service.

- An `IInterceptor` (`LoggerInterceptor`) is attached to this proxy.
- When a method on the controller is called, the interceptor's `Intercept` method is invoked first, allowing us to automatically log method entry, exit, arguments, and exceptions without adding a single line of logging code to the controller itself.
- Registration is handled by convention using the `AddProxiedControllers` extension method, which finds all classes marked with `[ApiController]` in an assembly and applies the logging proxy.

---

## Anatomy of a Request

Here is a step-by-step walkthrough of what happens when a `POST /customers` request with a JSON body hits the server:

1.  **Connection Accepted:** The `TinyWebServer`'s `TcpListener` accepts an incoming `TcpClient` connection.
2.  **Request Handling Begins:** A new task is fired off to run `HandleClientAsync` to process the request without blocking the listener.
3.  **DI Scope Created:** A new dependency injection scope is created for this specific request (`serviceProvider.CreateAsyncScope()`). This ensures any scoped services live only for the duration of this request.
4.  **Request Parsing:** The server reads from the `NetworkStream` line-by-line to parse the HTTP headers. It then reads the request body based on the `Content-Length` header. The raw text is parsed into a structured `HttpRequest` object.
5.  **Middleware Execution:** The `HttpRequest` is passed to the first middleware in the pipeline. Each middleware runs and calls `next()` to pass the request down the chain.
6.  **Routing:** The final "middleware" in the pipeline is the `Router`. It matches the request's method (`POST`) and path (`/customers`) to the handler that was registered by the `ControllerMapper`.
7.  **Controller Resolution:** The handler, created by `MapController`, is invoked. It uses the request's `IServiceProvider` (`request.RequestServices`) to get an instance of `CustomersController`.
    - Because controllers were registered via the `AddProxiedControllers` extension method, the DI container doesn't return a direct instance. Instead, it returns a **proxy** with the `LoggerInterceptor` attached.
8.  **Model Binding:** Before invoking the controller action, the `ControllerMapper`'s logic inspects the target method (`CreateCustomer(Customer customer)`). It sees the `Customer` parameter and uses `JsonSerializer` to deserialize the `request.Body` into a `Customer` object.
9.  **AOP Interception & Action Execution:**
    - The call to `CreateCustomer` is intercepted by `LoggerInterceptor`, which logs "Calling CreateCustomer...".
    - The actual `CreateCustomer` method on the real controller is invoked with the model-bound `Customer` object.
    - The method runs its validation and business logic, returning a `Task<HttpResponse>`.
    - The interceptor's async handling logic logs the successful completion of the method after the `Task` finishes.
10. **Response Generation:** The `HttpResponse` object (e.g., with status code 201) is returned up the call stack.
11. **Response Serialization:** The `HandleClientAsync` method serializes the `HttpResponse` object into a raw HTTP response string (status line, headers, and body).
12. **Sending the Response:** The response string is converted to bytes and written back to the client's `NetworkStream`.
13. **Cleanup:** The `try-finally` block ensures the `TcpClient` is closed. The `await using` statements on the DI scope and `NetworkStream` ensure they are properly disposed of, cleaning up all resources for the request.

## How to Run

1.  Open the solution in Visual Studio.
2.  Set `MyTinyWebServer` as the startup project.
3.  Press F5 or click the "Run" button.
4.  The console will indicate that the server is running. You can now send requests to it using a tool like Postman, curl, or a web browser.
    - `GET http://localhost:4221/customers`
    - `GET http://localhost:4221/customers/123`
    - `POST http://localhost:4221/customers` (with a JSON body like `{"id": 10, "name": "New Customer"}`)

## Technologies Used

- **.NET 8** - Target framework
- **Castle.Core** - Dynamic proxy generation for AOP
- **Microsoft.Extensions.DependencyInjection** - Dependency injection container
- **Microsoft.Extensions.Logging** - Logging abstractions

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This project is intended for **educational purposes only**. It is not designed or tested for production use. Use it to learn about web server internals, middleware patterns, and metaprogramming concepts in C#.
