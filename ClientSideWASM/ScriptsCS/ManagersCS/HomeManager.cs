using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.JSInterop;
using Shared;
using Blazored.LocalStorage;
using System.Runtime.CompilerServices;
using System.Drawing;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
namespace ClientSideWASM;

public class HomeManager : RenderManager
{
    public ClientInputWrapper userInput;
    public string requestedLobby = "";

    public string failedToJoinLobby = "";


    public DrawText editing;
    public string editingPlaceholder;
    //text stuff
    List<DrawText> collideableTexts = new List<DrawText>();
        // main screen
            DrawText title;
            Button settingsText;
            Button playText;
        //lobby screen
            DrawText lobbyTitle;
            DrawText currentLobyText;
            InputField lobbyInput;
            Button launchText;
            Button requestButton;
            Button joinButton;

            Button lobbyBack;
        //settings screen
            DrawText settingsTitle;
            InputField playerNameTextBox;
            
            Button settingsBack;

    //bg stars
    float starSpeed = 0.1f;
    List<GameObject> stars = new List<GameObject>();

    Transform mouse  = new Transform(0,0,10,10);

    //functional references
    IJSRuntime JS;
    ILocalStorageService localStorage;
    NetworkManager nm;
    DateTime charTicker;
    public int cScreen = 0; //0 is main menu, 1 is lobby screen, 2 is settings
    NavigationManager Nav;
    public HomeManager(IJSRuntime JS, ILocalStorageService localStorage, NetworkManager nm, NavigationManager Nav) : base(JS)
    {
        this.Nav = Nav;
        this.JS = JS;
        this.localStorage = localStorage;
        this.nm = nm;

        GenerateStars();
        Transform canvasCenter = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight / 2, 0, 0);
        this.CenterCameraOn(canvasCenter);

        this.Initialize();
        
    }
    void GenerateStars()
    {
        Random r = new Random();
        // Calculate total area and desired density
        long totalArea = (long)Settings.CanvasWidth * Settings.CanvasHeight;
        
        // Instead of iterating 25 million times, we calculate how many stars we want.
        // Adjust 'DensityConstant' to get the look you want (e.g., 0.0001 for 1 star per 10k pixels)
        int starCount = (int)(totalArea * Settings.Sparseness / Math.Sqrt(totalArea) ); 
        Console.WriteLine("spawning " + starCount + " stars for bg");
        for (int i = 0; i < starCount; i++)
        {
            int x = r.Next(0, Settings.CanvasWidth);
            int y = r.Next(0, Settings.CanvasHeight);
            
            int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize, Settings.minSize, Settings.maxSize);
            
            Transform t = new Transform(x, y, size, size);
            Star s = new Star(t);
            stars.Add(s);

        }

        RegisterGroupToRender(stars);
    }



    //screen functions

    public void RegisterText(DrawText t)
    {
        collideableTexts.Add(t);
        RegisterDrawTextToRender(t);
    }
    public void Initialize()
    {
        //initialize ALLLLLLLL the things.

        //Main Title Inititialize
        Transform titleTransform = new Transform(Settings.CanvasWidth / 2, 100, 600, 70);
        title = new DrawText("GALAX.IO", titleTransform);
        RegisterText(title);   


        Transform playTransform = new Transform(Settings.CanvasWidth / 2, 250, 250, 50);
        playText = new Button("Play", playTransform);
        RegisterText(playText);   

        Transform settingsTransform = new Transform(Settings.CanvasWidth / 2, 300, 250, 50);
         settingsText = new Button("Settings", settingsTransform);
        RegisterText(settingsText);   


        //Lobby screen
  
        Transform lobbyTitleTrasnform = new Transform(Settings.CanvasWidth / 2, 100, 250, 70);
        lobbyTitle = new DrawText("Lobbies", lobbyTitleTrasnform);
        RegisterText(lobbyTitle);   

        Transform currentLobbyTransform = new Transform(Settings.CanvasWidth / 2, 200, 200, 50);
        currentLobyText = new DrawText("Current Lobby: None", currentLobbyTransform);
        RegisterText(currentLobyText);   

        Transform lobbyTextBoxTransform = new Transform(Settings.CanvasWidth / 2, 300, 400, 50);
        lobbyInput = new InputField("", lobbyTextBoxTransform);
        RegisterText(lobbyInput);   

        Transform requestButtonTransform = new Transform(Settings.CanvasWidth / 2-50, 400, 100, 50);
        requestButton = new Button("Create", requestButtonTransform);
        RegisterText(requestButton);   

        Transform joinButtonTransform = new Transform(Settings.CanvasWidth / 2 + 50, 400, 100, 50);
        joinButton = new Button("Join", joinButtonTransform);
        RegisterText(joinButton);   

        Transform launchTextTransform = new Transform(Settings.CanvasWidth / 2, 400, 250, 70);
        launchText = new Button("LAUNCH!", launchTextTransform);
        RegisterText(launchText);   

        Transform lobbyBackTransform = new Transform(Settings.CanvasWidth / 2, 500, 250, 50);
        lobbyBack = new Button("Back", lobbyBackTransform);
        RegisterText(lobbyBack);   

        //Settings screen
        Transform settingsTitleTransform = new Transform(Settings.CanvasWidth / 2, 100, 250, 70);
        settingsTitle = new DrawText("Settings", settingsTitleTransform);
        RegisterText(settingsTitle);   

        Transform settingsPlayerNameTextBoxTransform = new Transform(Settings.CanvasWidth / 2, 200, 250, 50);
        playerNameTextBox = new InputField(Settings.name, settingsPlayerNameTextBoxTransform);

        RegisterText(playerNameTextBox);   



        Transform settingsBackTrasnform = new Transform(Settings.CanvasWidth / 2, 500, 250, 50);
        settingsBack = new Button("Back", settingsBackTrasnform);
        RegisterText(settingsBack);   




        //finally, set our main screen.
        SwitchScreen(0);
    }

    //screen update functions.
    void SwitchScreen(int screenNum)
    {

        //disable all texts.
        foreach (DrawText t in base.DrawTextsToRender)
        {
            t.disableRender = true;
        }


        cScreen = screenNum;
         if (screenNum == 0)
        {
            //kind of tedious, enable all the texts
            title.disableRender = false;
            playText.disableRender = false;
            settingsText.disableRender = false;


         }
        else if (screenNum == 1)
        {
            lobbyTitle.disableRender = false;
            lobbyBack.disableRender = false;
            lobbyInput.disableRender = false;
            requestButton.disableRender = false;
            joinButton.disableRender = false;
            currentLobyText.disableRender = false;
        }
        else if (screenNum == 2)
        {
            settingsTitle.disableRender = false;
            settingsBack.disableRender = false; 
            playerNameTextBox.disableRender = false;
        }
    }
    public void MainScreen()
    {


        if (playText.clicked)
        {
            SwitchScreen(1);
        }


        if (settingsText.clicked)
        {
            SwitchScreen(2);
        }


    }

    public async Task LobbyScreen()
    {

        if (nm.myLobby == "")
        {
            currentLobyText.text = "Current Lobby: None";
            this.launchText.disableRender = true;
            this.requestButton.disableRender = false;
            this.joinButton.disableRender = false;
            this.lobbyInput.disableRender = false;
        }
        else
        {
            this.requestButton.disableRender = true;
            this.joinButton.disableRender = true;
            this.lobbyInput.disableRender = true;
            this.launchText.disableRender = false;
            currentLobyText.text = "Current Lobby: " + nm.myLobby;
        }


            if (joinButton.clicked)
            {
                bool success = await RequestJoinLobby(lobbyInput.placeholder);
                if (success)
                {
                    this.currentLobyText.text = "Current Lobby: " + nm.myLobby;
                }
                else
                {
                    this.lobbyInput.setTextColor(Color.Red,255);
                    this.lobbyInput.placeholder = "FAILED";
                }
            }



            if (requestButton.clicked)
            {
                bool success = await RequestNewLobby(lobbyInput.placeholder);
                if (success)
                {
                    this.currentLobyText.text = "Current Lobby: " + nm.myLobby;
                }
                else
                {
                    this.lobbyInput.setTextColor(Color.Red,255);
                    this.lobbyInput.placeholder = "FAILED";
                }
            }
        


   
            if (launchText.clicked)
            {
                //launch the game!
                //naviate to the game screen
                Nav.NavigateTo("/Game");

            }
        



            if (lobbyBack.clicked)
            {
                SwitchScreen(0);
            }
        

    }

    public void SettingsScreen()
    {
        if (settingsBack.clicked)
        {
            SaveToStorage();
            SwitchScreen(0);
        }


    }
    //update functions. Unlike game, these are called at the same rate! 
    public override void Update()
    {
        base.Update();


        foreach (Star s in stars)
        {
            s.transform.rect.X += starSpeed;
            if (s.transform.rect.X > Settings.CanvasWidth + s.transform.rect.Width)
            {
                s.transform.rect.X = 0 - s.transform.rect.Width;
            }
        }

        foreach (DrawText t in collideableTexts)
        {
            if (!t.disableRender)
            {
                if (t.GetType() == typeof(Button))
                {
                    Button b = (Button)t;
                    b.Update();
                }
                else if (t.GetType() == typeof(InputField))
                {
                    InputField i = (InputField)t;
                    i.Update();
                }
            }
        }
        
        switch (cScreen)
        {
            case 0:
                MainScreen();
                break;
            case 1:
                LobbyScreen();
                break;
            case 2:
                SettingsScreen();
                break;
        }

    }

    public override void Render(float deltaTime)
    {
        base.Render(deltaTime);
    }



    //network call functions
    public async Task<bool> RequestNewLobby(string lobbyName)
    {

        if (lobbyName.Length == 0)
        {
            return false;
        }
        var response = await nm.client.SendWithResponse("{NewLobby}",null,lobbyName);

        if (response != null)
        {
            // Populate the UI with the args returned from the server
            var lobby = response.Args.ToList()[0];
            nm.isHost = true;
            nm.myLobby = lobby;
            Console.WriteLine("Lobby receieved: " +lobby);
            return true;
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
        }
        return false;
    }
    public async Task<bool> RequestJoinLobby(string lobbyName)
    {
        if (lobbyName.Length == 0)
        {
            return false;
        }

        var response = await nm.client.SendWithResponse("{JoinLobby}",null,lobbyName);

        if (response != null)
        {
            var args = response.Args.ToList();
            var resp = args[0];//what did the server say
            if (resp == "{Success}")
            {
                this.nm.myLobby = lobbyInput.placeholder;
                return true;
            }
            else
            {
                return false;
                //tell we failed to join
            }
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
            return false;
        }

    }

    //settings functions
    void Save()
    {
      localStorage.SetItemAsStringAsync("settingsSaveKey",Settings.Save());
    }
    void SaveToStorage()
    {
         nm.client.Send("{SetName}",null,Settings.name);
         Save();
    }
}