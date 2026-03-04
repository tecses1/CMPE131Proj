namespace ServerSideStandalone;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Server
{
    private List<User> users = new List<User>();
    private readonly string _url = "http://*:8888/"; // WebSockets start as HTTP

    public Server()
    {
        Console.WriteLine("Server initialized.");
        _ = RunServerAsync(); // Fire and forget
    }

    public async Task RunServerAsync()
    {
        using HttpListener listener = new HttpListener();
        listener.Prefixes.Add(_url);
        listener.Start();
        Console.WriteLine($"Launching server on {_url}...");

        while (true)
        {
            // Wait for an incoming HTTP request
            HttpListenerContext context = await listener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                // Accept the WebSocket connection
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;

                Console.WriteLine("Client requesting connection: WebSocket Upgrade");
                _ = Task.Run(() => ProcessClientAsync(webSocket));
            }
            else
            {
                // Reject non-WebSocket requests
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task ProcessClientAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024];
        
        // Receive the first message (assuming it's your JSON Packet)
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string request = Encoding.UTF8.GetString(buffer, 0, result.Count);
        
        // Packet p = Packet.fromJSON(request); // Your custom logic

        // Create user with the WebSocket instead of a Socket
        User newUser = new User(webSocket);
        users.Add(newUser);

        Console.WriteLine("Client connected via WebSocket.");
        
        // Keep connection alive/listen for more messages if needed
        // Note: You must handle the receive loop or newUser.Update() must use webSocket.SendAsync/ReceiveAsync
    }
}
