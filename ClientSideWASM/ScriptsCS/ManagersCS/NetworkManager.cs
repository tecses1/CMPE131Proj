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

    public List<byte[]> inputsReceived = new List<byte[]>();
    public List<byte[]> objsToAdd = new List<byte[]>();
    public NetworkManager()
    {
        client = new Client(this);
    }

    public void UpdateGameState(byte[] newState)
    {

        if (newState == null || newState.Length < 8) 
        {
            Console.WriteLine("WARNING: Direct from server null or invalid update!! :(");
            return;
        }

        // 1. Read the long (tick) from the first 8 bytes of the array.
        // C#'s BinaryWriter uses Little Endian by default. 
        long serverTick = BinaryPrimitives.ReadInt64LittleEndian(newState);

        // 2. Convert the server tick to your logical timeline in milliseconds.
        // (Assuming your server sends raw tick numbers like 1, 2, 3... and runs at 30 TPS)
        //long logicalServerTime = (long)(serverTick * (1000.0 / 30.0));

        // NOTE: If your server is ALREADY writing its elapsed time in milliseconds 
        // into that long instead of a raw tick count, you can just do:
        // long logicalServerTime = serverTick;

        // 3. Push it into the queue using the perfect server timeline!
        this.StateQueue.PushState(newState, serverTick);

    }

    public  byte[] GetGameState(out long arrivalTime)
    {

        byte[] returnState = StateQueue.TryPopState(out arrivalTime);
        if (returnState == null)
        {
            Console.WriteLine("Warning, Pulled null byte update. Count Debug: " + StateQueue.Count);
        }

        return returnState;
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
