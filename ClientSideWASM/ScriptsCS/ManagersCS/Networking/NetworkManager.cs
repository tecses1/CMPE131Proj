using System;
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
    public byte[][] playerStates = {};
    public List<byte[]> objsToAdd = new List<byte[]>();

    public NetworkManager(GameManager gm)
    {
        client = new Client(this);
        this.gm = gm;

    }
    public async Task Initialize()
    {
        
        await client.ConnectToServer();
    }
    public void LoadPlayerState(byte[] data, List<Player> otherPlayers)
    {
        if (data == null || data.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 1. Read Metadata
            string playerName = reader.ReadString();
            string uid = reader.ReadString();

            // 2. Find or Create
            Player existingPlayer = otherPlayers.Find(p => p.uid.ToString() == uid);

            if (existingPlayer != null)
            {
                // UPDATE
                existingPlayer.Decode(reader);
            }
            else
            {
                // ADD: New player detected
                Transform t = new Transform(0, 0, 0, 0);
                
                Player newPlayer = new Player(ref gm, t);
                newPlayer.uid = Guid.Parse(uid); // Ensure the UID is set!
                newPlayer.playerName.text = playerName;
                newPlayer.playerName.worldSpace = true;

                // Decode the rest of the properties from the stream
                newPlayer.Decode(reader);
                
                otherPlayers.Add(newPlayer);
            }
        }
    }
    public GameObject SpawnNewObject(string className, string uid)
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

        return newObj;
    }
}
