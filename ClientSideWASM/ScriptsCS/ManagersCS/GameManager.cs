
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
    GameObject player;
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();

    private List<GameObject> objsToAdd = new List<GameObject>();

     DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;

    NetworkManager nm;

    Text isLocal;
    
    string currentGamestateJson = "";
    bool isHost = false;

    bool recieveLoopIsAlive = false;
    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        GameManager reference = this;
         player  = new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0));

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
    public override async Task Render()
    {

        if (!nm.client.isConnected()) isLocal.Draw(this);
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
        AddObjToRender(player);//tell RenderManager to Render the object.
        player.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        await base.Render(); //Do whatever the RenderManager wants to do by itself. probably the official render calls.

    }
    public string[][] getGamesstate()
    {
        string[][] gameState = new string[activeObjects.Count][];
        for (int i = 0; i < gameState.Length; i++)
        {
            List<string> encoded = new List<string>();

            GameObject go = activeObjects[i];
            string goName = go.GetType().Name;
            string uid = go.uid.ToString();
            string[] state = activeObjects[i].Encode();

            encoded.Add(goName);
            encoded.Add(uid);
            encoded.AddRange(state);
            
            gameState[i] = encoded.ToArray();
        }
        
        return gameState;

    }
    public void loadGamesstate()
    {
        if (currentGamestateJson == "") return; //No gamestate to load.
        Console.WriteLine("Loading gamestate: " + currentGamestateJson);
        string[][] gameState = JsonSerializer.Deserialize<string[][]>(currentGamestateJson);
        //Console.WriteLine("Updating gamestate");
        // 1. Collect all UIDs from the incoming data
        HashSet<string> incomingUIDs = new HashSet<string>();
        
        foreach (string[] data in gameState)
        {
            incomingUIDs.Add(data[1]); // Assuming index 1 is the UID
        }

        // 2. DESTROY: Remove objects that are no longer in the game state
        // We loop backwards so we can safely remove items from the list
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (!incomingUIDs.Contains(activeObjects[i].uid.ToString()))
            {
                GameObject toDestroy = activeObjects[i];
                this.activeObjects.Remove(toDestroy); // Mark for removal after the loop to avoid modifying the list while iterating
            }
        }

        // 3. UPDATE or ADD: Process the incoming data
        foreach (string[] gameStateObj in gameState)
        {
            string objName = gameStateObj[0];
            string uid = gameStateObj[1];
            string[] state = gameStateObj.Skip(2).ToArray();

            // Check if we already have this object
            GameObject existingGo = activeObjects.Find(x => x.uid.ToString() == uid);

            if (existingGo != null)
            {
                // Update existing
                existingGo.Decode(state);
            }
            else
            {
                // ADD: This UID wasn't found in activeObjects, so spawn it
                if (objName == "Asteroid")
                {
                    Transform t = new Transform(0, 0, 0, 0);
                    GameManager reference = this;
                    Asteroid a = new Asteroid(ref reference, t,1);
                    a.Decode(state);
                    this.activeObjects.Add(a);
                }
                else if (objName == "Projectile")
                {
                    Transform t = new Transform(0, 0, 0, 0);
                    GameManager reference = this;
                    Projectile p = new Projectile(ref reference, t, new Vector2(0,0));
                    p.Decode(state);
                    this.activeObjects.Add(p);
                }
            }
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



        foreach (GameObject go in activeObjects)
        {
            go.Update();
            if (go.disableCollision) continue; //If the object is already dead, skip collision.
            
            foreach (GameObject collideGO in activeObjects)
            {
            if (collideGO.disableCollision) continue;
                
            if (go.CollideWith(collideGO))
            {
                //Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
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

    public async Task requestGamestateFromServer()
    {
        Console.WriteLine("recieving indefnitely.   ");
        recieveLoopIsAlive = true;
        while (recieveLoopIsAlive)
        {
            Packet p = await nm.client.SendWithResponse("{RequestGameState}");
            currentGamestateJson = p.Args[0];
        }


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
                await nm.client.Send("{GameUpdate}",JsonSerializer.Serialize<string[][]>(this.getGamesstate()));
                
            }
            else
            {
                //Launch the RECV thread async so it doesn't pause our update thread.
                if(recieveLoopIsAlive == false) _ = requestGamestateFromServer();
                //Request the gamestate from the server.
                loadGamesstate();
            }


        }
        else
        {
            Runlocal();
        }

        
        
    }
    public void AddNewGameObject(GameObject o)
    {
        objsToAdd.Add(o);
    }
    public void RemoveGameObject(GameObject o)
    {
        objsToRemove.Add(o);
    }
}
