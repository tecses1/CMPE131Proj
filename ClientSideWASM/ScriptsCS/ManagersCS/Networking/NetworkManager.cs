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
    public byte[][] playerStates = {};
    public List<byte[]> objsToAdd = new List<byte[]>();

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
    public void LoadPlayerState(byte[] data, List<Player> otherPlayers)
    {
        if (data == null || data.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 1. Read Metadata
            object[] metaData = NetworkObject.ReadMetaData(reader);
            //cast those.
            string playerName = (string)metaData[0]; //class name cast to string
            string uid = (string)metaData[1]; //uid cast to string
            bool eventOnly = (bool)metaData[2]; //event only check

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
    public byte[] getGameState(List<GameObject> activeObjects)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(activeObjects.Count);

            foreach (var obj in activeObjects)
            {
                // We wrap each object in a 'Size' prefix so we can slice it easily
                // This is the "Frame" for the object data

                
                // 1. Write metadata needed for routing
                obj.WriteMetaData(writer);
                long startPos = writer.BaseStream.Position;
                writer.Write(0); // Placeholder for length

                long dataStart = writer.BaseStream.Position;

                //Get the size of the body (data)
                // 2. Write the synced properties
                obj.Encode(writer);

                // 3. Go back and fill in the length
                long endPos = writer.BaseStream.Position;
                int length = (int)(endPos - dataStart);
                writer.BaseStream.Position = startPos;
                writer.Write(length);
                writer.BaseStream.Position = endPos;
            }
            return ms.ToArray();
        }
    }





    public void loadGameState( ref List<GameObject> activeObjects)
    {
        byte[] data = this.gameState;
        if (data == null || data.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            int incomingCount = reader.ReadInt32();
            Console.WriteLine("Item amount:" + incomingCount);
            HashSet<string> incomingUIDs = new HashSet<string>();
            
            // Temporary storage to hold object data until we decide to Update or Spawn
            // Key: UID, Value: The raw bytes for that specific object
            Dictionary<string, byte[]> packetData = new Dictionary<string, byte[]>();
            Dictionary<string, string> typeMapping = new Dictionary<string, string>();

            for (int i = 0; i < incomingCount; i++)
            {
                

                object[] metaData = NetworkObject.ReadMetaData(reader);
                //cast those.
                string className = (string)metaData[0]; //class name cast to string
                string uid = (string)metaData[1]; //uid cast to string
                bool eventOnly = (bool)metaData[2]; //event only check

                int length = reader.ReadInt32();
                Console.WriteLine("obj length: " + length);
                byte[] objectBody = reader.ReadBytes(length); 
                // Note: BinaryReader strings are length-prefixed, hence the extra bytes calculation logic.
                // Simplified: Just read the remaining bytes of this 'frame'
                
                incomingUIDs.Add(uid);
                packetData[uid] = objectBody;
                typeMapping[uid] = className;
                //Console.WriteLine("Read obj: " + className);
            }

            // 3. DESTROY Phase
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                if (!incomingUIDs.Contains(activeObjects[i].uid.ToString()))
                {
                    // Despawn logic here
                    Console.WriteLine("SYNC: Removing object (no longer exists)");
                    gm.RemoveGameObject(activeObjects[i]);
                    activeObjects.RemoveAt(i);
                }
            }

            // 4. UPDATE or CREATE Phase
            foreach (var kvp in packetData)
            {
                string uid = kvp.Key;
                string className = typeMapping[uid];
                byte[] body = kvp.Value;

                var existingGo = activeObjects.Find(x => x.uid.ToString() == uid);
                using (MemoryStream objMs = new MemoryStream(body))
                using (BinaryReader objReader = new BinaryReader(objMs))
                {
                    if (existingGo != null) {
                        Console.WriteLine("SYNC: Object found, decoding to it.");
                        existingGo.Decode(objReader);
                    }
                    else {
                        Console.WriteLine("SYNC: Spawning new object.");
                        GameObject newobj = CreateGameObject(className, uid);
                        newobj.Decode(objReader);

                        activeObjects.Add(newobj);
                    }
                }

            }
        }
    }
}
