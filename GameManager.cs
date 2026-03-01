using System;
using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager
{
    //pass by reference settigns object so all objects use the same one.
    
    public readonly int worldSizeX = 2000;
    public readonly int worldSizeY = 2000;

    public int worldOffsetX = 0;
    public int worldOffsetY = 0;

    //List of different game objects.
    List<GameObject> player = new List<GameObject>();
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();
    public List<GameObject> objsToRender = new List<GameObject>();
    public List<Text> textsToRender = new List<Text>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    private IJSRuntime js;
    private CanvasBase mainCanvas;
    DateTime counter = DateTime.Now;
    int frames = 0;
    int fps = 0;
    
    Text t;
    public GameManager(IJSRuntime js)
    {
        this.js = js;

        GameManager reference = this;
         player.Add(new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0)));


        //cache images for batch drawing to JS.
        assetCache = new GameAsset[AssetManager._assets.Count];
        imageCache = new ElementReference[AssetManager._assets.Count];
        KeyValuePair<string, GameAsset>[] assets = AssetManager._assets.ToArray();
        for (int i = 0; i < assetCache.Length; i++){
            assetCache[i] = assets[i].Value;
            imageCache[i] = assets[i].Value.Image;
        }
        Transform tTransform = new Transform(10,10,0,0);
        t = new Text("FPS: " + fps, ref tTransform, 25,10);

        this.GeneateStars();

    }
    public void SetMainCanvas(CanvasBase c)
    {
        this.mainCanvas = c;
    }

    public void AddTextToRender(Text text)
    {
        this.textsToRender.Add(text);
    }
    int getCacheIndex(string name)
    {
        for (int i = 0; i < assetCache.Length; i++)
        {
            if (assetCache[i].Name == name)
            {
                return i;
            }
        }
        return -1;
    }
    //Add update function to call from main thread.
    public async Task RenderTexts()
    {

            var textToRender = textsToRender.Select(t => new {
                text = t.text,
                font = t.font,
                fontColor = t.fontColor,
                borderColor = t.borderColor,
                borderWidth = t.borderWidth,
                x = t.transform.position.X,
                y = t.transform.position.Y,
                offX = t.offsetX,
                offY = t.offsetY
            }).ToArray();
            await js.InvokeVoidAsync("drawTextBatch", mainCanvas.Id, textToRender);
        
        textsToRender.Clear();
    }
    public async Task RenderGroup(List<GameObject> objectList) 
    {
        float[] _renderBuffer = new float[objectList.Count * 6];
        
        //Create array of all possible images loaded in the game.
        for (int i = 0; i < objectList.Count; i++)
        {
            
            int offset = i * 6;
            GameObject obj = objectList[i];//the gameobject.

            _renderBuffer[offset] = obj.transform.position.X - worldOffsetX;
            _renderBuffer[offset+1] = obj.transform.position.Y + worldOffsetY;
            _renderBuffer[offset+2] = obj.transform.size.X;
            _renderBuffer[offset+3] = obj.transform.size.Y;
            _renderBuffer[offset+4] = obj.transform.rotation * (float)Math.PI / 180f;
            _renderBuffer[offset+5] = getCacheIndex(obj.GetType().Name); // Tell JS which image to use



        }

        // Send all images and all data in one go
        await js.InvokeVoidAsync("batchDrawMulti",mainCanvas.Id , imageCache, _renderBuffer);
        _renderBuffer = null;
        
    }
    //Returns offset trasnform, what we actually see relative to canvas view.
    public Transform GetRenderTransform(Transform obj)
    {
        return new Transform(obj.position.X - worldOffsetX, obj.position.Y + worldOffsetY, (int)obj.size.X, (int)obj.size.Y, obj.rotation);
    }
    void GeneateStars()
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
    //Returns Canvas bounds. USeful for seeign if an object moves outside of bounds, within reason.
    public float[] GetCanvasBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0 + worldOffsetX;
        bounds[1] = 0 - worldOffsetY;
        bounds[2] = Settings.CanvasWidth + worldOffsetX;
        bounds[3] = Settings.CanvasHeight - worldOffsetY;

        return bounds;

    }
    public float[] GetWorldBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0; //Top left corner X
        bounds[1] = 0; //Top left corner Y
        bounds[2] = worldSizeX; //Bottom right corner X
        bounds[3] = worldSizeY; //Bottom right corner Y

        return bounds;
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

    //Render call. To update a GameObject, add it to a List<GameObject> and pass it with 'await RenderGroup(List<GameObject> objectList)'. This will batch render all objects in the list with one call to JS, which is much faster then individual calls for each object.
    public async void Render(IRenderContext ctx)
    {
        //Clear the background in JS. Faster, and synced.
        await js.InvokeVoidAsync("clearBackground",mainCanvas.Id ,Settings.CanvasBackground );

        //Render the "objsToRender" group. This group is modified to only include on screen GameObjects.
        await RenderGroup(objsToRender);
        await RenderTexts();
        objsToRender.Clear();

        

    }
    public void UpdatePlayer(InputWrapper e)
    {
        ((Player)player[0]).cInput = e;

    }
    public void Update()
    {
        //UPDATE FPS
        if ( (DateTime.Now-counter).Seconds > 1)
        {
            t.text = "FPS: " + fps;
            counter = DateTime.Now;
            fps = frames;
            frames = 0;
        }
        else
        {
            frames++;
        }
        //add text to render pipeline.
        AddTextToRender(t);

        //Remove dead objects.
        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }

        //Update the player.
        player[0].Update();

        //Update background stars
        foreach (GameObject go in backgroundStars)
        {
            go.Update();
        }
        
        //Update active objects. Check for collision withj stars.
        foreach (GameObject go in activeObjects)
        {
            go.Update();
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
