using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TinyWebServerLib.Http;
using TinyWebServerLib.Routing;

namespace TinyWebServerLib.Server;

public class TinyWebServer(IPAddress address, int port, RequestPipeline pipeline, IServiceProvider serviceProvider)
{
    private readonly TcpListener listener = new(address, port);
    private readonly RequestPipeline pipeline = pipeline;
    private readonly IServiceProvider serviceProvider = serviceProvider;


    public async Task StartAsync(CancellationToken token)
    {
        listener.Start();
        while (!token.IsCancellationRequested)
        {
            try 
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                _ = HandleClientAsync(client, token);
            }
            catch(OperationCanceledException ocEx) 
            {
                Console.WriteLine(ocEx.Message);
            }
           
        }
        listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        await using NetworkStream stream = client.GetStream();
        await using var scope = serviceProvider.CreateAsyncScope();
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var requestText = new StringBuilder();
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(token)))
            {
                requestText.AppendLine(line);
            }

            HttpRequest request = HttpParser.Parse(requestText.ToString());
            request.RequestServices = scope.ServiceProvider;

            if (request.Headers.TryGetValue("Content-Length", out var contentLengthStr) && int.TryParse(contentLengthStr, out var contentLength))
            {
                var bodyBuffer = new char[contentLength];
                await reader.ReadBlockAsync(bodyBuffer, 0, contentLength);
                request.Body = new string(bodyBuffer);
            }

            HttpResponse response = await pipeline.RequestDelegate(request);
            string responseText = HttpSerializer.Serialize(response);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            await stream.WriteAsync(responseBytes, token);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            var response = new HttpResponse(500, [], "Internal Server Error");
            response.Headers["Content-Type"] = "text/plain";
            string responseText = HttpSerializer.Serialize(response);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            await stream.WriteAsync(responseBytes, token);
        }
        finally
        {
            client.Close();
        }
    }
}
