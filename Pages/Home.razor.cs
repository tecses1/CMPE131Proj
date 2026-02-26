

namespace CMPE131Proj.Pages;
using Blazorex;
public partial class Home
{
    private CanvasManager? _canvasManager;
    private IRenderContext? _context;

    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 768;

    public InputWrapper inputWrapper;
    public GameMain main;
    public Settings settings;

    //This is called on first frame All intializing logic goes here..
    //For simplity's sake, I've added a "GameMain" that will be called
    //from this file, to make it more familiar.
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        //Initialize my stuff.
        inputWrapper = new InputWrapper();
        settings = Settings.Load();
        main = new GameMain(ref settings);

        

        //create the canvas and events.
        _canvasManager?.CreateCanvas(
            "keyrain",
            new CanvasCreationOptions
            {
                Hidden = false,
                Width = settings.CanvasWidth,
                Height = settings.CanvasHeight,
                Alpha = settings.hasAlpha,
                Desynchronized = settings.isDesyncronized, // Better performance for animations
                WillReadFrequently = settings.willReadFrequently,
                OnCanvasReady = OnCanvasReady,
                OnFrameReady = OnFrameReady,
                OnKeyUp = OnKeyUp,
                OnKeyDown = OnKeyDown,
                OnMouseMove = OnMouseMove,
                OnMouseDown = OnMouseDown
                

            }
        );
    }

    private void OnCanvasReady(CanvasBase canvas)
    {
        _context = canvas.RenderContext;
        
    }
    
    private void OnFrameReady(float timestamp)
    {
        
        if (_context is null)
            return;


        //call update function in GameMain.cs to update game state each frame. Pass input over.
        main.Update(inputWrapper);
        //Render the stuff, provide the canvas. 
        main.Render(_context);
    }

    private void OnKeyDown(KeyboardPressEvent e)
    {
        if (_context is null)
            return;
        //store the current event.
        this.inputWrapper.loadKeysDown(e);
        
    }
    private void OnKeyUp(KeyboardPressEvent e)
    {
        if (_context is null)
            return;
        //store the current event.
        this.inputWrapper.loadKeysUp(e);
        
    }
    private void OnMouseMove(MouseMoveEvent e)
    {
        if (_context is null)
            return;
        this.inputWrapper.cMouseMovementInput = e;
    }
 private void OnMouseDown(MouseClickEvent e)
    {
        if (_context is null)
            return;
        this.inputWrapper.cMouseClickInput = e;
        
    }


}
