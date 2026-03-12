

namespace ClientSideWASM.Pages;

using System.Numerics;
using Blazorex;
using Shared;
using System.Diagnostics;
public partial class Game
{
    private CanvasManager? _canvasManager;

    private IRenderContext? _context;

    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 768;

    public ClientInputWrapper inputWrapper;
    public GameManager main;

    
    private double tick = 0;
    private const float _fixedDeltaTime = 1000f / 30f; //60 hz

    public float lastTime = -1;
    protected override async Task OnInitializedAsync()
    {
        if (!nm.client.isConnected()){
        }
        else
        {
            await nm.client.Send("{SetName}",null,Settings.name);
            await nm.client.Send("{SetPage}",null,this.GetType().Name);
        }
        // Example: Set a default supplier if null


    }
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;




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
        //Initialize my stuff.
        main = new GameManager(JS,  nm);
        inputWrapper = new ClientInputWrapper();
    }

    private void OnCanvasReady(CanvasBase canvas)
    {

        main.SetMainCanvas(canvas);
        _context = canvas.RenderContext;

        
    }

    public void OnFrameReady(float timestamp)
    {
        if (lastTime == -1)
        {
            lastTime = timestamp;
            return;
        }
        float deltaTime = (timestamp - lastTime); //convert to ms;
        main.Render(deltaTime);
        lastTime = timestamp;
        // 2. The Fixed Update Loop
        // This calls your physics exactly 60 times per "simulated" second
        tick += deltaTime;
        if (tick > _fixedDeltaTime)
        {
            main.UpdateInput(inputWrapper);
            inputWrapper.Clear();
            
            main.Update();
            
            tick -= _fixedDeltaTime;
        }




        
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
        //Console.WriteLine("offset: " + e.OffsetX + "," +e.OffsetY);
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
