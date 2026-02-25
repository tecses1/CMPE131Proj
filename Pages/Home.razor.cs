

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
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        //Initialize my stuff.
        inputWrapper = new InputWrapper();
        main = new GameMain();


        _canvasManager?.CreateCanvas(
            "keyrain",
            new CanvasCreationOptions
            {
                Hidden = false,
                Width = CanvasWidth,
                Height = CanvasHeight,
                Alpha = false,
                Desynchronized = true, // Better performance for animations
                WillReadFrequently = false,
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
