namespace ServerSideStandalone;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

public class Server
{
    int uidCounter = 0;
    int lobbyCtr = 0;
    public List<User> users = new List<User>();
    private readonly string _url = "http://127.0.0.1:8888/"; // WebSockets start as HTTP
    private List<String> openLobbies = new List<String>();

    public string getLobby()
    {
        string newLobby = "Game" + lobbyCtr++;
        openLobbies.Add(newLobby);
        return newLobby;
    }
    public bool getLobby(string lobbyName)
    {
        return openLobbies.Contains(lobbyName);
    }
    
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

        // Create user with the WebSocket instead of a Socket
        User newUser = new User(uidCounter++,this);
        newUser.Initialize(webSocket); // Assuming User has an Initialize method to set the WebSocket
        await Task.Delay(1000);
        users.Add(newUser);

        Console.WriteLine("Client connected via WebSocket.");
        
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
            sb.AppendLine("    Current Lobby: " + user.myLobby);
            sb.AppendLine("    Current Page: " + user.currentPage);
        }
        return sb.ToString();
    }
}
