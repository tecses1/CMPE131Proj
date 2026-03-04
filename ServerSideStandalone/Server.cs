namespace ServerSideStandalone;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

public class Server
{
    int uidCounter = 0;
    private List<User> users = new List<User>();
    private readonly string _url = "http://127.0.0.1:8888/"; // WebSockets start as HTTP

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

                Console.WriteLine("Client requesting connection:"+ webSocket.SubProtocol);
                _ = Task.Run(() => ProcessClientAsync(webSocket));
            }
            else
            {
                Console.WriteLine("Non websocket protocol requested connection... blocking!");
                // Reject non-WebSocket requests
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task ProcessClientAsync(WebSocket webSocket)
    {

        
        // Packet p = Packet.fromJSON(request); // Your custom logic

        // Create user with the WebSocket instead of a Socket
        User newUser = new User(webSocket, uidCounter++);
        newUser.Initialize(); // Assuming User has an Initialize method to set the WebSocket
        users.Add(newUser);

        Console.WriteLine("Client connected via WebSocket.");
        
        // Keep connection alive/listen for more messages if needed
        // Note: You must handle the receive loop or newUser.Update() must use webSocket.SendAsync/ReceiveAsync
    }

    public int GetClientCount()
    {
        return users.Count;
    }

    public string GetUserList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Connected Clients:");
        foreach (var user in users)
        {
            sb.AppendLine($"- {user.name} (UID: {user.uid})"); // Assuming User has an Id property
        }
        return sb.ToString();
    }
}
