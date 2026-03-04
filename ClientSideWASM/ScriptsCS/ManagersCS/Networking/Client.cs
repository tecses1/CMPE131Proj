namespace ClientSideWASM;
using Shared;
using System.Net;
using System.Net.WebSockets;


using System.Net.WebSockets;
using System.Text;

public class Client
{  
    NetworkManager nm;
    private ClientWebSocket handler;
    public List<Packet> queuedToSend = new List<Packet>();
    public List<Packet> queueRecieved = new List<Packet>();

    public Client(NetworkManager nm)
    {
        this.nm = nm;
        handler = new ClientWebSocket();
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
    public async Task ConnectToServer()
    {
        try
        {
            // Note: Use "ws://" for standard or "wss://" for secure connections
            Uri serverUri = new Uri($"ws://{Settings.server}:{Settings.port}");
            
            Console.WriteLine($"Connecting to {serverUri}...");
            await handler.ConnectAsync(serverUri, CancellationToken.None);
            
            Console.WriteLine("Connected to server!");

            await this.Initialize();

            _ = Task.Run(() => Update());
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
        await Send();
        Console.WriteLine("Waiting for server information...");

        await Recv();
        //WAIT TILLL WE HAVE SERVER INFO!!!!
        Packet recieved = await getLatest();
        Console.WriteLine("Connection Message: " + recieved.customMessage);


        
        
        Console.WriteLine("Server information recieved. Intiializing with server information...");
        
        //Recv server info
    }
    public async Task Update()
    {
        Console.WriteLine("Starting client update loop...");
        while (true)
        {
            if (isConnected())
                {

                    await Send();
                    await Recv();

                }
            await Task.Delay(100);
            }
            

    }

    public bool isConnected()
    {
        return handler.State == WebSocketState.Open;
    }

    public async Task Send()
    {
        if (!isConnected()) return;
        if (queuedToSend.Count == 0) return; 
        Packet toSend = queuedToSend[0];
        queuedToSend.RemoveAt(0);
        byte[] buffer = Encoding.UTF8.GetBytes(toSend.toJSON());
        await handler.SendAsync(
            new ArraySegment<byte>(buffer), 
            WebSocketMessageType.Text, 
            true, 
            CancellationToken.None
        );
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
    }

}