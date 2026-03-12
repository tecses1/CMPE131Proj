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
    int lobbyCtr = 0;
    public List<User> users = new List<User>();
    private readonly string _url = "http://*:8888/"; // WebSockets start as HTTP
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
        _ = UpdateLobbies();
    }

    public async Task UpdateLobbies()
    {
        Console.WriteLine("Lobby thread called...");
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000f/60f)); //60hz

        while (await timer.WaitForNextTickAsync())        {
            foreach (Lobby l in openLobbies)
            {
                try{
                l.Update();
                }catch (Exception ex)
                {
                    Console.WriteLine("Error processing lobby: " + ex + " Trace: " + ex.StackTrace);
                }
            }

        }
    }

    public async Task RunServerAsync()
    {
        HttpListener listener = new HttpListener();

        try 
        {
            // Attempt 1: Broadcast mode
            listener.Prefixes.Add(_url);
            listener.Start();
            Console.WriteLine($"Server started in BROADCAST mode at {_url}");
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5) // Error 5 is "Access Denied"
        {
            Console.WriteLine("Broadcast failed (Admin rights required). Falling back to local...");
            listener.Close();
            listener = new HttpListener();
            // Clear the failed prefix and try the local one
            listener.Prefixes.Clear();
            listener.Prefixes.Add("http://localhost:8888/");
            
            try 
            {
                listener.Start();
                Console.WriteLine($"Server started in LOCAL mode at {"localhost:8888"}");
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Critical Failure: {fallbackEx.Message}");
            }
        }

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
        User newUser = new User(this);
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
