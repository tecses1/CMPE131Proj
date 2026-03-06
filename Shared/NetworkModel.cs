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
    public async Task Send(string purpose, params string[] args)
    {
        
        var packet = new Packet 
        { 
            CorrelationId = Guid.NewGuid(), 
            RequiresResponse = false,
            IsResponse = false, 
            Purpose = purpose, 
            Args = args 
        };
        if (debug) Console.WriteLine($"Queueing packet: {purpose} with args [{string.Join(", ", args)}]");
        await queuedToSend.Writer.WriteAsync(packet);
    }

    public async Task<Packet> SendWithResponse(string purpose, params string[] args){
        int timeoutMs = 5000; //DEFAULT TIMEOUT
    
        var packet = new Packet 
        { 
            CorrelationId = Guid.NewGuid(), 
            RequiresResponse = true,
            IsResponse = false, 
            Purpose = purpose, 
            Args = args 
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
            byte[] data = Encoding.UTF8.GetBytes(packet.toJSON());
            if (debug) Console.WriteLine("Data length: " + data.Length);
            await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
            if (debug) Console.WriteLine("Send completed.");
        }
            
        
    }

    public async Task ReceiveLoopAsync(WebSocket webSocket, CancellationToken ct)
    {
        if (debug) Console.WriteLine("Starting receive loop...");
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Connecting) 
            {
                await Task.Delay(100, ct); 
            }

            if (webSocket.State != WebSocketState.Open) return;
        if(debug) Console.WriteLine("WebSocket is open, entering receive loop...");
        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                if (debug) Console.WriteLine("Waiting for message...");
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (debug) Console.WriteLine("Message received.");
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct).ConfigureAwait(false);
                    return;
                }
                ms.Write(buffer, 0, result.Count);
            } 
            while (!result.EndOfMessage);

            byte[] fullMessage = ms.ToArray();
            string fullMessageJSON = Encoding.UTF8.GetString(fullMessage);
            if (debug) Console.WriteLine("Received raw message: " + fullMessageJSON);
            Packet p = Packet.fromJSON(fullMessageJSON);

            // --- THE INTERCEPTOR LOGIC ---
            if (p.IsResponse)
            {
                // This is an answer to our CommandAsync! Route it to the TCS.
                if (_pendingRequests.TryRemove(p.CorrelationId, out var tcs))
                {
                    tcs.TrySetResult(p);
                }
            }
            else
            {
                // This is a brand new request from the OTHER side.
                // Put it in the queue for the game loop to process.
                await queueRecieved.Writer.WriteAsync(p);
                
                // You could also call an abstract method here like:
                // _ = HandleIncomingRequestAsync(p);
            }
        }
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
                    string[] responseArgs = await HandleRecvWithResponse(packet.Purpose, packet.Args);


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
                    await HandleRecv(packet.Purpose, packet.Args);
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
        protected abstract Task<string[]> HandleRecvWithResponse(string purpose, string[] args);
        protected abstract Task HandleRecv(string purpose, string[] args);

        // ... (ReceiveLoopAsync and SendLoopAsync remain the same) ...
    
}