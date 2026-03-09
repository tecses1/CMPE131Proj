
using Microsoft.JSInterop;
using Shared;
using System.Text.Json;
using System.Numerics;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.


public class GameManager : RenderManager
{
    //em for collision
    EventManager eventManager = new EventManager();
    //Intiialzie the rendre manager
    //List of different game objects.
    Player  player;
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    List<Player> otherPlayers = new List<Player>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();

    private List<GameObject> objsToAdd = new List<GameObject>();

     DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;

    public  NetworkManager nm;

    Text isLocal;
    

    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        GameManager reference = this;
        player  = new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0));
        //activeObjects.Add(player);
        player.isLocalPlayer = true;
        GenerateStars();
        Transform t = new Transform(Settings.CanvasWidth/2, 25, 300,50);
        isLocal = new Text("Playing Locally ", ref t);
        isLocal.worldSpace = false;
    }
    

    void GenerateStars()
    {
        //iterate through canvas coordinates.
        Random r = new Random();
        for (int i = 0; i < worldSizeX; i++)
        {
            for (int j = 0; j < worldSizeY; j++)
            {   
                double chance = r.NextDouble();
                double sizeModifier = Math.Sqrt(worldSizeX * worldSizeY);
                if (chance < Settings.Sparseness / sizeModifier)
                {
                    int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize,Settings.minSize, Settings.maxSize);
                    Transform t = new Transform(i,j,size,size);
                    GameManager reference = this;
                    Star s = new Star(ref reference, t);
                    
                    backgroundStars.Add(s);
                }

            }
        }
        
    }


    public void UpdatePlayer(InputWrapper e)
    {
        if (e.keys[5]) //Escape key pressed, exit game.
        {
            Environment.Exit(0);
        }
        ((Player)player).cInput = e;

    }


    public void SpawnAsteroid()
    {
        Random r = new Random();
        int size = (int)(20 + r.NextDouble() * 30);
        if (r.NextInt64(0,15) == 8)
        {
            size = size * 5;
        }
                
        int spawnX,spawnY;
        int edge = r.Next(0,4);
        switch (edge)
        {
            case 0: //top
                spawnX = r.Next(0, worldSizeX);
                spawnY = -size;
                break;
            case 1: //right
                spawnX = worldSizeX + size;
                spawnY = r.Next(0, worldSizeY);
                break;
            case 2: //bottom
                spawnX = r.Next(0, worldSizeX);
                spawnY = worldSizeY + size;
                break;
            case 3: //left
                spawnX = -size;
                spawnY = r.Next(0, worldSizeY);
                break;
            default:
                spawnX = -size;
                spawnY = r.Next(0, worldSizeY);
                break;

        }
        Transform t = new Transform(spawnX, spawnY, size, size);
        GameManager reference = this;
        Asteroid a = new Asteroid(ref reference, t,r.Next(1,3));
        a.SetTarget(player.transform);
        activeObjects.Add(a);
    }

/*json serializer defunct.
    public byte[] getGamesstate()
    {
        byte[][] gameState = new byte[activeObjects.Count][];
        for (int i = 0; i < gameState.Length; i++)
        {
            gameState[i] = activeObjects[i].Encode();
        }
        
        return JsonSerializer.Serialize(gameState); //Make the array of obj jsons a single string

    }*/
    public byte[] getGameState()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(activeObjects.Count);

            foreach (var obj in activeObjects)
            {
                // We wrap each object in a 'Size' prefix so we can slice it easily
                // This is the "Frame" for the object data
                long startPos = writer.BaseStream.Position;
                writer.Write(0); // Placeholder for length

                long dataStart = writer.BaseStream.Position;
                
                // 1. Write metadata needed for routing
                writer.Write(obj.GetType().Name);
                writer.Write(obj.uid.ToString()); // Or writer.Write(obj.uid) if it's a Guid/long

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


    public byte[] getPlayerState(Player p)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // 1. Metadata (The Header)
            writer.Write(Settings.name);  // Player Name
            writer.Write(p.uid.ToString()); // UID

            // 2. Data (The Payload)
            p.Encode(writer); // Uses your refined reflection-based encoder

            return ms.ToArray();
        }
    }


    public void loadGameState(byte[] data)
    {
        if (data == null || data.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            int incomingCount = reader.ReadInt32();
            HashSet<string> incomingUIDs = new HashSet<string>();
            
            // Temporary storage to hold object data until we decide to Update or Spawn
            // Key: UID, Value: The raw bytes for that specific object
            Dictionary<string, byte[]> packetData = new Dictionary<string, byte[]>();
            Dictionary<string, string> typeMapping = new Dictionary<string, string>();

            for (int i = 0; i < incomingCount; i++)
            {
                
                int length = reader.ReadInt32();
                string className = reader.ReadString();
                string uid = reader.ReadString();
                
                byte[] objectBody = reader.ReadBytes(length - (className.Length + 1) - (uid.Length + 1)); 
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
                    if (existingGo != null) existingGo.Decode(objReader);
                    else SpawnNewObject(className, uid).Decode(objReader);
                }

            }
        }
    }

    //called when the client requests a new item to be added.
    private GameObject SpawnNewObject(string className, string uid)
    {
        GameObject newObj = nm.SpawnNewObject(className, uid);

        if (newObj != null)
        {
            activeObjects.Add(newObj);    // Track it
        }

        return newObj;
    }




    public void loadPlayerStates(byte[][] playerStates)
    {
        foreach (byte[] playerState in playerStates)
        {
            nm.LoadPlayerState(playerState, otherPlayers);
        }
    }



    // TODO: Everything run from host perspective, so currently other players manually updated here BUT
    // player scores all go to the host, other players cant take damage, weird stuff happens when refreshing
    public void Runlocal()
    {
        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }
        objsToRemove.Clear();

        foreach (GameObject go in objsToAdd)
        {
            activeObjects.Add(go);
        }
        objsToAdd.Clear();

        //old collisions for reference  
        //Update stars.
        /*
        foreach (GameObject other in backgroundStars)
        {
            other.Update();
        }*/
        //Update active objects. Check for collision withj stars.
        // Console.WriteLine(activeObjects);
        
        // foreach (GameObject go in activeObjects)
        // {
        //     go.Update();
        //     if (go.disableCollision) continue; //If the object is already dead, skip collision.
            
        //     foreach (GameObject collideGO in activeObjects)
        //     {
        //         if (collideGO.disableCollision) continue;
        //         // Console.WriteLine(go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
                
        //         if (go.CollideWith(collideGO))
        //         {
        //             // Console.WriteLine("Should be dead but no respawn yet");
        //             // Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
        //             if (go.GetType().Name == "Projectile" && collideGO.GetType().Name == "Asteroid")
        //             {
        //                 go.Kill();
        //                 ((Asteroid)collideGO).hp -= ((Projectile)go).damage;
        //                 if (((Asteroid)collideGO).hp <= 0)
        //                 {
        //                     ((Player)player).AddScore(10);
        //                     collideGO.Kill();
        //                 }
        //             }
        //             if (go.GetType().Name == "Asteroid" && collideGO.GetType().Name == "Player")
        //             {
        //                 // Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
        //                 ((Player)collideGO).TakeDamage(10);
        //                 go.Kill();
        //             }
        //         }
        //     }
        eventManager.Clear();//clear before they update on this frame.
        foreach (GameObject go in activeObjects)
        {
            //Update and register in same lop. No need to run twice. On.
            go.Update();
            eventManager.Register(go);

        } 

        eventManager.Register(player);

        // TODO: probably update this when main player updates???
        foreach (Player p in otherPlayers)
        {
            eventManager.Register(p);
        }


            // process collisions
        eventManager.ProcessCollisions((go, collideGO) =>
        {
            // Console.WriteLine(go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
            
            if (!go.CollideWith(collideGO)) return;
            if ((go is Projectile && collideGO is Player) ||
                (go is Player && collideGO is Projectile))
                return;
            if (go is Projectile proj && collideGO is Asteroid asteroid)
            {
                proj.Kill();
                asteroid.hp -= proj.damage;
                if (asteroid.hp <= 0)
                {
                    getPlayerWithUID(proj.owner).AddScore(10);
                    // player.AddScore(10);
                    asteroid.Kill();
                }
            }
            else if (go is Asteroid asteroid2 && collideGO is Projectile proj2)
            {
                proj2.Kill();
                asteroid2.hp -= proj2.damage;
                if (asteroid2.hp <= 0)
                {
                   getPlayerWithUID(proj2.owner).AddScore(10);
                    asteroid2.Kill();
                }
            }
            else if (go is Asteroid a && collideGO is Player p)
            {
                p.TakeDamage(10);
                a.Kill();
            }

        });
            //if obj is in the bounds of the canvas, we can render.


        if ((DateTime.Now - counter).TotalSeconds >= AsteroidSpawnCooldownSeconds)
        {
            SpawnAsteroid();
            counter = DateTime.Now;
        }
    }

    public override async Task Render()
    {

        if (!nm.client.isConnected()) {
            isLocal.Draw(this);
            nm.isHost = true;
        }
        if (nm.myLobby == "")
        {
            isLocal.text = "Playing Solo (No Lobby)";
        }
        

        foreach (GameObject other in backgroundStars)
        {
            AddObjToRender(other);//tell RenderManager to Render the object.
            other.Render(); //Call custom render, if it has one. (Syncs text and rect draw calls)
        }

        foreach (GameObject go in activeObjects)
        {
            AddObjToRender(go); //tell RenderManager to Render the object.
            go.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        }   

        foreach (GameObject go in otherPlayers)
        {
            AddObjToRender(go); //tell RenderManager to Render the object.
            go.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        }
        AddObjToRender(player);//tell RenderManager to Render the object.
        player.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        await base.Render(); //Do whatever the RenderManager wants to do by itself. probably the official render calls.

    }


    public override async Task Update()
    {
        await base.Update();

        //Update the player, always.
        player.Update();


        
        if (nm.client.isConnected() && nm.myLobby != "")
        {
            //Send over our position first, as thats needed on all.

            //Check if we're the host first. Theres gotta be a better way then calling each update. Will ivnestigate.
            
            if (this.nm.isHost) //we're hosting ,so we'll send our gamestate.
            {
                Runlocal(); //Update the game locally, the nsend our state to the server.
                byte[] gamestate = this.getGameState();
                await nm.client.Send("{GameUpdate}",gamestate);
                
                foreach (byte[] objToAdd in nm.objsToAdd)
                {
                    
                    AddNewGameObject(objToAdd);

                }
                nm.objsToAdd.Clear();

            }
            else
            {
                //load gamestate auto recvied in Network Manager./
                loadGameState(nm.gameState);

            }

            //send my player data to the server. It will be relayed to the others.
            await nm.client.Send("{PlayerUpdate}", getPlayerState(player));//player.Encode());
            loadPlayerStates(nm.playerStates);
        }
        else
        {
            Runlocal();
        }
    }
    public Player getPlayerWithUID(Guid uid)
    {
        if (player.uid == uid) return player;
        foreach (Player p in otherPlayers)
        {
            if (p.uid == uid) return p;
        }
        return null;
    }

    public void AddNewGameObject(byte[] objData)
    {
        if (objData == null || objData.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(objData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 1. Read Metadata from the head of the stream
            string className = reader.ReadString();
            string uid = reader.ReadString();

            // 2. Use your Factory to create the instance
            GameObject newObj = SpawnNewObject(className, uid);

            if (newObj != null)
            {
                // 3. Let the new object read the rest of the bytes directly from the stream
                newObj.Decode(reader);
            }
        }
    }

    //local call. Might need to send data to server.
    public void AddNewGameObject(GameObject o)
    {
        if (nm.isHost)
        {
            objsToAdd.Add(o);
        }
        else
        {
            // 1. Create a temporary buffer for this specific object's data
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // Write Metadata Header
                writer.Write(o.GetType().Name);
                writer.Write(o.uid.ToString());

                // Write the actual property data
                o.Encode(writer); 

                // 2. Wrap it in your Binary Packet

                nm.client.Send("{SpawnGameObject}",ms.ToArray());
            }
        }
    }

    
    public void RemoveGameObject(GameObject o)
        {
            objsToRemove.Add(o);
        }
    }
