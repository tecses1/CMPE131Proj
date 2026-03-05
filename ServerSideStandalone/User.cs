using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Windows.Controls;
using Shared;
using System.Threading.Channels;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServerSideStandalone;
using System.IO;
public class User
{
    Server myServer;
    public string name;
    public string myLobby;
    public string currentPage;
    public int uid;
    public WebSocket handler;
     Channel<Packet> queuedToSend = Channel.CreateUnbounded<Packet>();
     Channel<Packet> queueRecieved = Channel.CreateUnbounded<Packet>();
    public User(WebSocket handler, int uid, Server s)
    {
        this.handler = handler;
        this.uid = uid;
        this.myServer = s;
        

    }   



    
 public async Task Initialize()
    {
        var cts = new CancellationTokenSource();
        
        //Server expects us to send first, then will follow immediately with server information.
        Console.WriteLine("Initializing User...recv initial packet from Client!");
  
        Console.WriteLine("Waiting for server information... starting recieve loop.");
        Task receiveTask = ReceiveLoopAsync(handler, cts.Token);


        Console.WriteLine("My data queued to send, starting send loop." );
        Task sendTask = SendLoopAsync(handler, cts.Token);




        Console.WriteLine("Blocking for first recv...");
        Packet recieved = await queueRecieved.Reader.ReadAsync();
        Console.WriteLine("Connection Message: " + recieved.customMessage);


        Console.WriteLine("Sending ours back...");
        Packet backToClient = new Packet();
        backToClient.customMessage = "WELCOME TO SERVER BRUH";
        await this.QueueToSend(backToClient);

        Console.WriteLine("Beginning process thread");
        Task processTask = Process(handler, cts.Token);
        
    

        Console.WriteLine("Intitialization complete.");
        await Task.WhenAll(receiveTask, sendTask, processTask);
        cts.Cancel();
        //Recv server info
    }


    public async Task Process(WebSocket webSocket, CancellationToken ct)
    {
        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            //Console.WriteLine("Processing...");

            Packet p = await queueRecieved.Reader.ReadAsync(ct);

            //Console.WriteLine("Processed.");
            if (p.clientInfo != null)
            {
                name = p.clientInfo.myName;
                currentPage = p.clientInfo.currentPage;
            }

            Packet toSend = new Packet();
            if (p.customMessage.StartsWith("{requestLobby}"))
            {
                Console.WriteLine("User " + name + " is requesting a game lobby.");
                this.myLobby = myServer.getLobby();
                toSend.customMessage = "{LobbyCreated}" + '\x1F' + this.myLobby;
            }


            QueueToSend(toSend);
        }
    }
    public string GetAddress()
    {
        return handler?.CloseStatusDescription ?? "Unknown";
    }   
    public bool isConnected()
    {
        return handler.State == WebSocketState.Open;
    }



   public async Task QueueToSend(Packet p)
    {
        //Console.WriteLine("Sending request!");
        //keep client info updated.
        await queuedToSend.Writer.WriteAsync(p);
    }




    public async Task SendLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            //Console.WriteLine("Sending...");
            await foreach (var packet in queuedToSend.Reader.ReadAllAsync(ct))
            {
                byte[] data = Encoding.UTF8.GetBytes(packet.toJSON());//.ToBytes();
                
                // SendAsync is awaited, ensuring we don't start a second send 
                // until the first one is finished.
                await handler.SendAsync(new ArraySegment<byte>(data), 
                                    WebSocketMessageType.Binary, true, ct);

                //Console.WriteLine("Sent");
            }

        }
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
                //Console.WriteLine("RECIEVING");
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            } 
            while (!result.EndOfMessage);
            //Console.WriteLine("RECIEVED");

            // Convert the full accumulated byte array into a Packet
            byte[] fullMessage = ms.ToArray();
            string fullMessageJSON = Encoding.UTF8.GetString(fullMessage);
            Packet receivedPacket = Packet.fromJSON(fullMessageJSON);

            // Add to the non-thread-safe Queue (assuming this is the only thread writing)
            await queueRecieved.Writer.WriteAsync(receivedPacket);

        }
    }

    



}