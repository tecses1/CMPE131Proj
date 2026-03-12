using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class RenderManager
{
    //World size.
    

    //World offset, for rendering.
    public float worldOffsetX = 0;
    public float worldOffsetY = 0;

    protected float prevWorldOffsetX = 0;
    protected float prevWorldOffsetY = 0;

    //pass by reference settigns object so all objects use the same one.
    //Objs to render
    public List<List<GameObject>> groupsToRender = new();
    public List<GameObject> objsToRender = new();
    public List<Text> textsToRender = new List<Text>();
    public List<Rect> rectsToRender = new List<Rect>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    protected IJSInProcessRuntime js;
    private CanvasBase mainCanvas;
    int frames = 0;
    int fps = 0;
    int tick = 0;
    int ticks = 0;
    Stopwatch fpsTimer = Stopwatch.StartNew();
    Stopwatch tickTimer = Stopwatch.StartNew();
    Text t;
    public float cameraSmoothing = 0.25f; 

    int[] megaBuffer = new int[16384];
    private Dictionary<string, int> _assetStartingIndex = new();

    //interpolation settings
    private Stopwatch _tickTimer = Stopwatch.StartNew();
    private const float TICK_MS = 33.33f;

    public RenderManager(IJSRuntime js)
     {
        this.js = (IJSInProcessRuntime)js;
        int currentGlobalIndex = 0;
            foreach (var asset in AssetManager._assets)
            {
                // Map the asset name (e.g., "Projectile") to its position in the flat JS array
                _assetStartingIndex[asset.Key] = currentGlobalIndex;
                
                // Increment by the number of frames this asset contains
                currentGlobalIndex += asset.Value.Frames.Length;
                
                // (Optional) Add your Console.WriteLine debugs here
            }

        //cache images for batch drawing to JS.
        assetCache = new GameAsset[AssetManager._assets.Count];
        List<ElementReference> imageCacheList = new List<ElementReference>();
        KeyValuePair<string, GameAsset>[] assets = AssetManager._assets.ToArray();
        //Cache the images. 
        for (int i = 0; i < assetCache.Length; i++){
            assetCache[i] = assets[i].Value;
            imageCacheList.AddRange(assets[i].Value.Frames);
            Console.WriteLine("[RENDERMANAGER] Caching " + assets[i].Value.Frames.Length + " frames (Indexes: " + (imageCacheList.Count - assets[i].Value.Frames.Length) + " to " + (imageCacheList.Count - 1) + ") for asset: " + assets[i].Key);
        }
        Console.WriteLine("[RENDERMANAGER] Cached " + imageCacheList.Count + " images for batch rendering.");
        this.imageCache = imageCacheList.ToArray<ElementReference>();


        Transform tTransform = new Transform(50,25,100,100);
        t = new Text("FPS: " + fps, tTransform);
        t.textAlpha = 150;
        t.worldSpace = false;
        InitializeJSCache();

        groupsToRender.Add(objsToRender);
    }

    public void InitializeJSCache()
    {
        js.InvokeVoid("initializeCache", imageCache);
    }

    public Vector2 WorldToCameraPos(Vector2 v)
    {
        return new Vector2(v.X + worldOffsetX, v.Y + worldOffsetY);
    }
    //Usually called from update funciton.
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

    public void CenterCameraOnLerp(Transform t, float deltaTime, bool constrainX = false, bool constrainY = false)
    {   

        //X and Y need to be constrained independently to properly work with world bounds (otherwise camera locks on worldbounds and can't move in one axis even if the other is free).

         if (!constrainX)
        {
             float targetX = t.position.X - Settings.CanvasWidth / 2;
            // Move a fraction of the way to the target
             this.worldOffsetX = Lerp(this.worldOffsetX, targetX, cameraSmoothing);
         }
        
        if (!constrainY)
         {
             float targetY = t.position.Y - Settings.CanvasHeight / 2;
             this.worldOffsetY = Lerp(this.worldOffsetY, targetY, cameraSmoothing);
         }
    }

    // Simple Lerp helper if your framework doesn't have one
    private float Lerp(float start, float end, float amount)
    {
        return start + (end - start) * amount;
    }

    public void MoveCamera(Vector2 direction)
    {
        this.worldOffsetX += direction.X;
        this.worldOffsetY += direction.Y;
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


    int getCacheIndex(GameObject o)
    {
    // 1. Get the name of the class (Projectile, Asteroid, etc.)
    string typeName = o.GetType().Name;

    // 2. Look up the starting index we calculated at boot
    if (_assetStartingIndex.TryGetValue(typeName, out int baseIndex))
    {
        // 3. Add the current frame offset
        // Ensure you don't overshoot the frame count for this specific asset
        return baseIndex + o.currentFrame;
    }

    return -1; // Asset not found
}
    private int ColorToAlphaInt(string htmlColor, int alpha0to255)
    {
        if (string.IsNullOrEmpty(htmlColor)) return 0;

        try
        {
            // ColorTranslator.FromHtml handles BOTH "#FFFFFF" and "Red" strings
            var c = System.Drawing.ColorTranslator.FromHtml(htmlColor);
            
            // Pack: AARRGGBB 
            // We use the alpha0to255 passed from your object, 
            // ignoring any alpha baked into the HTML string.
            uint packed = ((uint)alpha0to255 << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | (uint)c.B;
            
            return (int)packed;
        }
        catch
        {
            return 0; // Transparent black fallback
        }
    }


    public void ResetInterpolationClock() {
        _tickTimer.Restart(); // Reset the 'bucket'
    }

    public float GetInterpolationFactor() {
        // This gives us a value from 0.0 to 1.0 (or higher if the next packet is late)
        float t = _tickTimer.ElapsedMilliseconds / TICK_MS;
        
        // Clamp it to 1.0 to prevent the object from "ghosting" 
        // past its current logical position while waiting for the next packet.
        return Math.Min(t, 1.0f);
    }
    public void RenderAll(float deltaTime)
    {
        if (mainCanvas == null) return;

        int cursor = 0;


        // 1. Pack Sprites in obj to render (Type 0)
        //pack groups by reference.
        foreach (List<GameObject> group in groupsToRender) {
            foreach (var obj in group) {
                    var pos = obj.transform.position;
                    var size = obj.transform.size;

                // "Loose" Culling: Check if object is roughly in view
                // If it's outside these bounds, we don't even put it in the int[]
                if (obj.InBounds(GetCanvasBounds()) == false) {
                    continue; // Skip rendering this object
                }
                // Interpolation: Calculate the interpolated position based on previous and current transform
                float interpolationOffsetX = obj.transform.position.X;
                float interpolationOffsetY = obj.transform.position.Y;
                float interpolationRotation = obj.transform.RotationRadians();
                if (obj.previousTransform != null) {
                    interpolationOffsetX = obj.previousTransform.position.X + (obj.transform.position.X - obj.previousTransform.position.X) * GetInterpolationFactor();
                    interpolationOffsetY = obj.previousTransform.position.Y + (obj.transform.position.Y - obj.previousTransform.position.Y) * GetInterpolationFactor();

                    float deltaRotation = (obj.transform.rotation - obj.previousTransform.rotation + 540) % 360 - 180; // Shortest angle difference
                    //3. Interpolate using the corrected delta
                    interpolationRotation = obj.previousTransform.rotation + deltaRotation * GetInterpolationFactor();

                    //Must be in radians.
                    interpolationRotation = interpolationRotation * (float)Math.PI / 180f;
                }


                int idx = getCacheIndex(obj);
                if (idx == -1) continue;
                megaBuffer[cursor++] = 0;
                megaBuffer[cursor++] = (int)((interpolationOffsetX - worldOffsetX) * 100);
                megaBuffer[cursor++] = (int)((interpolationOffsetY - worldOffsetY) * 100);
                megaBuffer[cursor++] = (int)(obj.transform.size.X * 100);
                megaBuffer[cursor++] = (int)(obj.transform.size.Y * 100);
                megaBuffer[cursor++] = (int)(interpolationRotation * 100);
                megaBuffer[cursor++] = idx;
            }
        }
        // 2. Pack Rects AND Text-Backgrounds (Type 1)
        // We treat them identically in the buffer to simplify JS
        void PackRect(Rect r) {
            megaBuffer[cursor++] = 1;
            float x = r.worldSpace ? (r.transform.position.X - worldOffsetX) : r.transform.position.X;
            float y = r.worldSpace ? (r.transform.position.Y - worldOffsetY) : r.transform.position.Y;
            megaBuffer[cursor++] = (int)(x * 100);
            megaBuffer[cursor++] = (int)(y * 100);
            megaBuffer[cursor++] = (int)(r.transform.size.X * 100);
            megaBuffer[cursor++] = (int)(r.transform.size.Y * 100);
            megaBuffer[cursor++] = ColorToAlphaInt(r.fillColor, r.fillAlpha); 
            megaBuffer[cursor++] = ColorToAlphaInt(r.borderColor, r.borderAlpha);
            megaBuffer[cursor++] = (int)(r.borderWidth * 100);
        }
        int debug = 0;
        foreach (var r in rectsToRender) {
            
            if (r.InBounds(GetCanvasBounds()) || r.worldSpace == false) PackRect(r);
        }

        foreach (var t in textsToRender) {
            if (t.InBounds(GetCanvasBounds()) || t.worldSpace == false) PackRect(t);
        } // Text inherits from Rect!

        // 3. Prepare ONLY the Text labels
        var textLabels = textsToRender.Where(t => (t.InBounds(GetCanvasBounds()) || t.worldSpace == false)).Select(t => new {
            text = t.text,
            x = (t.worldSpace ? (t.transform.position.X - worldOffsetX) : t.transform.position.X) + t.offsetX,
            y = (t.worldSpace ? (t.transform.position.Y - worldOffsetY) : t.transform.position.Y) + t.offsetY,
            w = t.transform.size.X, // Matches t.w in JS
            h = t.transform.size.Y, // Matches t.h in JS
            tCol = t.fontColor,
            tAlp = t.textAlpha / 255f,
            fnt = t.font,
            fontSize = t.fontSize,
            fillToSize = t.fillToRect
        }).ToArray();


        var activeBuffer = new ArraySegment<int>(megaBuffer, 0, cursor);
        js.InvokeVoid("combinedRender", mainCanvas.Id, Settings.CanvasBackground, activeBuffer, textLabels);

        //objsToRender.Clear();
        rectsToRender.Clear();
        textsToRender.Clear();
    }
    public Vector2 CameraToWorldPos(Vector2 v)
    {
                //Console.WriteLine("Offset: " + v.X + " + " +worldOffsetX);

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


    public Transform GetRenderTrasnform(Transform t)
    {
        Transform newT = new Transform(t.position.X - worldOffsetX, t.position.Y - worldOffsetY,(int)t.size.X, (int)t.size.Y);
        newT.rotation = t.rotation;
        return newT;
    }
    public void RegisterGroupToRender(List<GameObject> go)
    {
        //only render if in canvas camera view.

        this.groupsToRender.Add(go);
        
        
    }

    public void RegisterObjToRender(GameObject go)
    {
        this.objsToRender.Add(go);
    }
    //Render call. To update a GameObject, add it to a List<GameObject> and pass it with 'await RenderGroup(List<GameObject> objectList)'. This will batch render all objects in the list with one call to JS, which is much faster then individual calls for each object.
    public virtual void Render(float deltaTime)
    {
        frames++;
        //Console.WriteLine("[DEBUG] [RENDERMANAGE] Called at " + DateTime.Now.ToLongTimeString() + ":" + DateTime.Now.Millisecond);
        AssetManager.fps = fps;
        //UPDATE FPS

        //add text to render pipeline.
        t.Draw((GameManager)this);
        //Clear the background in JS. Faster, and synced.
        //js.InvokeVoid("clearBackground",mainCanvas.Id , );

        //Render the "objsToRender" group. This group is modified to only include on screen GameObjects.
        //RenderGroup();
        //RenderTexts();
        //RenderRects();

        this.RenderAll(deltaTime);

    }

    public virtual void  Update()
    {
        tick++;
        if (tickTimer.ElapsedMilliseconds >= 1000) // Every second
        {
            //Console.WriteLine("Tick: " + tick);
            tickTimer.Restart();
            ticks = tick;
            tick = 0;


            //calculate fps.
            double elapsedSeconds = fpsTimer.Elapsed.TotalMilliseconds / 1000.0; // ms bc seconds will be 0.
            fps = (int)(frames / elapsedSeconds);
            fpsTimer.Restart();
            t.text = "FPS: " + fps + "| Ticks: " + ticks;
            frames = 0;

        }


            

    }
}
