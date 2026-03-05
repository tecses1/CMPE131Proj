namespace ClientSideWASM;
using Shared;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Channels;

using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class Client
{  
    NetworkManager nm;
    private ClientWebSocket handler;
     Channel<Packet> queuedToSend = Channel.CreateUnbounded<Packet>();
     Channel<Packet> queueRecieved = Channel.CreateUnbounded<Packet>();
    CancellationTokenSource cts;

    public Client(NetworkManager nm)
    {
        this.nm = nm;
    }  
    public async Task ConnectToServer()
    {
        try
        {
            handler = new ClientWebSocket();
            cts = new CancellationTokenSource();
            // Note: Use "ws://" for standard or "wss://" for secure connections
            Uri serverUri = new Uri($"ws://{Settings.server}:{Settings.port}");
            
            Console.WriteLine($"Connecting to {serverUri}...");
            await handler.ConnectAsync(serverUri, cts.Token);
            
            Console.WriteLine("Connected to server!");

            await this.Initialize();

        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
        }
    }

    public async Task Initialize()
    {

        
        //Server expects us to send first, then will follow immediately with server information.
        Console.WriteLine("Initializing client...Sending initial packet to server.");
        Packet initialPacket = new Packet() { customMessage = "Hello Server, I'm a new client!" };
        //Load our client info

        ClientInfo ci = new ClientInfo();
        ci.myName = Settings.name;
        initialPacket.clientInfo = ci;      
        QueueToSend(initialPacket); 



        Console.WriteLine("My data queued to send, starting send loop." );
        Task sendTask = SendLoopAsync(handler, cts.Token);


        Console.WriteLine("Waiting for server information... starting recieve loop.");
        Task receiveTask = ReceiveLoopAsync(handler, cts.Token);

        Console.WriteLine("Blocking for first packet...");
        Packet recieved = await queueRecieved.Reader.ReadAsync(cts.Token);
        Console.WriteLine("Connection Message: " + recieved.customMessage);

        Console.WriteLine("End first packet. Starting process loop.");
        Task processTask = ProcessLoop(handler, cts.Token);
        
        


        Console.WriteLine("Intitialization complete.");
        await Task.WhenAll(sendTask, receiveTask,processTask);
        cts.Cancel();
        //Recv server info
    }

    public bool isConnected()
    {
        return handler.State == WebSocketState.Open;
    }
    public async Task ProcessLoop(WebSocket webSocket, CancellationToken ct)
    {
        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            Console.WriteLine("Process...");
            Packet p = await queueRecieved.Reader.ReadAsync(ct);

            if (p.customMessage.StartsWith("{LobbyCreated}"))
            {
                nm.lobbyName = p.customMessage.Split('\x1f')[1];
            }

            Console.WriteLine("Processed!");
        }
        Console.WriteLine("Process thread died.");

    }


    //Likely shared with USER.cs
    public async Task QueueToSend(Packet p)
    {
        Console.WriteLine("Sending request!");
        //keep client info updated.
        if (p.clientInfo == null)
        {
            p.clientInfo = new ClientInfo();
            p.clientInfo.myName = Settings.name;


        }
        await queuedToSend.Writer.WriteAsync(p);
    }




    public async Task SendLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            
            await foreach (var packet in queuedToSend.Reader.ReadAllAsync(ct))
            {
                Console.WriteLine("Sending...");
                byte[] data = Encoding.UTF8.GetBytes(packet.toJSON());//.ToBytes();
                
                // SendAsync is awaited, ensuring we don't start a second send 
                // until the first one is finished.
                await handler.SendAsync(new ArraySegment<byte>(data), 
                                    WebSocketMessageType.Binary, true, ct);

                Console.WriteLine("Sent");
            }

        }
        Console.WriteLine("Send thread died.");
    }
    public async Task ReceiveLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            // Loop to handle fragmented messages (EndOfMessage = true)
            do
            {
                Console.WriteLine("RECIEVING");
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            } 
            while (!result.EndOfMessage);
            Console.WriteLine("RECIEVED");

            // Convert the full accumulated byte array into a Packet
            byte[] fullMessage = ms.ToArray();
            string fullMessageJSON = Encoding.UTF8.GetString(fullMessage);
            Packet receivedPacket = Packet.fromJSON(fullMessageJSON);

            // Add to the non-thread-safe Queue (assuming this is the only thread writing)
            await queueRecieved.Writer.WriteAsync(receivedPacket);

        }
        Console.WriteLine("Recv thread died.");

    }

}