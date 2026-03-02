using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class RenderManager
{
    //World size.
    
    public readonly int worldSizeX = 2000;
    public readonly int worldSizeY = 2000;
    //World offset, for rendering.
    public float worldOffsetX = 0;
    public float worldOffsetY = 0;

    //pass by reference settigns object so all objects use the same one.
    //Objs to render
    public List<GameObject> objsToRender = new List<GameObject>();
    public List<Text> textsToRender = new List<Text>();
    public List<Rect> rectsToRender = new List<Rect>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    protected IJSRuntime js;
    private CanvasBase mainCanvas;
    DateTime counter = DateTime.Now;
    int frames = 0;
    int fps = 0;
    
    Text t;
    public RenderManager(IJSRuntime js)
     {
        this.js = js;
        //this.gm = gm;


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

    }

    public void CenterCameraOn(Transform t, bool constrainX = false, bool constrainY = false)
    {   
        if (!constrainX)
        {
            this.worldOffsetX = t.position.X  - Settings.CanvasWidth / 2;
        }
        if (!constrainY)
        {
            this.worldOffsetY = t.position.Y - Settings.CanvasHeight / 2;
        }
    }

    public void MoveCamera(Vector2 direction)
    {
        this.worldOffsetX += direction.X;
        this.worldOffsetY += direction.Y;
    }

    public void CanvasToWorldPosition(Vector2 v)
    {
        //return new Vector2()
    }
    public void SetMainCanvas(CanvasBase c)
    {
        this.mainCanvas = c;
    }

    public void AddTextToRender(Text text)
    {
        this.textsToRender.Add(text);
    }
    public void AddRectToRender(Rect rect)
    {
        
        this.rectsToRender.Add(rect);
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
    public async Task RenderRects()
    {
        var rectToRender = rectsToRender.Select(r => new {
            fillColor = r.fillColor,
            alpha = ((float)r.alpha)/255f,
            sizeX = r.transform.size.X, // The box width
            sizeY = r.transform.size.Y, // The box height
            x = r.transform.position.X,
            y = r.transform.position.Y,
            borderWidth = r.borderWidth,
            borderColor = r.borderColor
        }).ToArray();
        await js.InvokeVoidAsync("drawRectBatch", mainCanvas.Id, rectToRender);
        
        rectsToRender.Clear();
    }
    public async Task RenderTexts()
    {
        var textToRender = textsToRender.Select(t => new {
            text = t.text,
            fontFamily = "Arial", // Pass just the name, JS handles the size
            fillColor = t.fillColor,
            fontColor = t.fontColor,
            textAlpha = ((float)t.textAlpha)/255f,
            rectAlpha = ((float)t.rectAlpha)/255f,
            sizeX = t.transform.size.X, // The box width
            sizeY = t.transform.size.Y, // The box height
            x = t.transform.position.X,
            y = t.transform.position.Y,
            offX = t.offsetX,
            offY = t.offsetY,
            borderWidth = t.borderWidth,
            borderColor = t.borderColor
        }).ToArray();
        await js.InvokeVoidAsync("drawTextBatch", mainCanvas.Id, textToRender);
        
        textsToRender.Clear();
    }
    public async Task RenderText(Text t)
    {
        var textToRender = new[] { new {
                text = t.text,
                fontFamily = "Arial",
                fillColor = t.fillColor,
                fontColor = t.fontColor,
                textAlpha = ((float)t.textAlpha) / 255f,
                rectAlpha = ((float)t.rectAlpha) / 255f,
                sizeX = t.transform.size.X,
                sizeY = t.transform.size.Y,
                x = t.transform.position.X,
                y = t.transform.position.Y,
                offX = t.offsetX,
                offY = t.offsetY,
                borderWidth = t.borderWidth,
                borderColor = t.borderColor
            }};

        await js.InvokeVoidAsync("drawTextBatch", mainCanvas.Id, textToRender);
    }
    public async Task RenderGroup() 
    {

        float[] _renderBuffer = new float[objsToRender.Count * 6];
        
        //Create array of all possible images loaded in the game.
        for (int i = 0; i < objsToRender.Count; i++)
        {
            GameObject obj = objsToRender[i];//the gameobject.
            int cacheIndex = getCacheIndex(obj.GetType().Name);
            int offset = i * 6;
            if (cacheIndex == -1)
            {
                //Console.WriteLine("Image not found for " + obj.GetType().Name + ": making error icon.");
                await RenderTexts();
                continue;
            }


            _renderBuffer[offset] = obj.transform.position.X - worldOffsetX;
            _renderBuffer[offset+1] = obj.transform.position.Y - worldOffsetY;
            _renderBuffer[offset+2] = obj.transform.size.X;
            _renderBuffer[offset+3] = obj.transform.size.Y;
            _renderBuffer[offset+4] = obj.transform.rotation * (float)Math.PI / 180f;
            _renderBuffer[offset+5] = getCacheIndex(obj.GetType().Name); // Tell JS which image to use



        }

        // Send all images and all data in one go
        await js.InvokeVoidAsync("batchDrawMulti",mainCanvas.Id , imageCache, _renderBuffer);
        objsToRender.Clear();
        
        
    }
    public Vector2 CameraToWorldPos(Vector2 v)
    {
        return new Vector2(v.X + worldOffsetX, v.Y + worldOffsetY);
    }
    public Vector2 CameraToWorldPos(float x, float y)
    {
        return new Vector2(x + worldOffsetX, y + worldOffsetY);
    }
    //Returns Canvas bounds. USeful for seeign if an object moves outside of bounds, within reason.
    public float[] GetCanvasBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0 + worldOffsetX;
        bounds[1] = 0 + worldOffsetY;
        bounds[2] = Settings.CanvasWidth + worldOffsetX;
        bounds[3] = Settings.CanvasHeight + worldOffsetY;

        return bounds;
    }

    public float[] GetCanvasCenter()
    {
        float[] center = new float[2];
        center[0] = Settings.CanvasWidth/2;
        center[1] = Settings.CanvasHeight/2;

        return center;
    }

    public float[] GetCanvasCenterWorld()
    {
        float[] center = new float[2];
        center[0] = Settings.CanvasWidth/2 + worldOffsetX;
        center[1] = Settings.CanvasHeight/2 + worldOffsetY;

        return center;
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

    public Transform GetRenderTrasnform(Transform t)
    {
        Transform newT = new Transform(t.position.X - worldOffsetX, t.position.Y - worldOffsetY,(int)t.size.X, (int)t.size.Y);
        newT.rotation = t.rotation;
        return newT;
    }

    //Render call. To update a GameObject, add it to a List<GameObject> and pass it with 'await RenderGroup(List<GameObject> objectList)'. This will batch render all objects in the list with one call to JS, which is much faster then individual calls for each object.
    public async virtual void Render()
    {
        //Clear the background in JS. Faster, and synced.
        await js.InvokeVoidAsync("clearBackground",mainCanvas.Id ,Settings.CanvasBackground );

        //Render the "objsToRender" group. This group is modified to only include on screen GameObjects.
        await RenderGroup();
        await RenderTexts();
        await RenderRects();
    }
    public void AddObjToRender(GameObject go)
    {
        //only render if in canvas camera view.
        if (go.CollideWith(GetCanvasBounds()))
        {
            this.objsToRender.Add(go);
        }
        
    }
    public virtual void Update()
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
    }
}
