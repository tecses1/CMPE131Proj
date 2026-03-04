using System.Net;
using System.Net.WebSockets;
using System.Text;
using Shared;

namespace ServerSideStandalone;

public class User
{
    public string name;
    public int uid;
    public WebSocket handler;
    public List<Packet> queuedToSend = new List<Packet>();
    public List<Packet>  queueRecieved = new List<Packet>();
    public User(WebSocket handler, int uid)
    {
        this.handler = handler;
        this.uid = uid;
        

    }   

    public async Task<Packet> getLatest()
    {
        while (queueRecieved.Count == 0)
        {
            await Task.Delay(100);
            // Wait until we have received a packet
        }

        Packet p = queueRecieved[0];
        queueRecieved.RemoveAt(0);
        return p;
    }
    public void QueueToSend(Packet p)
    {
        queuedToSend.Add(p);
    }   
    public void Initialize()
    {
        //We expect client to send first.
        Console.WriteLine("Initializing user...Waiting for first recieve.");
        Recv(); // Blocking call to receive the initial packet from the client
        Console.WriteLine("Recieve complete. Processing initial packet...");
        Packet p = getLatest().Result; // Get the initial packet from the client        
        Console.WriteLine("Connection Message: " + p.customMessage);
        Console.WriteLine("Got client name: " + p.clientInfo.myName);
        this.name = p.clientInfo.myName; // Set the user's name based on the packet's client info

        //Send server information back to them. 
        Console.WriteLine("Client expects our information. Sending it over...");
        Packet serverInfo = new Packet() { customMessage = "Welcome to the server!" };
        QueueToSend(serverInfo);
        Send();

        Console.WriteLine("Initialize Complete. Calling update thread.");
        //Begin update thread.
        _ = Task.Run(() => Update());
    }
    public async Task Update()
    {
        Console.WriteLine("Starting update loop for user..." + GetAddress());
        while (true)
        {
            await this.Recv();
            await this.Send();

            

            //TEST WITH CUSTOM MESSGHAGE
            queuedToSend.Add(new Packet() { customMessage = "Hello from server!" });
            await Task.Delay(100);
        }

    }
    private async Task Recv()
    {
        var buffer = new byte[4096];
        try{
            while (handler.State == WebSocketState.Open)
            {
                var result = await handler.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                        await handler.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Use the Count property!
                    var data = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    //Console.WriteLine($"ACTUAL DATA: {data}");
                    
                    if (result.EndOfMessage) 
                    {
                        string request = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        //Console.WriteLine("Raw message: " + request);
                        Packet p = Packet.fromJSON(request);
                        queueRecieved.Add(p); 
                        return;
                    }
                }
            }
        }catch (Exception e)
        {
            Console.WriteLine($"Connection lost: {e.Message}");
        }
    }    public async Task Send( )
    {
        if (queuedToSend.Count == 0) return;
        Packet packet = queuedToSend[0];
        queuedToSend.RemoveAt(0);
        await handler.SendAsync(Encoding.UTF8.GetBytes(packet.toJSON()), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public string GetAddress()
    {
        return handler?.CloseStatusDescription ?? "Unknown";
    }   
}