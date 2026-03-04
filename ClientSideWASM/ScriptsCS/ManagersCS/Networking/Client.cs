namespace ClientSideWASM;
using Shared;
using System.Net;
using System.Net.WebSockets;


using System.Net.WebSockets;
using System.Text;

public class Client
{  
    NetworkManager nm;
    private ClientWebSocket _socket;
    public List<Packet> packetsToSend = new List<Packet>();

    public Client(NetworkManager nm)
    {
        this.nm = nm;
        _socket = new ClientWebSocket();
    }

    public async Task ConnectToServer()
    {
        try
        {
            // Note: Use "ws://" for standard or "wss://" for secure connections
            Uri serverUri = new Uri($"ws://{Settings.server}:{Settings.port}");
            
            Console.WriteLine($"Connecting to {serverUri}...");
            await _socket.ConnectAsync(serverUri, CancellationToken.None);
            
            Console.WriteLine("Connected to server!");


        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
        }
    }
    public async Task Update()
    {
        if (isConnected())
        {
            await Recv();

            if (packetsToSend.Count > 0)
            {
                await Send(packetsToSend[0]);
                packetsToSend.RemoveAt(0);
            }
        }
    }

    public bool isConnected()
    {
        return _socket.State == WebSocketState.Open;
    }

    public async Task Send(Packet p)
    {
        if (!isConnected()) return;

        byte[] buffer = Encoding.UTF8.GetBytes(p.toJSON());
        await _socket.SendAsync(
            new ArraySegment<byte>(buffer), 
            WebSocketMessageType.Text, 
            true, 
            CancellationToken.None
        );
    }
    
    private async Task Recv()
    {
        var buffer = new byte[1024 * 4];
        try
        {
            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Packet p = Packet.fromJSON(message);
                    Console.WriteLine($"Received from server: {message}");
                    // Packet p = Packet.fromJSON(message); // Handle your logic here
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection lost: {e.Message}");
        }
    }
}