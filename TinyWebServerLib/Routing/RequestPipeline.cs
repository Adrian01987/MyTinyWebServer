using TinyWebServerLib.Http;

namespace TinyWebServerLib.Routing;

public class RequestPipeline(Func<HttpRequest, Task<HttpResponse>> requestDelegate)
{
    public Func<HttpRequest, Task<HttpResponse>> RequestDelegate { get; init; } = requestDelegate;
}
