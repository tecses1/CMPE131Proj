using Shared;
using System.Numerics;
namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    GameManager gm;
    public string myLobby = "";
    public bool isHost = false;
    public byte[] gameState;
    public List<byte[]> inputsReceived = new List<byte[]>();
    public List<byte[]> objsToAdd = new List<byte[]>();
    public List<byte[]> serverGameStates = new List<byte[]>();
    public NetworkManager()
    {
        client = new Client(this);
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
