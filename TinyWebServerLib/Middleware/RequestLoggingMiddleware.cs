using System.Diagnostics;
using TinyWebServerLib.Http;

namespace TinyWebServerLib.Middleware;

/// <summary>
/// Provides middleware for logging HTTP requests and responses.
/// This is an educational implementation demonstrating request timing and logging patterns.
/// </summary>
public static class RequestLoggingMiddleware
{
  /// <summary>
  /// Creates a middleware that logs request and response information to the console.
  /// </summary>
  /// <returns>A middleware function that can be added to the pipeline.</returns>
  /// <example>
  /// <code>
  /// builder.Use(RequestLoggingMiddleware.Create());
  /// </code>
  /// </example>
  public static Func<Func<HttpRequest, Task<HttpResponse>>, Func<HttpRequest, Task<HttpResponse>>> Create()
  {
    return next => async request =>
    {
      var stopwatch = Stopwatch.StartNew();
      var requestId = Guid.NewGuid().ToString("N")[..8];
      var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

      Console.WriteLine($"[{timestamp}] [{requestId}] --> {request.Method} {request.Path}");

      try
      {
        var response = await next(request);
        stopwatch.Stop();

        var statusEmoji = response.StatusCode switch
        {
          >= 200 and < 300 => "✓",
          >= 300 and < 400 => "→",
          >= 400 and < 500 => "✗",
          _ => "!"
        };

        Console.WriteLine($"[{timestamp}] [{requestId}] <-- {statusEmoji} {response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");

        return response;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        Console.WriteLine($"[{timestamp}] [{requestId}] <-- ! ERROR: {ex.Message} ({stopwatch.ElapsedMilliseconds}ms)");
        throw;
      }
    };
  }

  /// <summary>
  /// Creates a middleware that logs detailed request and response information.
  /// Includes headers and body information for debugging purposes.
  /// </summary>
  /// <param name="logHeaders">Whether to log request and response headers.</param>
  /// <param name="logBody">Whether to log request and response bodies.</param>
  /// <returns>A middleware function that can be added to the pipeline.</returns>
  public static Func<Func<HttpRequest, Task<HttpResponse>>, Func<HttpRequest, Task<HttpResponse>>>
      CreateDetailed(bool logHeaders = true, bool logBody = false)
  {
    return next => async request =>
    {
      var stopwatch = Stopwatch.StartNew();
      var requestId = Guid.NewGuid().ToString("N")[..8];
      var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

      Console.WriteLine($"[{timestamp}] [{requestId}] ========== REQUEST ==========");
      Console.WriteLine($"[{timestamp}] [{requestId}] {request.Method} {request.Path}");

      if (logHeaders && request.Headers.Count > 0)
      {
        Console.WriteLine($"[{timestamp}] [{requestId}] Headers:");
        foreach (var header in request.Headers)
        {
          Console.WriteLine($"[{timestamp}] [{requestId}]   {header.Key}: {header.Value}");
        }
      }

      if (logBody && !string.IsNullOrEmpty(request.Body))
      {
        var truncatedBody = request.Body.Length > 500
                ? request.Body[..500] + "... (truncated)"
                : request.Body;
        Console.WriteLine($"[{timestamp}] [{requestId}] Body: {truncatedBody}");
      }

      try
      {
        var response = await next(request);
        stopwatch.Stop();

        Console.WriteLine($"[{timestamp}] [{requestId}] ========== RESPONSE ==========");
        Console.WriteLine($"[{timestamp}] [{requestId}] Status: {response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");

        if (logHeaders && response.Headers.Count > 0)
        {
          Console.WriteLine($"[{timestamp}] [{requestId}] Headers:");
          foreach (var header in response.Headers)
          {
            Console.WriteLine($"[{timestamp}] [{requestId}]   {header.Key}: {header.Value}");
          }
        }

        if (logBody && !string.IsNullOrEmpty(response.Body))
        {
          var truncatedBody = response.Body.Length > 500
                  ? response.Body[..500] + "... (truncated)"
                  : response.Body;
          Console.WriteLine($"[{timestamp}] [{requestId}] Body: {truncatedBody}");
        }

        Console.WriteLine($"[{timestamp}] [{requestId}] ==============================");

        return response;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        Console.WriteLine($"[{timestamp}] [{requestId}] ========== ERROR ==========");
        Console.WriteLine($"[{timestamp}] [{requestId}] Exception: {ex.GetType().Name}");
        Console.WriteLine($"[{timestamp}] [{requestId}] Message: {ex.Message}");
        Console.WriteLine($"[{timestamp}] [{requestId}] Duration: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"[{timestamp}] [{requestId}] ===========================");
        throw;
      }
    };
  }
}
