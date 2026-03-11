

namespace ClientSideWASM.Pages;

using System.Numerics;
using Blazorex;
using Shared;
public partial class Game
{
    private CanvasManager? _canvasManager;

    private IRenderContext? _context;

    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 768;

    public ClientInputWrapper inputWrapper;
    public GameManager main;

    
    private float _accumulator = 0f;
    private float _lastTime = 0f;
    private const float _fixedDeltaTime = 1f / 60f; // 0.01666... seconds

    public float lastTime;
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


public async void OnFrameReady(float timestamp)
{
    float currentTime = timestamp / 1000f;
    if (_lastTime == 0) _lastTime = currentTime;

    float elapsed = currentTime - _lastTime;
    _lastTime = currentTime;

    // 1. Fill the bucket (cap it to 0.1s to prevent "Spiral of Death" lag)
    _accumulator += Math.Min(elapsed, 0.1f);

    // 2. The Fixed Update Loop
    // This calls your physics exactly 60 times per "simulated" second
    while (_accumulator >= _fixedDeltaTime)
    {
        await FixedUpdate();
        _accumulator -= _fixedDeltaTime;
    }

    // 3. Render whenever the browser is ready
    
     await main.Render((timestamp - lastTime));
     lastTime = timestamp;
}

    private async Task FixedUpdate()
    {
        // All physics logic goes here!
        main.UpdateInput(inputWrapper);
        // Clear input only after it has been processed by the physics
        inputWrapper.Clear();

        await main.Update();
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
