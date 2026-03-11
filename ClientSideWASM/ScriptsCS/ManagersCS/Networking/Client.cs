namespace ClientSideWASM;
using Shared;
using System.Data.Common;
using System.Net.WebSockets;


public class Client : NetworkModel
{  
    NetworkManager nm;
    public Guid assignedUID;
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
            this.Initialize(handler, cts);
            Console.WriteLine("Connected to server! Connection check: " + handler.State);

            Console.WriteLine("Requesting UID from server...");
            Packet uidPacket = await SendWithResponse("{RequestUID}",null);
            Console.WriteLine("parsing UID: " + uidPacket.Args[0]);
            this.assignedUID = Guid.Parse(uidPacket.Args[0]);
            Console.WriteLine($"Received assigned UID: {assignedUID}");
            

        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
        }
    }

    protected override async Task<string[]> HandleRecvWithResponse(string purpose, byte[] data, string[] args)
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
    protected override async Task HandleRecv(string purpose, byte[] data, string[] args)
    {
        switch (purpose)
        {
            case "{GameStateUpdate}":
                // Logic: Show a popup
                //if (data == null) Console.WriteLine("WARNING: NULL GAMESTATE???");
                //Console.WriteLine("Got game state update: " + data.Length);
                //this.nm.gm.gl.LoadGameState(data);
                this.nm.UpdateGameState(data);
                break;
            case "{InputAll}":
                // Logic: Show a popup
                //Console.WriteLine("got new json player: " + args[0]);
                this.nm.inputsReceived.Add(data);
                break;
            case "{SpawnGameObject}":
                // Logic: Show a popup
                nm.objsToAdd.Add(data);
                
                break;
            default:
                break;
        }
    }
}