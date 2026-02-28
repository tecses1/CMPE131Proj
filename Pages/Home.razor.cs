

namespace CMPE131Proj.Pages;
using Blazorex;
public partial class Home
{
    private CanvasManager? _canvasManager;

    private IRenderContext? _context;

    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 768;

    public InputWrapper inputWrapper;
    public static GameMain main;


    
    //This is called on first frame All intializing logic goes here..
    //For simplity's sake, I've added a "GameMain" that will be called
    //from this file, to make it more familiar.
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        //Initialize my stuff.
        inputWrapper = new InputWrapper();
        //Settings.Load();
        main = new GameMain();

        

        //create the canvas and events.
        _canvasManager?.CreateCanvas(
            "keyrain",
            new CanvasCreationOptions
            {
                Hidden = false,
                Width = Settings.CanvasWidth,
                Height = Settings.CanvasHeight,
                Alpha = Settings.hasAlpha,
                Desynchronized = Settings.isDesyncronized, // Better performance for animations
                WillReadFrequently = Settings.willReadFrequently,
                OnCanvasReady = OnCanvasReady,
                OnFrameReady = OnFrameReady,
                OnKeyUp = OnKeyUp,
                OnKeyDown = OnKeyDown,
                OnMouseMove = OnMouseMove,
                OnMouseDown = OnMouseDown,
                OnMouseUp = OnMouseUp

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
        main.UpdatePlayerInput(inputWrapper);
        //Render the stuff, provide the canvas. 
        main.Render(_context);
        inputWrapper.Clear();
        //_context.DrawImage(_myImage, 50, 50, 200, 150);
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
    private void OnMouseMove(Blazorex.MouseMoveEvent e)
    {
        if (_context is null)
            return;

        // store the coords in our InputWrapper (MouseMoveEvent.OffsetX/Y are double)
        inputWrapper.loadMouseMove(e.OffsetX, e.OffsetY);
    }

    private void OnMouseDown(Blazorex.MouseClickEvent e)
    {
        if (_context is null)
            return;

        // Most canvas mouse events let you know which button; if you want to branch
        // by button you can inspect 'e' here. For now assume left button press:
        inputWrapper.loadMouseDown(true);
    }
    private void OnMouseUp(Blazorex.MouseClickEvent e)
    {
        if (_context is null) return;

        inputWrapper.loadMouseUp(true);
    }

}
