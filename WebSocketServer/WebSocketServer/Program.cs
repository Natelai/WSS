using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MessagePack;

class Program
{
    static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://0.0.0.0:5000");
                webBuilder.Configure(app =>
                {
                    app.UseWebSockets();
                    app.Use(async (context, next) =>
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            await WebSocketHandler(webSocket, context.RequestAborted);
                        }
                        else
                        {
                            await next();
                        }
                    });
                });
            });

    private static async Task WebSocketHandler(WebSocket websocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        while (websocket.State == WebSocketState.Open)
        {
            var responseMessage = "Message";
            var responseBuffer = MessagePackSerializer.Serialize(responseMessage);
            await websocket.SendAsync(new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length),
                    WebSocketMessageType.Binary, true, cancellationToken);
            await Task.Delay(10000);
        }
    }
}
