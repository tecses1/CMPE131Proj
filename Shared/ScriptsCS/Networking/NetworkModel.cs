namespace Shared;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;public abstract class NetworkModel
{
    private WebSocket handler;
    protected Channel<Packet> queuedToSend = Channel.CreateUnbounded<Packet>();
    protected Channel<Packet> queueRecieved = Channel.CreateUnbounded<Packet>();

    public bool debug = false;
    
    // Tracks outgoing commands waiting for a response
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Packet>> _pendingRequests = new();
    protected CancellationTokenSource cts;

    public string GetAddress() => handler?.CloseStatusDescription ?? "Unknown";

    int latency = 0;

    public int GetLatency()
    {
        // Placeholder for latency measurement logic
        return 0;
    }
    public bool isConnected(){
        return handler?.State == WebSocketState.Open;
    }

    public void Initialize(WebSocket handler, CancellationTokenSource cts = null)
    {
        Console.WriteLine("Initializing network model...");
        if (cts == null){
            cts = new CancellationTokenSource();
        }
        this.handler = handler;
        this.cts = cts;
        Console.WriteLine("Double checking state: " + handler.State + "/" + this.isConnected());
        Console.WriteLine("Starting loops in the background...");
        
        // We do NOT await these here. We let them run in the background.
        Task.Run(() => ReceiveLoopAsync(handler, cts.Token), cts.Token);
        Task.Run(() => SendLoopAsync(handler, cts.Token), cts.Token);
        Task.Run(() => ProcessIncomingPackets(cts.Token), cts.Token);
    }
    public async Task Send(string purpose, byte[] data = null, params string[] args)
    {
        
        var packet = new Packet 
        { 
            CorrelationId = Guid.NewGuid(), 
            RequiresResponse = false,
            IsResponse = false, 
            Purpose = purpose, 
            Args = args, 
            Data = data
            
        };
        if (debug) Console.WriteLine($"Queueing packet: {purpose} with args [{string.Join(", ", args)}]");
        await queuedToSend.Writer.WriteAsync(packet);
    }

    public async Task<Packet> SendWithResponse(string purpose, byte[] data=null, params string[] args){
        int timeoutMs = 5000; //DEFAULT TIMEOUT
    
        var packet = new Packet 
        { 
            CorrelationId = Guid.NewGuid(), 
            RequiresResponse = true,
            IsResponse = false, 
            Purpose = purpose, 
            Args = args, 
            Data = data
        };

        var tcs = new TaskCompletionSource<Packet>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[packet.CorrelationId] = tcs;

        await queuedToSend.Writer.WriteAsync(packet);

        using var cts = new CancellationTokenSource(timeoutMs);
        using var registration = cts.Token.Register(() => 
        {
            _pendingRequests.TryRemove(packet.CorrelationId, out _);
            tcs.TrySetCanceled();
        });

        try { return await tcs.Task; }
        catch (TaskCanceledException) { return null; }
    }

    public async Task SendLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        if (debug) Console.WriteLine("Starting send loop...");

        await foreach (var packet in queuedToSend.Reader.ReadAllAsync(ct))
        {
            if (debug) Console.WriteLine($"Sending packet: {packet.Purpose} with args [{string.Join(", ", packet.Args)}]");
            byte[] data = packet.Serialize();
            if (debug) Console.WriteLine("Data length: " + data.Length);
            await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, ct).ConfigureAwait(false);
            if (debug) Console.WriteLine("Send completed.");
        }
            
        
    }

    public async Task ReceiveLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        var buffer = new byte[1024 * 4];

        // Wait for connection to be ready
        while (webSocket.State == WebSocketState.Connecting) 
        {
            await Task.Delay(100, ct); 
        }

        if (webSocket.State != WebSocketState.Open) return;

        try
        {
            while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                // 1. Receive a full message
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                        return;
                    }
                    
                    ms.Write(buffer, 0, result.Count);
                } 
                while (!result.EndOfMessage);

                // 2. Process the packet
                byte[] fullMessage = ms.ToArray();
                Packet p = Packet.Deserialize(fullMessage);
                //latency = (int)(DateTime.UtcNow - p.Timestamp).TotalMilliseconds;

                if (p.IsResponse)
                {
                    if (_pendingRequests.TryRemove(p.CorrelationId, out var tcs))
                    {
                        // TrySetResult runs continuations; to keep this loop fast, 
                        // ensure your TCS continuations aren't doing heavy work.
                        tcs.TrySetResult(p);
                    }
                }
                else
                {
                        await queueRecieved.Writer.WriteAsync(p, ct);
                    
                }
                
            }
        }
        catch (OperationCanceledException) { /* Handle shut down */ }
        catch (Exception ex) { /* Log connection reset etc */ }
    }
    private async Task ProcessIncomingPackets(CancellationToken ct)
    {
        if (debug) Console.WriteLine("Starting process loop...");
        // ReadAllAsync will keep this loop alive as long as the Channel is open.
        // We wrap the INSIDE in a try-catch so one crash doesn't stop the loop.
        await foreach (var packet in queueRecieved.Reader.ReadAllAsync(ct))
        {
            try
            {
                if (packet.RequiresResponse)
                {
                    string[] responseArgs = await HandleRecvWithResponse(packet.Purpose,packet.Data, packet.Args);


                    await queuedToSend.Writer.WriteAsync(new Packet
                    {
                        CorrelationId = packet.CorrelationId,
                        RequiresResponse = false,
                        IsResponse = true,
                        Purpose = packet.Purpose + "_ACK",
                        Args = responseArgs
                    });

                }
                else
                {
                    await HandleRecv(packet.Purpose, packet.Data, packet.Args);
                }

            }
            catch (Exception ex)
            {
                // Log the error but DON'T let the loop exit.
                Console.WriteLine($"Error processing packet {packet.Purpose}: {ex.Message}");
            }
        }
    }

        // This is your "Callback." Inheriting classes MUST implement this.
        protected abstract Task<string[]> HandleRecvWithResponse(string purpose,byte[] data, string[] args);
        protected abstract Task HandleRecv(string purpose,byte[] data, string[] args);

        // ... (ReceiveLoopAsync and SendLoopAsync remain the same) ...
//Helper function. 
public static byte[] SerializeJagged(byte[][] source)
{
    if (source == null) return new byte[0];

    using (MemoryStream ms = new MemoryStream())
    using (BinaryWriter writer = new BinaryWriter(ms))
    {
        // 1. Write the count of sub-arrays
        writer.Write(source.Length);

        foreach (byte[] subArray in source)
        {
            if (subArray == null)
            {
                writer.Write(0); // Handle nulls as 0-length
                continue;
            }

            // 2. Write the length of THIS sub-array
            writer.Write(subArray.Length);
            // 3. Write the actual bytes
            writer.Write(subArray);
        }

        return ms.ToArray();
    }
}
//helper
 public static byte[][] DeserializeJagged(byte[] data)
{
    if (data == null || data.Length == 0) return new byte[0][];

    using (MemoryStream ms = new MemoryStream(data))
    using (BinaryReader reader = new BinaryReader(ms))
    {
        // 1. Read how many sub-arrays to expect
        int count = reader.ReadInt32();
        byte[][] result = new byte[count][];

        for (int i = 0; i < count; i++)
        {
            // 2. Read the length of the next sub-array
            int subLength = reader.ReadInt32();
            
            // 3. Read exactly that many bytes
            result[i] = reader.ReadBytes(subLength);
        }

        return result;
    }
}   
}