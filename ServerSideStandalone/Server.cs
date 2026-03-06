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
    private List<Lobby> openLobbies = new List<Lobby>();

    public Lobby CreateLobby()
    {
        Lobby newLobby = new Lobby();
        newLobby.Name = "Game" + lobbyCtr++;
        openLobbies.Add(newLobby);
        return newLobby;
    }
    public Lobby GetLobby(string lobbyName)
    {
        foreach (Lobby l in openLobbies)
        {
            if (l.Name == lobbyName)
            {
                return l;
            }
        }
        return null;
    }
    
    public Server()
    {
        Console.WriteLine("Server initialized.");
        _ = RunServerAsync(); // Fire and forget
        _ = UpdateServer();// do any update methods we want to update regardless of client input.
    }

    public async Task UpdateServer()
    {
        while (true)
        {
            foreach (Lobby l in openLobbies)
            {
                l.Update();//Send gamestates to the clients, do this on our end to reduce client overhead.
            }
            await Task.Delay(1);
        }
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
            if (user.myLobby != null)
            {
                sb.AppendLine("    Current Lobby: " + user.myLobby.Name);
            }
            else
            {
                sb.AppendLine("    Current Lobby: None");
            }
            sb.AppendLine("    Current Page: " + user.currentPage);
        }
        return sb.ToString();
    }
}
