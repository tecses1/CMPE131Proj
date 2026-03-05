namespace ClientSideWASM;
using Shared;
using System.Net.WebSockets;


public class Client : NetworkModel
{  
    NetworkManager nm;
    public int UID;



    public Client(NetworkManager nm)
    {
        this.nm = nm;
    }  
    public async Task ConnectToServer()
    {
        try
        {
            ClientWebSocket handler = new ClientWebSocket();
            var cts = new CancellationTokenSource();
            // Note: Use "ws://" for standard or "wss://" for secure connections
            Uri serverUri = new Uri($"ws://{Settings.server}:{Settings.port}");
            
            Console.WriteLine($"Connecting to {serverUri}...");
            await handler.ConnectAsync(serverUri, cts.Token);
            
            Console.WriteLine("Connected to server! Connection check: " + handler.State);

            this.Initialize(handler, cts);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
        }
    }

    protected override async Task<string[]> HandleRecvWithResponse(string purpose, string[] args)
    {
        //Console.WriteLine("PACKET: " + purpose + " | " + string.Join(", ", args));  
        switch (purpose)
        {
            case "ServerAlert":
                // Logic: Show a popup
                Console.WriteLine($"SERVER SAYS: {args[0]}");
                return new[] { "Acknowledge" }; // Send an ACK back to server

            default:
                return null;
        }
    }
    protected override async Task HandleRecv(string purpose, string[] args)
    {
        
    }
}