
using Microsoft.JSInterop;
using Shared;
using System.Text.Json;
using System.Numerics;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.


public class GameManager : RenderManager
{
    //em for collision

    public  NetworkManager nm;

    public GameLogic gl;

    public LocalPlayer localPlayer;
    public List<GameObject> backgroundStars = new List<GameObject>();

    Text isLocal;

    ClientInputWrapper cInput;
    

    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        nm.Initialize(this);
        
        //initialize the game logic which handles the game behavior.
        this.gl = new GameLogic();


        GenerateStars();
        Transform t = new Transform(Settings.CanvasWidth/2, 25, 300,50);
        isLocal = new Text("Playing Locally ", ref t);
        isLocal.worldSpace = false;

        //Create local player, add to GameLogic? 
        localPlayer = new LocalPlayer(this, new Transform(0,0,100,100));
        //make sure local player has UID for finding updates.
        localPlayer.uid = nm.client.assignedUID;
        localPlayer.isLocalPlayer = true;
        //Add the local player to the players.
        gl.AddPlayer(localPlayer);
    }
    

    void GenerateStars()
    {
        //iterate through canvas coordinates.
        Random r = new Random();
        for (int i = 0; i < GameConstants.worldSizeX; i++)
        {
            for (int j = 0; j < GameConstants.worldSizeY; j++)
            {   
                double chance = r.NextDouble();
                double sizeModifier = Math.Sqrt(GameConstants.worldSizeX * GameConstants.worldSizeY);
                if (chance < Settings.Sparseness / sizeModifier)
                {
                    int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize,Settings.minSize, Settings.maxSize);
                    Transform t = new Transform(i,j,size,size);
                    Star s = new Star(t);
                    //s.RegisterGameLogic(gl);
                    
                    backgroundStars.Add(s);
                }

            }
        }
        
    }


    public void UpdateInput(ClientInputWrapper e)
    {
        this.cInput = e;
    }

    public override async Task Update()
    {

        //pass local input to the local player
        //localPlayer.cInput = this.cInput;


        //cast the camera position locally to a world pos, for calculations on server.
        cInput.OverwriteCameraToWorldPos(this);
        //make sure our input has our UID.
        this.cInput.owner = nm.client.assignedUID;
        //send our input over to the server!
        this.nm.client.Send("{Input}",cInput.ToBytes());
    
        //Console.WriteLine(cInput.ToString());


        //Update our gamestate from the input.
        if (nm.gameState != null) //wait for the gamestate to populate.
        gl.LoadGameState(nm.gameState);
        else Console.WriteLine("Gamestate is null. Are we recieving? ");
    }





    // TODO: Everything run from host perspective, so currently other players manually updated here BUT
    // player scores all go to the host, other players cant take damage, weird stuff happens when refreshing

    public override async Task Render()
    {
        //Console.WriteLine("Calling render!");
        if (!nm.client.isConnected()) {
            isLocal.Draw(this);
            nm.isHost = true;
        }
        if (nm.myLobby == "")
        {
            isLocal.text = "Playing Solo (No Lobby)";
        }
        
        //Console.WriteLine("Stars render!");

        foreach (GameObject other in backgroundStars)
        {
            AddObjToRender(other);//tell RenderManager to Render the object.
            other.Render(); //Call custom render, if it has one. (Syncs text and rect draw calls)
        }
        //Console.WriteLine("Players render!");

        foreach (GameObject go in gl.GetPlayers())
        {
            if (go.uid == localPlayer.uid) continue; //skip local player.
            AddObjToRender(go); //tell RenderManager to Render the object.
            go.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        }   
        //Console.WriteLine("ACtive Objects render!");

        foreach (GameObject go in gl.GetActiveObjects())
        {
            AddObjToRender(go); //tell RenderManager to Render the object.
            go.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        }
                //Console.WriteLine("Local Player render!");

        AddObjToRender(localPlayer);//tell RenderManager to Render the object.
        localPlayer.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
                //Console.WriteLine("Base render!");

        //Do whatever the RenderManager wants to do by itself. probably the official render calls.
        await base.Render(); 

    }

    
    //Verify that we did what the host did. If not, we need to correct our gamestate to match the host.







}
