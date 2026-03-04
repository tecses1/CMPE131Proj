using System.Net;
using System.Net.WebSockets;
using System.Text;
using Shared;

namespace ServerSideStandalone;

public class User
{
    public WebSocket handler;
    public List<Packet> queuedToSend = new List<Packet>();
    public User(WebSocket handler)
    {
        this.handler = handler;
    }   

    public async Task Update()
    {
        this.Recv();

        if (queuedToSend.Count > 0)
        {
            await this.Send(queuedToSend[0]);
            queuedToSend.RemoveAt(0);
        }
        //TEST WITH CUSTOM MESSGHAGE
        queuedToSend.Add(new Packet() { customMessage = "Hello from server!" });
    }

    public async Task Recv()
    {
        var buffer = new byte[1_024];
        WebSocketReceiveResult result = await handler.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string request = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine("recv: " + request);  
        Packet p = Packet.fromJSON(request);
    }
    public async Task Send(Packet packet)
    {
        await handler.SendAsync(Encoding.UTF8.GetBytes(packet.toJSON()), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}