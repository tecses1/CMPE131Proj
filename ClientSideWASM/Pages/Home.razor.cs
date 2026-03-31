namespace ClientSideWASM.Pages;

using System.ComponentModel.DataAnnotations.Schema;
using Blazorex;
using Shared;

public partial class Home
{


    private CanvasManager? _canvasManager;

    private IRenderContext? _context;

    public ClientInputWrapper inputWrapper;

    public HomeManager home;

    protected override void OnInitialized()
    {
        if (!nm.client.isConnected()){
            Nav.NavigateTo("/");
        }
        nm.client.Send("{SetName}",null,Settings.name);
        nm.client.Send("{SetPage}",null,this.GetType().Name);
    }

    public async Task RequestNewLobby()
    {
        var response = await nm.client.SendWithResponse("{NewLobby}",null,"lobbyName");

        if (response != null)
        {
            // Populate the UI with the args returned from the server
            var lobby = response.Args.ToList()[0];
            nm.isHost = true;
            nm.myLobby = lobby;
            Console.WriteLine(lobby);
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
        }
    }
    public async Task RequestJoinLobby()
    {
        var response = await nm.client.SendWithResponse("{JoinLobby}",null,"lobbyName");

        if (response != null)
        {
            var args = response.Args.ToList();
            var resp = args[0];//what did the server say
            if (resp == "{Success}")
            {
                this.nm.myLobby = "lobbyName";
            }
            else
            {
                //tell we failed to join
            }
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
        }
    
        
    }
    //Called when button is pr essed, see above.
    /*
    private async Task SaveToStorage()
    {
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            // Saves the current value of 'userInput' to local storage
            
            Settings.name = userInput;
            await nm.client.Send("{SetName}",null,Settings.name);
            await Save();
        }

    }*/
    //Reference method. Called from OnIntializedAsync().
    async Task Save(){
        await LocalStorage.SetItemAsStringAsync("settingsSaveKey",Settings.Save());

    }


    //canvas stuff
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;
        //create the canvas and events.
        _canvasManager?.CreateCanvas(
            "MainGame",
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
        home = new HomeManager(JS);
        inputWrapper = new ClientInputWrapper();
    }
    private void OnCanvasReady(CanvasBase canvas)
    {

        home.SetMainCanvas(canvas);
        _context = canvas.RenderContext;

        
    }

    public void OnFrameReady(float timestamp)
    {
        home.ApplyInput(inputWrapper);
        home.Update();
        home.Render(timestamp);

        
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