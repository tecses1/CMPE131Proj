using System;
using System.Diagnostics;
using System.Dynamic;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager
{
    //pass by reference settigns object so all objects use the same one.
    
    int worldSizeX = 2000;
    int worldSizeY = 2000;

    int worldPosX = 0;
    int worldPosY = 0;

    //List of different game objects.
    List<GameObject> player = new List<GameObject>();
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    private IJSRuntime js;
    private CanvasBase mainCanvas;
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
    this.GeneateStars();

    }
    public void SetMainCanvas(CanvasBase c)
    {
        this.mainCanvas = c;
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

    public async Task RenderGroup(List<GameObject> objectList) 
    {
        float[] _renderBuffer = new float[objectList.Count * 6];
        
        //Create array of all possible images loaded in the game.
        for (int i = 0; i < objectList.Count; i++)
        {
            GameObject obj = objectList[i];//the gameobject.
            
            // Get the index of this object's specific image
            

            int offset = i * 6;
            _renderBuffer[offset] = obj.transform.position.X;
            _renderBuffer[offset+1] = obj.transform.position.Y;
            _renderBuffer[offset+2] = obj.transform.size.X;
            _renderBuffer[offset+3] = obj.transform.size.Y;
            _renderBuffer[offset+4] = obj.transform.rotation * (float)Math.PI / 180f;
            _renderBuffer[offset+5] = getCacheIndex(obj.GetType().Name); // Tell JS which image to use

        }

        // Send all images and all data in one go
        await js.InvokeVoidAsync("batchDrawMulti",mainCanvas.Id , imageCache, _renderBuffer);
        _renderBuffer = null;
        
    }
    void GeneateStars()
    {
        //iterate through canvas coordinates.
        Random r = new Random();
        for (int i = 0; i < Settings.CanvasWidth; i++)
        {
            for (int j = 0; j < Settings.CanvasHeight; j++)
            {   
                double chance = r.NextDouble();
                double sizeModifier = Math.Sqrt(Settings.CanvasHeight * Settings.CanvasWidth);
                if (chance < Settings.Sparseness / (sizeModifier))
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
    public float[] GetBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0;
        bounds[1] = 0;
        bounds[2] = Settings.CanvasWidth;
        bounds[3] = Settings.CanvasHeight;

        return bounds;

    }
    //Add render function to call from main thread.
    public async void Render(IRenderContext ctx)
    {
        await js.InvokeVoidAsync("clearBackground",mainCanvas.Id ,Settings.CanvasBackground );
        await RenderGroup(backgroundStars);
        await RenderGroup(activeObjects);
        await RenderGroup(player);

        player[0].Render(ctx);

    }
    public void UpdatePlayer(InputWrapper e)
    {
        ((Player)player[0]).Update(e);
    }
    public void Update()
    {

        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }
        foreach (GameObject go in activeObjects)
        {
            go.Update();
            
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
