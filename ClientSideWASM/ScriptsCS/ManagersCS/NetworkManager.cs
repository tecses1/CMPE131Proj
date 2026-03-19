using Shared;
using System.Numerics;
namespace ClientSideWASM;
using System.Threading.Channels;
using System.Diagnostics;
using System.Buffers.Binary; // Required for BinaryPrimitives
using System.ComponentModel.DataAnnotations;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public GameManager gm;
    public string myLobby = "";
    public bool isHost = false;
    public StateManager StateQueue = new StateManager(8,65536);
        private Stopwatch _intervalTimer = Stopwatch.StartNew();

    public List<byte[]> inputsReceived = new List<byte[]>();
    public List<byte[]> objsToAdd = new List<byte[]>();
    public NetworkManager()
    {
        client = new Client(this);
    }

    public void UpdateGameState(byte[] newState)
    {
        this.StateQueue.PushState(newState,_intervalTimer.ElapsedMilliseconds);

    }

    public  byte[] GetGameState(out long arrivalTime)
    {
        return StateQueue.TryPopState(out arrivalTime);
    }

    public long PeekArrivalTime()
    {
        
        return this.StateQueue.PeekArrivalTime();

    }
    public void Initialize(GameManager gm){
        this.gm = gm;
    }
    public async Task Initialize()
    {
        
        await client.ConnectToServer();
    }
    public byte[] getPlayerState(Player p)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // 1. Metadata (The Header)
            p.WriteMetaData(writer);

            // 2. Data (The Payload)
            p.Encode(writer); // Uses your refined reflection-based encoder

            return ms.ToArray();
        }
    }

}
