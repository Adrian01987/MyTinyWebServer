// Type alias for the common request handler delegate type.
// This alias is available throughout the TinyWebServerLib project and provides
// a cleaner name for the frequently-used Func<HttpRequest, Task<HttpResponse>> signature.
global using RequestHandler = System.Func<TinyWebServerLib.Http.HttpRequest, System.Threading.Tasks.Task<TinyWebServerLib.Http.HttpResponse>>;
