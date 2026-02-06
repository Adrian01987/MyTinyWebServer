using TinyWebServerLib.Http;

namespace TinyWebServerLib.Routing;

public class RequestPipeline(RequestHandler requestDelegate)
{
    public RequestHandler RequestDelegate { get; init; } = requestDelegate;
}
