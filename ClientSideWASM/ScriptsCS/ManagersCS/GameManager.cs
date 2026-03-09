
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

    public  NetworkManager nm;

    //Intiialzie the rendre manager
    //List of different game objects.
    Player  player;
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    List<GameObject> otherPlayers = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();
    //Add new objects that spawn.
    private List<GameObject> objsToAdd = new List<GameObject>();
    //Request the host to spawn these.
    private List<GameObject> objsToAddRequest = new List<GameObject>();

     DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;


    Text isLocal;

    InputWrapper cInput;
    

    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        nm.Initialize(this);


        GameManager reference = this;
        player  = new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0));
        //activeObjects.Add(player);
        player.isLocalPlayer = true;
        //Make sure local player and  assigned UID match.
        player.uid = nm.client.assignedUID;
        
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


    public void UpdateInput(InputWrapper e)
    {

        this.cInput = e;


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
    

    //called when the client requests a new item to be added.
    private GameObject SpawnNewObject(string className, string uid)
    {
        GameObject newObj = nm.CreateGameObject(className, uid);

        if (newObj != null)
        {
            activeObjects.Add(newObj);    // Track it
        }

        return newObj;
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

    //Do all the updates and create a gamestate cache. 
    void Generate(DateTime framestamp)
    {
        
        //Get the input for this update
        //player inputs
        InputWrapper[] playerInputs = null;
        if (nm.inputsReceived.Count > 0)
        {
            playerInputs = DecodeInputs(nm.inputsReceived[0]);
            nm.inputsReceived.RemoveAt(0); //remove the input we just processed.

            foreach (InputWrapper input in playerInputs)
            {
                if (input == null)
                {
                    Console.WriteLine("Warning: Null input received from server!");
                    continue;
                }
                Player p = getPlayerWithUID(input.owner);
                if (p == player) continue; //skip our own input, we process that locally for client side prediction.
                if (p == null)
                {
                    Console.WriteLine("Player with UID " + input.owner + " not found for input processing! Creating...");

                    GameManager gameManagerReference = this;
                    Player newPlayer = new Player(ref gameManagerReference, new Transform(0, 0, 50, 50));

                    newPlayer.uid = input.owner;
                    otherPlayers.Add(newPlayer);
                }
                else
                {
                    p.cInput = input;
                    p.Update();
                }
            }
        }
        else
        {
            Console.WriteLine("No player inputs received for this frame.");
        }

        //local input
        InputWrapper e = this.cInput;
        //give the input wrapper an owner and timestamp.
        e.owner = player.uid;
        e.timeStamp = framestamp;


        if (e.keys[5]) //Escape key pressed, exit game.
        {
            Environment.Exit(0);
        }
        //pass off the input to the player.
        player.cInput = e;
        //Update the player.
        player.Update();

        //we're going to do clientSide predicion / remtoe model.... so,
        //first, we update our gamestate based on what we got.

        //Runlocal();


        //Then, we create a snapshot of our gamestate, and cache it. 
        byte[] gameState = this.nm.getGameState(framestamp, otherPlayers,activeObjects);
        GameState gs = new GameState(e, playerInputs, gameState);
        nm.gameStateHistory.Add(gs);

        //Then, send our input over to the host for confirmation
        nm.client.Send("{Input}", e.Encode());


        //if we're hosting, send over our calculated gamestate for others to sync to.
        if (nm.isHost)
        {
            nm.client.Send("{GameStateUpdate}", gameState);
        }
    }
    //Verify that we did what the host did. If not, we need to correct our gamestate to match the host.

    void Sync()
    {
        
    }
    public override async Task Update()
    {
        //Set the timestamp for this update.
        DateTime framestamp = DateTime.Now;
        Generate(framestamp);

        
    }


    public InputWrapper[] DecodeInputs(byte[] data)
    {

        byte[][] playerInputs = NetworkModel.DeserializeJagged(data);

        InputWrapper[] inputs = new InputWrapper[playerInputs.Length];

        for (int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] == null)
            {
                Console.WriteLine("Warning: Null player input at index " + i);
                continue;
            }

            if (playerInputs[i].Length == 0)
            {
                Console.WriteLine("Warning: Empty player input at index " + i);
                continue;
            }
            inputs[i] = InputWrapper.Decode(playerInputs[i]);
            
        }

        return inputs;
    }
    public Player getPlayerWithUID(Guid uid)
    {
        if (player.uid == uid) return player;
        foreach (Player p in otherPlayers)
        {
            if (p.uid == uid) return p;
        }
        Console.WriteLine("player " + uid.ToString() + " not found!");
        return null;
    }

    public void AddNewGameObject(byte[] objData)
    {
        if (objData == null || objData.Length == 0) return;

        using (MemoryStream ms = new MemoryStream(objData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 1. Read Metadata from the head of the stream
            object[] metaData = NetworkObject.ReadMetaData(reader);
            //cast those.
            string className = (string)metaData[0]; //class name cast to string
            string uid = (string)metaData[1]; //uid cast to string

            // 2. Use your Factory to create the instance
            GameObject newObj = SpawnNewObject(className, uid);

            if (newObj != null)
            {
                // 3. Let the new object read the rest of the bytes directly from the stream
                newObj.Decode(reader);
            }
        }
    }

    //Client call to ask server to spawn a new game object.
    public void RequestGameObjectSpawn(GameObject o)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // Write Metadata Header
            o.WriteMetaData(writer);

            // Write the actual property data
            o.Encode(writer); 

            // REquest the gamestate host spawn this object for us.


            nm.client.Send("{SpawnGameObject}",ms.ToArray());
        }
    }

    //Called when the client syncs.
    //Must be seperate from add new game object, as this creates a request by default.
    public void AddNewGameObjectSync(GameObject o)
    {
        objsToAdd.Add(o);
    }

    //Add a game object to the world
    public void AddNewGameObject(GameObject o)
    {
        if (nm.isHost)
        {
            objsToAdd.Add(o);
        }
        else
        {
            //This object was spawned clientside. 
            objsToAddRequest.Add(o);
        }
    }

    
    public void RemoveGameObject(GameObject o)
    {
            objsToRemove.Add(o);
    }
}
