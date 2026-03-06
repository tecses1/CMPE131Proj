
using Microsoft.JSInterop;
using Shared;
using System.Text.Json;
using System.Numerics;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager : RenderManager
{

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
    
    int testctr = 0;
    string lastencode = "";
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

    public string getGamesstate()
    {
        string[][] gameState = new string[activeObjects.Count][];
        for (int i = 0; i < gameState.Length; i++)
        {
            gameState[i] = activeObjects[i].Encode();
        }
        
        return JsonSerializer.Serialize(gameState); //Make the array of obj jsons a single string

    }
    public string[] getPlayerState(Player p)
    {
        List<string> encoded = new List<string>();

        string playerName = Settings.name;
        string[] state = p.Encode();

        encoded.Add(playerName);
        encoded.AddRange(state);

        return encoded.ToArray();
    }
    public void loadGameState(string currentGamestateJson)
    {
        if (string.IsNullOrWhiteSpace(currentGamestateJson)) return;

        // 1. Break the big JSON into the individual object arrays
        // Expects: [ ["Player", "uid1", "x", "y"], ["Asteroid", "uid2", "x", "y"] ]
        List<string[]> incomingObjects = JsonSerializer.Deserialize<List<string[]>>(currentGamestateJson);
        if (incomingObjects == null) return;

        // 2. Map incoming UIDs for quick "Destroy" and "Update" checks
        HashSet<string> incomingUIDs = new HashSet<string>();
        foreach (var objData in incomingObjects)
        {
            incomingUIDs.Add(objData[1]); // Index 1 is the UID
        }

        // 3. DESTROY Phase: If it's in our scene but NOT in the JSON, it despawned
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var localObj = activeObjects[i];
            if (!incomingUIDs.Contains(localObj.uid.ToString()))
            {
                // Trigger any in-game destruction logic (explosions, etc.)
                // localObj.OnDestroy(); 
                activeObjects.RemoveAt(i);
            }
        }

        // 4. UPDATE or CREATE Phase
        foreach (string[] objData in incomingObjects)
        {
            string className = objData[0];
            string uid = objData[1];
            string[] stateData = objData.Skip(1).ToArray(); // Everything after ClassName and UID

            // Try to find the existing object by UID
            GameObject existingGo = activeObjects.Find(x => x.uid.ToString() == uid);

            if (existingGo != null)
            {
                // UPDATE: Object exists, hand it the rest of the array to decode
                existingGo.Decode(stateData);
            }
            else
            {
                // CREATE: Object is new to this client
                SpawnNewObject(className, stateData);
            }
        }
    }

    private void SpawnNewObject(string className, string[] stateData)
    {
        GameObject newObj = null;
        
        // Setup common references (GameManager, etc.)
        GameManager gm = this;
        Transform defaultT = new Transform(0,0,0,0);

        // Factory logic based on the ClassName string at index [0]
        switch (className)
        {
            case "Asteroid":
                newObj = new Asteroid(ref gm, defaultT, 1);
                break;
            case "Projectile":
                newObj = new Projectile(ref gm, defaultT, new Vector2(0,0));
                break;
            // Add more types here as your game grows
        }

        if (newObj != null)
        {
            newObj.Decode(stateData);     // Apply initial position/rotation/state
            activeObjects.Add(newObj);    // Track it
        }
    }
    public void LoadPlayerState(string playerStateJson)
    {
        //return;
        if (string.IsNullOrEmpty(playerStateJson)) return;

        // 1. Deserialize the single player array
        // Expects: ["Name", "UID", "X", "Y", "Rot", "State"...]
        string[] playerUpdate = JsonSerializer.Deserialize<string[]>(playerStateJson);

        if (playerUpdate == null || playerUpdate.Length < 2) return;

        string playerName = playerUpdate[0];
        string uid = playerUpdate[2].Trim('"');

        //Console.WriteLine("atempting decode of player: "  + playerName + ", guid = " + uid);

        // 2. Find if this specific player already exists in our active list
        Player existingPlayer = otherPlayers.Find(p => p.uid.ToString()  == uid);
        
        if (existingPlayer != null)
        {
            // UPDATE: Just decode the remaining data (skipping Name and UID)
            //Console.WriteLine("DECODING PLAYER DEBUG");
            existingPlayer.Decode(playerUpdate.Skip(2).ToArray());
        }
        else
        {
            // ADD: We haven't seen this player UID before, so spawn them
            //Console.WriteLine($"New player detected: {playerName} ({uid})");
            
            Transform t = new Transform(0, 0, 0, 0);
            GameManager reference = this;
            
            Player newPlayer = new Player(ref reference, t);
            newPlayer.playerName.text = playerName;
            newPlayer.playerName.worldSpace = true;
            newPlayer.Decode(playerUpdate.Skip(2).ToArray());
            
            
            //Console.WriteLine("New player GUID " + newPlayer.uid.ToString());
            
            this.otherPlayers.Add(newPlayer);
    }
}
    public void loadPlayerStates(List<string> playerStates)
    {
        foreach (string playerState in playerStates)
        {
            LoadPlayerState(playerState);
        }
    }
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

        //Update stars.
        /*
        foreach (GameObject other in backgroundStars)
        {
            other.Update();
        }*/
        //Update active objects. Check for collision withj stars.
        // Console.WriteLine(activeObjects);
        
        foreach (GameObject go in activeObjects)
        {
            go.Update();
            if (go.disableCollision) continue; //If the object is already dead, skip collision.
            
            foreach (GameObject collideGO in activeObjects)
            {
                if (collideGO.disableCollision) continue;
                // Console.WriteLine(go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
                
                if (go.CollideWith(collideGO))
                {
                    // Console.WriteLine("Should be dead but no respawn yet");
                    // Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
                    if (go.GetType().Name == "Projectile" && collideGO.GetType().Name == "Asteroid")
                    {
                        go.Kill();
                        ((Asteroid)collideGO).hp -= ((Projectile)go).damage;
                        if (((Asteroid)collideGO).hp <= 0)
                        {
                            ((Player)player).AddScore(10);
                            collideGO.Kill();
                        }
                    }
                    if (go.GetType().Name == "Asteroid" && collideGO.GetType().Name == "Player")
                    {
                        // Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
                        ((Player)collideGO).TakeDamage(10);
                        go.Kill();
                    }
                }
            }
            

            //if obj is in the bounds of the canvas, we can render.
        }


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
                string gamestate = this.getGamesstate();
                await nm.client.Send("{GameUpdate}",gamestate);
                
                foreach (string objToAdd in nm.objsToAdd)
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
            await nm.client.Send("{PlayerUpdate}", JsonSerializer.Serialize<string[]>(getPlayerState(player)));//player.Encode());
            loadPlayerStates(nm.playerStatesJSON);
            nm.playerStatesJSON.Clear(); //Clear the player states after loading, so we dont keep adding them.
        }
        else
        {
            Runlocal();
        }
    }
    public void AddNewGameObject(string JSON)
    {
        //This is a request from the server to spawn a new game object. We will decode the JSON to figure out what to spawn and where.
        string[] data = JsonSerializer.Deserialize<string[]>(JSON);
        string objName = data[0];
        SpawnNewObject(objName, data.Skip(1).ToArray());
        
    }
    public void AddNewGameObject(GameObject o)
    {
        if (nm.isHost)
        {
            objsToAdd.Add(o);
        }
        else
        {
            //if we're not the host, lets send a request to spawn this game object.
            string[] encoded = o.Encode();
            nm.client.Send("{SpawnGameObject}", JsonSerializer.Serialize<string[]>(encoded));
        }
        
    }
    public void RemoveGameObject(GameObject o)
    {
        objsToRemove.Add(o);
    }
}
