
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




    Text isLocal;

    InputWrapper cInput;
    

    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        nm.Initialize(this);
        
        GenerateStars();
        Transform t = new Transform(Settings.CanvasWidth/2, 25, 300,50);
        isLocal = new Text("Playing Locally ", ref t);
        isLocal.worldSpace = false;
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
                    s.RegisterGameLogic(gl);
                    
                    backgroundStars.Add(s);
                }

            }
        }
        
    }


    public void UpdateInput(InputWrapper e)
    {
        this.cInput = e;
    }





    // TODO: Everything run from host perspective, so currently other players manually updated here BUT
    // player scores all go to the host, other players cant take damage, weird stuff happens when refreshing

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

    
    //Verify that we did what the host did. If not, we need to correct our gamestate to match the host.

    void Sync()
    {
        
    }
    public override async Task Update()
    {
        //Set the timestamp for this update.

        
    }





}
