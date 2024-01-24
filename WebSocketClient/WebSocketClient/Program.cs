using System.Net.WebSockets;
using MessagePack;

class Program
{
    static async Task Main(string[] args)
    {
        await ProcessSocket();
    }

    private static async Task ProcessSocket()
    {
        var clientWebSocket = new ClientWebSocket();
        try
        {
            Uri serverUri = new Uri("ws://localhost:5000");
            await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

            Console.WriteLine("Connected to the server.");

            while (clientWebSocket.State == WebSocketState.Open)
            {
                var receivedMessage = await ReceiveMessage(clientWebSocket);

                if (receivedMessage != null)
                {
                    Console.WriteLine($"Received: {receivedMessage}");
                }
            }
        }
        catch (Exception ex)
        {  
            Console.WriteLine($"Error connecting to the server: {ex.Message}");
        }
        finally
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the client", CancellationToken.None);
            }
        }
    }

    private static async Task<string> ReceiveMessage(ClientWebSocket webSocket)
    {
        try
        {
            byte[] buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType != WebSocketMessageType.Binary)
            {
                return string.Empty;
            }

            return MessagePackSerializer.Deserialize<string>(buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
            return string.Empty;
        }
    }
}

