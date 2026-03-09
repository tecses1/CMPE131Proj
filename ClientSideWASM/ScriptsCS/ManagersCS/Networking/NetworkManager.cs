using System;
using Shared;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
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

    public List<GameState> gameStateHistory = new List<GameState>();
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

    public GameObject CreateGameObject(string className, string uid)
    {
        GameObject newObj = null;
        
        // Setup common references (GameManager, etc.)
        Transform defaultT = new Transform(0,0,0,0);

        // Factory logic based on the ClassName string at index [0]
        switch (className)
        {
            case "Asteroid":
                newObj = new Asteroid(ref gm, defaultT, 1);
                break;
            case "Projectile":
                Projectile p = new Projectile(ref gm, defaultT, new Vector2(0,0));
                newObj = p;
                break;
            // Add more types here as your game grows
        }
        newObj.uid = Guid.Parse(uid);

        return newObj;
    }
    //Go through all groups passed, and, in order, write their meta data and object data.
    public byte[] getGameState(DateTime frameStamp, params List<GameObject>[] groups)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            foreach (List<GameObject> group in groups)
            {
                //write the group size.
                writer.Write(group.Count);
                foreach (GameObject go in group)
                {
                    go.WriteMetaData(writer);
                    go.Encode(writer);


                }
            }
            return ms.ToArray();
        }
    }





    public void loadGameState( ref List<GameObject> activeObjects)
    {
        
    }
}
