
using Microsoft.JSInterop;
using Shared;
using System.Text.Json;
using System.Numerics;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Text.RegularExpressions;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.


public class GameManager : RenderManager
{
    //em for collision

    public  NetworkManager nm;

    public GameLogic gl;

    public LocalPlayer localPlayer;
    public List<ClientPlayer> clientPlayers = new List<ClientPlayer>();
    public List<GameObject> backgroundStars = new List<GameObject>();

    Text isLocal;

    ClientInputWrapper cInput;
    
   float lastTime = 0;

    Stopwatch renderTimer = new Stopwatch();
    Stopwatch updateTimer = new Stopwatch();



    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        nm.Initialize(this);
        
        //initialize the game logic which handles the game behavior.
        this.gl = new GameLogic();

        // Pre-populate asteroids
        for (int i = 0; i < 50; i++) // choose a number based on density
        {
            Asteroid a = Asteroid.GenerateAsteroid();
            gl.AddGameObject(a);
        }

        // Pre-populate health packs
        for (int i = 0; i < 10; i++) // fewer than asteroids, to avoid clutter
        {
            Healthpack hp = Healthpack.GenerateHealthPack();
            gl.AddGameObject(hp);
        }

        GenerateStars();
        Transform t = new Transform(Settings.CanvasWidth/2, 25, 300,50);
        isLocal = new Text("Playing Locally ", t);
        isLocal.worldSpace = false;

        //Create local player, add to GameLogic? 
        localPlayer = new LocalPlayer(this, new Transform(GameConstants.worldSizeX/2,GameConstants.worldSizeY/2,50,50));
        //make sure local player has UID for finding updates.
        localPlayer.uid = nm.client.assignedUID;
        localPlayer.isLocalPlayer = true;
        //Add the local player to the players.
        gl.AddPlayer(localPlayer);
        //add groups to render manager by reference.


        RegisterGroupToRender(backgroundStars);
        RegisterGroupToRender(gl.GetActiveObjects());
        RegisterGroupToRender(gl.GetPlayers());
        RegisterObjToRender(localPlayer);


    }
    

void GenerateStars()
{
    Random r = new Random();
    
    // Calculate total area and desired density
    long totalArea = (long)GameConstants.worldSizeX * GameConstants.worldSizeY;
    
    // Instead of iterating 25 million times, we calculate how many stars we want.
    // Adjust 'DensityConstant' to get the look you want (e.g., 0.0001 for 1 star per 10k pixels)
    int starCount = (int)(totalArea * Settings.Sparseness / Math.Sqrt(totalArea)); 
    Console.WriteLine("spawning " + starCount + " stars for bg");
    for (int i = 0; i < starCount; i++)
    {
        int x = r.Next(0, GameConstants.worldSizeX);
        int y = r.Next(0, GameConstants.worldSizeY);
        
        int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize, Settings.minSize, Settings.maxSize);
        
        Transform t = new Transform(x, y, size, size);
        backgroundStars.Add(new Star(t));
    }
}


    public void UpdateInput(ClientInputWrapper e)
    {
        this.cInput = e;
    }

    public override void  Update()
    {
        updateTimer.Restart();
        //check if we're a local gamestate, if so, update locally. for testing.
        if (!nm.client.isConnected()) {
            cInput.OverwriteCameraToWorldPos(this);
            this.localPlayer.cInput = (InputWrapper)cInput;
            
            this.gl.Update();
            return;
        }else

        if (nm.myLobby == "")
        {
            cInput.OverwriteCameraToWorldPos(this);
            this.localPlayer.cInput = (InputWrapper)cInput;
            
            this.gl.Update();
            return;
        }
        //if we're a network game, send over our input, and wait for the host to send us the gamestate, then update our gamestate to match the host's.
        //cast the camera position locally to a world pos, for calculations on server.
        cInput.OverwriteCameraToWorldPos(this);
        //make sure our input has our UID.
        this.cInput.owner = nm.client.assignedUID;
        //send our input over to the server!
        this.nm.client.Send("{Input}",cInput.ToBytes());
        localPlayer.cInput = (InputWrapper)cInput; //make sure to update our local player with the input wrapper so it can move while we wait for the gamestate update from the server.
        //update the local player immediately exactly as game logic would.
        localPlayer.UpdateBase();


        base.Update();
        this.stateSize = nm.StateQueue.lastSize;
        updateTime = (int)updateTimer.ElapsedMilliseconds;


}

    
    public void GameStateCheck()
    {
        //After the gamestate is loaded, we may have added a player. Because GL does not send events yet,
        //this is a quick fix. Later, I need to have the GameLogic class attempt to send events such as
        //"On player connected" so we can overwrite the classes it makes by default with render classes.
        foreach (Player p in gl.GetPlayers())
        {
            if (p == localPlayer) continue; //ignore the local player, we already know this one is fixed.
            if (p.GetType() == typeof(Player)) //the game logic class created a player.
            {
                Console.WriteLine("Player class");

                gl.RemovePlayer(p);
                //replace it with our client player that handles rendering.
            
                ClientPlayer cp = new ClientPlayer(this, p.transform);
                //IMPORTANT, or it will make 1020935 players.... give the CP the same UID as the old player its replacing.
                cp.uid = p.uid;
                cp.playerName.text = p.playerNameString;
                gl.AddPlayer(cp);
                clientPlayers.Add(cp);

                RegisterObjToRender(cp);//make sure we tell the render manager HEY! This object needs to be rendered!
                //Because we modififed the collection, we have to close this loop.
                break;

            }

        }
    }




    // TODO: Everything run from host perspective, so currently other players manually updated here BUT
    // player scores all go to the host, other players cant take damage, weird stuff happens when refreshing

    public override void Render(float timestamp)
    {
        float deltaTime = timestamp - lastTime;
        lastTime = timestamp;

        this.renderTimer.Restart();
        //Console.WriteLine("Calling render!");
        if (!nm.client.isConnected()) {
            //isLocal.Draw(this);
            isLocal.disableRender = false;
            nm.isHost = true;
        }
        else
        {
            isLocal.disableRender = true;
        }
        if (nm.myLobby == "")
        {
            isLocal.text = "Playing Solo (No Lobby)";
        }
        
        double renderTime = timestamp - InterpolationDelay;
        long arrivalTime = nm.PeekArrivalTime();
        //Console.WriteLine("Arrival time: " + arrivalTime + ", Rendertime: " + renderTime);
            // While the next packet in line is "due" to be played...
        if (arrivalTime != -1 && arrivalTime <= renderTime) {
            //Console.WriteLine("loading tick: " + nm.PeekTick());
            byte[] gameState = nm.GetGameState(out arrivalTime); //redundant. will fix if really truly unneccesary.
            
            //Console.WriteLine("DEBUG: PROCESSING GAME STATE!!!!");
            // We need to know the time gap between the state we are LEAVING
            // and the state we just LOADED.
            _lastTransformTime = _nextTransformTime;
            _nextTransformTime = arrivalTime;

            _currentInterpolationDuration = (float)(_nextTransformTime - _lastTransformTime);
            _timeSinceLastLoad = 0;

            gl.LoadGameState(gameState);
            GameStateCheck();
            //CenterCameraOn(this.localPlayer.transform, false, false);
            localPlayer.CenterCameraOnMe();

        }
        else
        {
            _timeSinceLastLoad += deltaTime;
        }
        
        

        localPlayer.Render(deltaTime); // render local only stuff.

        foreach (ClientPlayer cp in clientPlayers)
        {
            cp.Render(deltaTime); //render local only stuff, like names and healthbars.
        }
        //localPlayer.CenterCameraOnMe((float)renderTime);
        //localPlayer.CenterCameraOnMe(deltaTime);
        base.Render(deltaTime); 
    
        this.renderTime = (int)renderTimer.ElapsedMilliseconds;
    }

    
    //Verify that we did what the host did. If not, we need to correct our gamestate to match the host.







}
