using Shared;
using System.Numerics;
namespace ClientSideWASM;
using System.Threading.Channels;
//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public GameManager gm;
    public string myLobby = "";
    public bool isHost = false;
    private readonly Channel<byte[]> _stateChannel = Channel.CreateBounded<byte[]>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest }
    );

    public List<byte[]> inputsReceived = new List<byte[]>();
    public List<byte[]> objsToAdd = new List<byte[]>();
    public List<byte[]> serverGameStates = new List<byte[]>();
    public NetworkManager()
    {
        client = new Client(this);
    }

    public void UpdateGameState(byte[] newState)
    {
        // TryWrite is non-blocking and instant. 
        // Because of 'DropOldest', this always succeeds.
        _stateChannel.Writer.TryWrite(newState);
    }


    public byte[] GetGameState()
    {
        byte[] latest = null;
        // Drain the whole buffer to get the absolute newest packet
        while (_stateChannel.Reader.TryRead(out var state))
        {
            latest = state;
        }
        return latest; 
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
