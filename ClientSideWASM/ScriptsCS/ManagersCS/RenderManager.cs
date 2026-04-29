using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Numerics;
using Blazorex;
using ClientSideWASM.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared;
using System.Drawing;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class RenderManager
{
    //World size.
    
    public Rect cameraViewRect =  new Rect(0, 0, Settings.CanvasWidth, Settings.CanvasHeight);
    //World offset, for rendering.
    public float targetWorldOffsetX = 0; //for interpolation
    public float targetWorldOffsetY = 0; //for interpolation.

    public float worldOffsetX = 0; //the current offset
    public float worldOffsetY = 0; //the current offset

    protected float prevWorldOffsetX = 0; //for interpolation
    protected float prevWorldOffsetY = 0; // for interpolation

    //pass by reference settigns object so all objects use the same one.
    //Objs to render
    public List<List<GameObject>> groupsToRender = new();
    public List<GameObject> objsToRender = new();
    public List<DrawText> DrawTextsToRender = new List<DrawText>();
    public List<DrawRect> DrawRectsToRender = new List<DrawRect>();

    //JS crap for batch drawing
    private GameAsset[] assetCache;
    private ElementReference[] imageCache;

    protected IJSInProcessRuntime js;
    private CanvasBase mainCanvas;

    DrawText t;
    public float cameraSmoothing = 0.8f; 

    int[] megaBuffer = new int[16384];
    private Dictionary<string, int> _assetStartingIndex = new();

    //interpolation settings
    protected float _timeSinceLastLoad = 0f; 
    protected double _lastTransformTime = 0; // Arrival time of the state currently in 'transform'
    protected double _nextTransformTime = 0; // Arrival time of the state we are moving TOWARD
    protected float _currentInterpolationDuration = GameConstants.updateRate; // Fallback   
    protected  float InterpolationDelay = GameConstants.updateRate * 2; // Buffer 2 updates.
    //debug stuff.
     Stopwatch fpsTimer = Stopwatch.StartNew();
    Stopwatch tickTimer = Stopwatch.StartNew();
    int frames = 0;
    int fps = 0;
    protected  int skipped = 0;
    int tick = 0;
    int ticks = 0;
    protected int updateTime = 0;
    protected int renderTime = 0;

    protected int stateSize;
    

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


        Transform tTransform = new Transform(200,25,400,100);
        t = new DrawText("FPS: " + fps, tTransform);
        t.textAlpha = 100;
        t.worldSpace = false;

        RegisterDrawTextToRender(t);

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
            this.worldOffsetX = t.rect.X  - Settings.CanvasWidth / 2;
        }
        if (!constrainY)
        {
            this.worldOffsetY = t.rect.Y - Settings.CanvasHeight / 2;
        }
    }

    public void LerpCamera()
    {
       // Console.WriteLine("previous offset: " + prevWorldOffsetX + "," + prevWorldOffsetY + " | target offset: " + targetWorldOffsetX + "," + targetWorldOffsetY);  
        this.worldOffsetX = Lerp(this.prevWorldOffsetX, this.targetWorldOffsetX, this.GetInterpolationFactor() * cameraSmoothing);
        this.worldOffsetY = Lerp(this.prevWorldOffsetY, this.targetWorldOffsetY, this.GetInterpolationFactor() * cameraSmoothing);
        //Console.WriteLine("Camera Offset: " + worldOffsetX + "," + worldOffsetY + " | Interpolation Factor: " + GetInterpolationFactor());
    }
    public void SetCameraTarget(Vector2 target, bool constrainX = false, bool constrainY = false    )
    {
        if (!constrainX)
        {
            this.prevWorldOffsetX = targetWorldOffsetX;
            this.targetWorldOffsetX = target.X - Settings.CanvasWidth / 2;
        }
        if (!constrainY)
        {
            //our current offset is the previous for interpolatio nto not jump.
            this.prevWorldOffsetY = targetWorldOffsetY;
            //create a dir vector for where we wanna be
            this.targetWorldOffsetY = target.Y - Settings.CanvasHeight / 2;
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

    public void RegisterDrawTextToRender(DrawText text)
    {
        this.DrawTextsToRender.Add(text);
    }
    public void RegisterRectToRender(DrawRect rect)
    {
        
        this.DrawRectsToRender.Add(rect);
    }
    public void UnregisterDrawText(DrawText text)
    {
        this.DrawTextsToRender.Remove(text);
    }
    public void UnregisterDrawRect(DrawRect rect)
    {
        
        this.DrawRectsToRender.Remove(rect);
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


    public float GetInterpolationFactor() 
    {
        // No more constants! We use the measured gap between the packets.
        // We still add a tiny bit of "slack" (e.g., 0.5ms) just in case of float rounding.
        if (_currentInterpolationDuration == 0) {
            //Console.WriteLine("error: divide by 0 prevention, _cUD = " + _currentInterpolationDuration);
            return 0f; // Avoid division by zero, snap to target. //
        }
        float raw = _timeSinceLastLoad / _currentInterpolationDuration ;
        float clamped = Math.Clamp(raw, 0f, 1.0f); // prevent divide by 0 situation.
        return clamped;
    }
    public void RenderAll()
    {
        if (mainCanvas == null) return;

        int cursor = 0;


        // 1. Pack Sprites in obj to render (Type 0)
        //pack groups by reference.
        Transform testTransform = new Transform(0,0,0,0); // use the same one so GC doesn't die.
        foreach (List<GameObject> group in groupsToRender) {
            foreach (var obj in group) {               
                    //var pos = obj.transform.position;
                    //var size = obj.transform.size;w
                // "Loose" Culling: Check if object is roughly in view
                // If it's outside these bounds, we don't even put it in the int[]



                if (obj.disableRender) continue; // Skip if object has rendering disabled (e.g., for invisible hitboxes or optimization)

                if (obj.previousTransform == null)  continue; //skip if we don't have interpolation capability.
                //why? So ojects don't "sit" for a while on spawn
                //also, it saves a little render time thats added to the time it takes to make the objects.
                //so it reduces jitter. :) 


                // Interpolation: Calculate the interpolated position based on previous and current transform
                float interpolationOffsetX = obj.transform.rect.X;
                float interpolationOffsetY = obj.transform.rect.Y;
                float interpolationRotation = obj.transform.RotationRadians();

                float prevRelX = obj.previousTransform.rect.X;
                float prevRelY = obj.previousTransform.rect.Y;
                
                float currRelX = obj.transform.rect.X;
                float currRelY = obj.transform.rect.Y ;

                // 2. Interpolate between the RELATIVE points
                float t = GetInterpolationFactor();
                interpolationOffsetX = prevRelX + (currRelX - prevRelX) * t;
                interpolationOffsetY = prevRelY + (currRelY - prevRelY) * t;

                //Set the test tsf to elimate pop in 
                testTransform.rect.X = interpolationOffsetX;
                testTransform.rect.Y = interpolationOffsetY;

                testTransform.rect.Width = obj.transform.rect.Width;
                testTransform.rect.Height = obj.transform.rect.Height;

                if (!testTransform.rect.IntersectsWith(GetCanvasBounds())) {
                    continue; // Skip rendering this object if interplation isn't viewable
                }
                //interpolationOffsetX = obj.previousTransform.position.X + (obj.transform.position.X - obj.previousTransform.position.X) * GetInterpolationFactor();
                //interpolationOffsetY = obj.previousTransform.position.Y + (obj.transform.position.Y - obj.previousTransform.position.Y) * GetInterpolationFactor();
                float deltaRotation = (obj.transform.rotation - obj.previousTransform.rotation + 540) % 360 - 180;
                float smoothedRotation = obj.previousTransform.rotation + (deltaRotation * GetInterpolationFactor());

                // Apply to the actual Render transform
                interpolationRotation = smoothedRotation * (float)(Math.PI / 180.0);

                //if (obj.GetType() == typeof(Projectile)) {
                //    // For the local player, we want to use the actual position for rendering, not the interpolated one.
                //    Console.WriteLine("Target: " + obj.transform.position.X + "," + obj.transform.position.Y + " | Interpolated: " + interpolationOffsetX + "," + interpolationOffsetY + " | Interpolation Factor: " + GetInterpolationFactor() + " Prev: " + obj.previousTransform?.position.X + "," + obj.previousTransform?.position.Y);
                //}
                //culling problem, we need the interpolation position of the object...
                int idx = obj.spriteOverrideIndex;
                if (idx == -1) {
                    idx = getCacheIndex(obj);
                }

                if (idx == -1) idx = 0; // Fallback to "MissingImage" if asset not found
                megaBuffer[cursor++] = 0;
                megaBuffer[cursor++] = (int)((interpolationOffsetX - worldOffsetX) * 100);
                megaBuffer[cursor++] = (int)((interpolationOffsetY - worldOffsetY) * 100);
                megaBuffer[cursor++] = (int)(obj.transform.rect.Width * 100);
                megaBuffer[cursor++] = (int)(obj.transform.rect.Height * 100);
                megaBuffer[cursor++] = (int)(interpolationRotation * 100);
                megaBuffer[cursor++] = idx;
            }
        }
        // 2. Pack Rects AND Text-Backgrounds (Type 1)
        // We treat them identically in the buffer to simplify JS

        void PackRect(DrawRect r) {
            megaBuffer[cursor++] = 1;
            float x = r.worldSpace ? (r.transform.rect.X - worldOffsetX) : r.transform.rect.X;
            float y = r.worldSpace ? (r.transform.rect.Y - worldOffsetY) : r.transform.rect.Y;
            megaBuffer[cursor++] = (int)(x * 100);
            megaBuffer[cursor++] = (int)(y * 100);
            megaBuffer[cursor++] = (int)(r.transform.rect.Width * 100);
            megaBuffer[cursor++] = (int)(r.transform.rect.Height * 100);
            megaBuffer[cursor++] = ColorToAlphaInt(r.fillColor, r.fillAlpha); 
            megaBuffer[cursor++] = ColorToAlphaInt(r.borderColor, r.borderAlpha);
            megaBuffer[cursor++] = (int)(r.borderWidth * 100);
        }
        foreach (var r in DrawRectsToRender) {
            
            if ((r.InteresectsWith(GetCanvasBounds()) || r.worldSpace == false) && !r.disableRender) PackRect(r);
        }

        foreach (var t in DrawTextsToRender) {
            if ((t.InteresectsWith(GetCanvasBounds()) || t.worldSpace == false) && !t.disableRender) PackRect(t);
        } // Text inherits from Rect!

        var textList = new List<object>();

        foreach (var t in DrawTextsToRender)
        {
            // SKIP: if explicitly disabled OR if it's world-space and off-screen
            if (t.disableRender) continue;
            if (!(t.InteresectsWith(GetCanvasBounds()) || t.worldSpace == false)) continue;

            // Only process the math for items we are actually going to show
            textList.Add(new {
                text = t.text,
                x = (t.worldSpace ? (t.transform.rect.X - worldOffsetX) : t.transform.rect.X) + t.offsetX,
                y = (t.worldSpace ? (t.transform.rect.Y - worldOffsetY) : t.transform.rect.Y) + t.offsetY,
                w = t.transform.rect.Width,
                h = t.transform.rect.Height,
                tCol = t.fontColor,
                tAlp = t.textAlpha / 255f,
                fnt = t.font,
                fontSize = t.fontSize,
                fillToSize = t.fillToRect
            });
        }

        // Convert to array for the JS Interop call
        var textLabels = textList.ToArray();


        var activeBuffer = new ArraySegment<int>(megaBuffer, 0, cursor);
        js.InvokeVoid("combinedRender", mainCanvas.Id, Settings.CanvasBackground, activeBuffer, textLabels);

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
    public Rect GetCanvasBounds()
    {
        cameraViewRect.X = (Settings.CanvasWidth / 2) + worldOffsetX;
        cameraViewRect.Y = (Settings.CanvasHeight / 2) + worldOffsetY;
        return cameraViewRect;
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

    public void UnregisterObjectToRender(GameObject go)
    {
        
    }
    //Render call. To update a GameObject, add it to a List<GameObject> and pass it with 'await RenderGroup(List<GameObject> objectList)'. This will batch render all objects in the list with one call to JS, which is much faster then individual calls for each object.
    public virtual void Render(float deltaTime)
    {

        frames++;
        AssetManager.fps = fps;

        //add text to render pipeline.
        //t.Draw((GameManager)this);
        this.LerpCamera();
        this.RenderAll();
        


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
            double percent = Math.Round(((double)stateSize * 100) / 65536.0,1);
            t.text = "FPS: " + fps + " | Ticks: " + ticks +" | UT: " + updateTime + "ms | RT: " + renderTime + "ms" + "| SZ: " + this.stateSize + "b(" + percent +"%)";
            frames = 0;

        }


            

    }
}
