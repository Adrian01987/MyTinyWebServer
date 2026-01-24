using CustomerApi;
using TinyWebServerLib.Server;
using System.Net;
using TinyWebServerLib.Extensions;
using TinyWebServerLib.Mapper;
using Microsoft.Extensions.DependencyInjection;
using TinyWebServerLib.Http;
using TinyLogger;

namespace MyTinyWebServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProxiedControllers(typeof(CustomersController).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        var builder = new TinyWebServerBuilder();
        builder.UseServiceProvider(serviceProvider);

        builder.Use(next => async request =>
        {
            try
            {
                Console.WriteLine($"{request.Method} {request.Path}");
                return await next(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                var response = new HttpResponse(500, [], "Internal Server Error");
                response.Headers["Content-Type"] = "text/plain";
                return response;
            }
        });

        builder.MapController<CustomersController>();

        builder.UseUrl(IPAddress.Any, 4221);

        TinyWebServer server = builder.Build();

        using var cts = new CancellationTokenSource();
        Task serverTask = server.StartAsync(cts.Token);
        Console.WriteLine("Server running. Press ENTER to stop.");
        Console.ReadLine();
        cts.Cancel();
        await serverTask;
    }
}
