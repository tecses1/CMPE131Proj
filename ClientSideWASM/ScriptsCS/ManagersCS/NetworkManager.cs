using Shared;
using System.Numerics;
namespace ClientSideWASM;
using System.Threading.Channels;
using System.Buffers.Binary; // Required for BinaryPrimitives
//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public GameManager gm;
    public string myLobby = "";
    public bool isHost = false;
    private readonly object _stateLock = new object();
    private byte[] _latestStateBuffer = new byte[4096]; // Initialize with an expected size
    private int _latestStateSize = 0; // Track the actual size of the latest state
    private bool _hasData = false;


    public List<byte[]> inputsReceived = new List<byte[]>();
    public List<byte[]> objsToAdd = new List<byte[]>();
    public List<byte[]> serverGameStates = new List<byte[]>();
    public NetworkManager()
    {
        client = new Client(this);
    }

    public void UpdateGameState(ReadOnlySpan<byte> newState)
    {
        lock (_stateLock)
        {
            // 1. Resize only if the new data is actually bigger than our capacity
            if (newState.Length > _latestStateBuffer.Length)
            {
                _latestStateBuffer = new byte[newState.Length];
                Console.WriteLine("overhead warning, we've exceeded the default buffer size of 1024.");
            }

            // 2. High-speed memory copy into the existing array
            newState.CopyTo(_latestStateBuffer);
            _latestStateSize = newState.Length;
        }
    }

    public int GetGameState(Span<byte> destination)
    {
        lock (_stateLock)
        {
            if (_latestStateSize == 0) return 0;

            // Ensure the caller's buffer is big enough
            if (destination.Length < _latestStateSize)
                throw new ArgumentException("Provided buffer is too small");

            // Direct copy from our internal storage to the caller's memory
            _latestStateBuffer.AsSpan(0, _latestStateSize).CopyTo(destination);
            
            return _latestStateSize;
        }
    }
    public long PeekTick()
    {
        lock (_stateLock)
        {
            // A 'long' is 8 bytes. Ensure we actually have that much data.
            if (_latestStateSize < 8) 
            {
                return -1; // Or throw an exception
            }

            // Create a span of the first 8 bytes and read it as a Little Endian long
            ReadOnlySpan<byte> tickSpan = _latestStateBuffer.AsSpan(0, 8);
            return BinaryPrimitives.ReadInt64LittleEndian(tickSpan);
        }
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







    public void loadGameState( ref List<GameObject> activeObjects)
    {
        
    }
}
