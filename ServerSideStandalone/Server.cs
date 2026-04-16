namespace ServerSideStandalone;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Shared;

public class Server
{
    public List<User> users = new List<User>();
    private readonly string _url = "http://*:8888/"; // WebSockets start as HTTP
    public List<Lobby> openLobbies = new List<Lobby>();

    Thread updateThread;
    Thread updateLobbyThread;
    public MainWindow mainWindow;
    public Server()
    {

        Console.WriteLine("Server initialized.");
        _ = RunServerAsync(); // Fire and forget

        //start an actual multithread.
        updateLobbyThread = new Thread( UpdateLobbies);
        updateLobbyThread.IsBackground = true; // Ensure it doesn't block application exit
        updateLobbyThread.Start();

        updateThread = new Thread( Update);
        updateThread.IsBackground = true; // Ensure it doesn't block application exit
        updateThread.Start();
    }
    public void Update()
    {
        while(true){
            try{
                foreach (User u in users)
                {
                    if (!u.isConnected())
                    {
                        u.Update();
                        users.Remove(u);
                        
                        break;
                    }
                     //update gui   
                }
            }catch(InvalidOperationException ex)
            {
                Console.WriteLine("Skipping tick, users modified.");
            }
        }
    }
    public  void UpdateLobbies()
    {

        Console.WriteLine("Lobby thread called...");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        double targetMs = 1000.0 / GameConstants.updateRate;
        while (true)
        {
            long startTime = sw.ElapsedMilliseconds;
            try{
                // Update each lobby
                foreach (Lobby lobby in openLobbies)
                {
                    if (lobby.TimeOut())
                    {
                        //Console.WriteLine("Lobby " + lobby.Name + " is empty. Removing...");
                        openLobbies.Remove(lobby);
                        continue;
                    }

                    lobby.Update();
                }
            }catch(InvalidOperationException ex)
            {
                Console.WriteLine("Skipping tick, Lobbies modified.");
            }
            // Sleep to maintain a consistent update rate (e.g., 30 FPS)
            double processTime = sw.ElapsedMilliseconds - startTime;
            // 45hz
            double sleepTime = Math.Max(0, targetMs - processTime);

            Thread.Sleep((int)sleepTime);
        }

    }
    public Lobby CreateLobby(string name)
    {
        foreach (Lobby l in openLobbies)
        {
            if (l.Name == name)
            {
                Console.WriteLine("lobby already exists");
                //Lobby with that name already exists. Return it.
                return l;
            }
        }
        Console.WriteLine("Creating lobby: " + name);
        Lobby newLobby = new Lobby();
        newLobby.Name = name;
        openLobbies.Add(newLobby);

        //Let the GUI know.
        Application.Current.Dispatcher.Invoke(() =>
        {
            LobbyNode newLobbyNode = new LobbyNode
            {
                Name = name,
                PlayerCount = 0,
                tps="Ticks Per Second: None",
                UserList = "Users: Empty",
                Lobby = newLobby


            };
            mainWindow.Lobbies.Add(newLobbyNode);
            newLobby.node = newLobbyNode;
        });


        

        return newLobby;
    }
    public Lobby GetLobby(string lobbyName)
    {
        foreach (Lobby l in openLobbies)
        {
            if (l.Name.Equals(lobbyName))
            {
                return l;
            }
            else
            {
                Console.WriteLine("Compare failed: '" + l.Name + "' vs '" + lobbyName + "'");
            }
        }
        return null;
    }

    public void CloseLobby(Lobby lobby)
    {
        if (openLobbies.Contains(lobby))
        {
            openLobbies.Remove(lobby);
            App.mainWindow.Lobbies.Remove(lobby.node);
            Console.WriteLine($"Lobby {lobby.Name} closed.");
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
    public string GetLobbyList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Open Lobbies:");
        foreach (var lobby in openLobbies)
        {
            sb.AppendLine(lobby.Name +"\n");
            sb.AppendLine(lobby.GetUsers()); // Assuming Lobby has a Name property and GetUsers() returns a string with users separated by newlines
        }
        return sb.ToString();
    }
    public string Command(params string[] args)
    {

        if (args.Length == 0) return "No command provided.";

        switch (args[0])
        {
            case "list":
                return GetUserList();
            case "lobby":
                if (args.Length < 2) return GetLobbyList() + "\n\nThis command is also callable: lobby [open/close] [lobby name]";
                if (args.Length < 3) return "Please specify a lobby name.";
                if (args[1] == "open")
                {
                    CreateLobby(args[2]);
                    return $"Lobby '{args[2]}' created.";
                }else if (args[1] == "close")
                {
                    Lobby l = GetLobby(args[2]);
                    if (l == null) return $"Lobby '{args[2]}' not found.";
                    CloseLobby(l);
                    return $"Lobby '{args[2]}' closed.";
                }
                else return "Unknown lobby command. Use 'open' or 'close'.";
            case "help":
                return "Available commands:\n- list: List connected clients\n- help: Show this help message";
            default:
                return "Unknown command.";
                
        }
        
    }
}
