using System;
using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager : RenderManager
{

    //Intiialzie the rendre manager
    //List of different game objects.
    List<GameObject> player = new List<GameObject>();
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    //private IJSRuntime js;
    DateTime counter = DateTime.Now;
    int frames = 0;
    int fps = 0;
    
    Text t;
    public GameManager(IJSRuntime JSRuntime) : base(JSRuntime)
    {

        GameManager reference = this;
         player.Add(new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0)));
        //Intialize the render manager.
        //cache images for batch drawing to JS.
        assetCache = new GameAsset[AssetManager._assets.Count];
        imageCache = new ElementReference[AssetManager._assets.Count];
        KeyValuePair<string, GameAsset>[] assets = AssetManager._assets.ToArray();
        for (int i = 0; i < assetCache.Length; i++){
            assetCache[i] = assets[i].Value;
            imageCache[i] = assets[i].Value.Image;
        }
        Transform tTransform = new Transform(50,25,100,50);
        t = new Text("FPS: " + fps, ref tTransform);
        t.textAlpha = 150;

        this.GenerateStars();

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
    public bool[] GetBoundCollided(Player obj)
    {
        bool[] collided = new bool[4]; //top, left, bottom, right
        float[] bounds = GetWorldBounds();
        collided[0] = obj.transform.position.Y < bounds[1];
        collided[1] = obj.transform.position.X < bounds[0];
        collided[2] = obj.transform.position.Y + obj.transform.size.Y > bounds[3];
        collided[3] = obj.transform.position.X + obj.transform.size.X > bounds[2];

        return collided;
    }

    public void UpdatePlayer(InputWrapper e)
    {
        ((Player)player[0]).cInput = e;

    }

    public override void Update()
    {

        base.Update();
        //Console.WriteLine("Objs in scene: " + activeObjects.Count + backgroundStars.Count() + ", objs to render: " + objsToRender.Count());
        //Remove dead objects.
        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }

        //Update the player.
        player[0].Update();
        AddObjToRender(player[0]);
        //Update stars.
        foreach (GameObject other in backgroundStars)
        {
            other.Update();
            AddObjToRender(other);
        }
        //Update active objects. Check for collision withj stars.
        foreach (GameObject go in activeObjects)
        {
            go.Update();
            AddObjToRender(go);
            //if obj is in the bounds of the canvas, we can render.

            foreach (GameObject other in backgroundStars)
            {

                if (go.CollideWith(other))
                {
                    backgroundStars.Remove(other);
                    break;
                }
            }
        }

        Render();
        
    }
    public void AddNewGameObject(GameObject o)
    {
        this.activeObjects.Add(o);
    }
    public void RemoveGameObject(GameObject o)
    {
        objsToRemove.Add(o);
    }
}
