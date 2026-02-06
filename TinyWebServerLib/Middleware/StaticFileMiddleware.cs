using TinyWebServerLib.Http;

namespace TinyWebServerLib.Middleware;

/// <summary>
/// Provides middleware for serving static files from a directory.
/// This is an educational implementation demonstrating file serving and MIME type handling.
/// </summary>
public static class StaticFileMiddleware
{
  /// <summary>
  /// Common MIME type mappings for web content.
  /// </summary>
  private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
  {
    [".html"] = "text/html",
    [".htm"] = "text/html",
    [".css"] = "text/css",
    [".js"] = "application/javascript",
    [".json"] = "application/json",
    [".xml"] = "application/xml",
    [".png"] = "image/png",
    [".jpg"] = "image/jpeg",
    [".jpeg"] = "image/jpeg",
    [".gif"] = "image/gif",
    [".svg"] = "image/svg+xml",
    [".ico"] = "image/x-icon",
    [".txt"] = "text/plain",
    [".pdf"] = "application/pdf",
    [".woff"] = "font/woff",
    [".woff2"] = "font/woff2",
    [".ttf"] = "font/ttf"
  };

  /// <summary>
  /// Creates a middleware that serves static files from the specified root path.
  /// </summary>
  /// <param name="rootPath">The root directory from which to serve files.</param>
  /// <returns>A middleware function that can be added to the pipeline.</returns>
  /// <example>
  /// <code>
  /// builder.Use(StaticFileMiddleware.Create("./wwwroot"));
  /// </code>
  /// </example>
  public static Func<RequestHandler, RequestHandler>
      Create(string rootPath)
  {
    // Resolve the full path once at creation time
    var fullRootPath = Path.GetFullPath(rootPath);

    return next => async request =>
    {
      // Only handle GET requests for static files
      if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
      {
        return await next(request);
      }

      // Get the path without query string
      var requestPath = request.PathBase.TrimStart('/');

      // Prevent directory traversal attacks
      if (requestPath.Contains(".."))
      {
        return await next(request);
      }

      var filePath = Path.Combine(fullRootPath, requestPath);

      // Ensure the resolved path is still within the root
      var resolvedPath = Path.GetFullPath(filePath);
      if (!resolvedPath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase))
      {
        return await next(request);
      }

      // Check if file exists
      if (!File.Exists(filePath))
      {
        return await next(request);
      }

      // Determine content type
      var extension = Path.GetExtension(filePath);
      var contentType = MimeTypes.GetValueOrDefault(extension, "application/octet-stream");

      // Read and return the file content
      var content = await File.ReadAllTextAsync(filePath);

      var headers = new Dictionary<string, string>
      {
        ["Content-Type"] = contentType,
        ["Content-Length"] = content.Length.ToString()
      };

      return new HttpResponse(200, headers, content);
    };
  }

  /// <summary>
  /// Creates a middleware that serves static files with optional default document support.
  /// </summary>
  /// <param name="rootPath">The root directory from which to serve files.</param>
  /// <param name="defaultDocument">The default document to serve for directory requests (e.g., "index.html").</param>
  /// <returns>A middleware function that can be added to the pipeline.</returns>
  public static Func<RequestHandler, RequestHandler>
      Create(string rootPath, string defaultDocument)
  {
    var fullRootPath = Path.GetFullPath(rootPath);

    return next => async request =>
    {
      if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
      {
        return await next(request);
      }

      var requestPath = request.PathBase.TrimStart('/');

      if (requestPath.Contains(".."))
      {
        return await next(request);
      }

      var filePath = Path.Combine(fullRootPath, requestPath);
      var resolvedPath = Path.GetFullPath(filePath);

      if (!resolvedPath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase))
      {
        return await next(request);
      }

      // Check for default document if path is a directory or empty
      if (string.IsNullOrEmpty(requestPath) || Directory.Exists(filePath))
      {
        filePath = Path.Combine(filePath, defaultDocument);
      }

      if (!File.Exists(filePath))
      {
        return await next(request);
      }

      var extension = Path.GetExtension(filePath);
      var contentType = MimeTypes.GetValueOrDefault(extension, "application/octet-stream");
      var content = await File.ReadAllTextAsync(filePath);

      var headers = new Dictionary<string, string>
      {
        ["Content-Type"] = contentType,
        ["Content-Length"] = content.Length.ToString()
      };

      return new HttpResponse(200, headers, content);
    };
  }
}
