using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.JSInterop;
using Shared;
using Blazored.LocalStorage;
using System.Runtime.CompilerServices;
using System.Drawing;
namespace ClientSideWASM;
public class HomeManager : RenderManager
{
    public ClientInputWrapper userInput;
    public string requestedLobby = "";

    public string failedToJoinLobby = "";



    //text stuff
    List<DrawText> collideableTexts = new List<DrawText>();
        // main screen
            DrawText title;
            DrawText settingsText;
            DrawText playText;
        //lobby screen
            DrawText lobbyTitle;
            DrawText lobbyBack;
        //settings screen
            DrawText settingsTitle;
            DrawText settingsBack;

    //bg stars
    float starSpeed = 0.1f;
    List<GameObject> stars = new List<GameObject>();

    Transform mouse  = new Transform(0,0,10,10);

    //functional references
    IJSRuntime JS;
    ILocalStorageService localStorage;
    NetworkManager nm;

    public int cScreen = 0; //0 is main menu, 1 is lobby screen, 2 is settings
    public HomeManager(IJSRuntime JS, ILocalStorageService localStorage, NetworkManager nm) : base(JS)
    {
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
    public void ApplyInput(ClientInputWrapper input)
    {
        userInput = input;
    }


    //screen functions

    public void RegisterText(DrawText t, bool collideable = false)
    {
        if (collideable)
        {
            this.collideableTexts.Add(t);
        }
        RegisterDrawTextToRender(t);
    }
    public void Initialize()
    {
        //initialize ALLLLLLLL the things.

        //Main Title Inititialize
        Transform titleTransform = new Transform(Settings.CanvasWidth / 2, 100, 600, 70);
        title = new DrawText("GALAX.IO", titleTransform);
        RegisterText(title,false);   


        Transform playTransform = new Transform(Settings.CanvasWidth / 2, 250, 250, 50);
        playText = new DrawText("Play", playTransform);
        RegisterText(playText,true);   

        Transform settingsTransform = new Transform(Settings.CanvasWidth / 2, 300, 250, 50);
         settingsText = new DrawText("Settings", settingsTransform);
        RegisterText(settingsText,true);   


        //Lobby screen
  
        Transform lobbyTitleTrasnform = new Transform(Settings.CanvasWidth / 2, 100, 250, 70);
        lobbyTitle = new DrawText("Lobbies", lobbyTitleTrasnform);
        RegisterText(lobbyTitle,false);   



        Transform lobbyBackTransform = new Transform(Settings.CanvasWidth / 2, 500, 250, 50);
        lobbyBack = new DrawText("Back", lobbyBackTransform);
        RegisterText(lobbyBack,true);   

        //Settings screen
        Transform settingsTitleTransform = new Transform(Settings.CanvasWidth / 2, 100, 250, 70);
        settingsTitle = new DrawText("Settings", settingsTitleTransform);
        RegisterText(settingsTitle,false);   



        Transform settingsBackTrasnform = new Transform(Settings.CanvasWidth / 2, 500, 250, 50);
        settingsBack = new DrawText("Back", settingsBackTrasnform);
        RegisterText(settingsBack,true);   




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
        }
        else if (screenNum == 2)
        {
            settingsTitle.disableRender = false;
            settingsBack.disableRender = false; 
        }
    }
    public void MainScreen(List<DrawText> hovered)
    {


        if (hovered.Contains(playText))
        {
            playText.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
            {
                SwitchScreen(1);
            }
        }
        else
        {
            playText.setTextColor(Color.White,255);
        }

        if (hovered.Contains(settingsText))
        {
            settingsText.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
            {
                SwitchScreen(2);
            }
        }
        else
        {
            settingsText.setTextColor(Color.White,255);
        }
    }

    public void LobbyScreen(List<DrawText> hovered)
    {
        if (hovered.Contains(lobbyBack))
        {
            lobbyBack.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
            {
                SwitchScreen(0);
            }
        }
        else
        {
            lobbyBack.setTextColor(Color.White,255);
        }
    }

    public void SettingsScreen(List<DrawText> hovered)
    {
        if (hovered.Contains(settingsBack))
        {
            settingsBack.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
            {
                SwitchScreen(0);
            }
        }
        else
        {
            settingsBack.setTextColor(Color.White,255);
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

        //make sure the rect for the mouse is updated to our client input
        this.mouse.rect.X = (float) userInput.MouseX;
        this.mouse.rect.Y = (float) userInput.MouseY;
        //C heck if mouse is over these texts, we can do dynamic stuff with this.
        List<DrawText> texthovered = new List<DrawText>();
        foreach(DrawText t in this.collideableTexts)
        {
            //Console.WriteLine("Checking intersection with text: " + t.text + " at " + t.transform.rect.X + "," + 
            //t.transform.rect.Y + "," + t.transform.rect.Width + "," + t.transform.rect.Height + " | mosue at " 
            //+ mouse.rect.X + "," + mouse.rect.Y + "," + mouse.rect.Width + "," + mouse.rect.Height);
            if (mouse.rect.IntersectsWith(t.transform.rect) && !t.disableRender)
            {
                //clicked on a text object, pass that info to the update function so it can handle it.
                texthovered.Add(t);
            }
        }
        
        switch (cScreen)
        {
            case 0:
                MainScreen(texthovered);
                break;
            case 1:
                LobbyScreen(texthovered);
                break;
            case 2:
                SettingsScreen(texthovered);
                break;
        }

    }

    public override void Render(float deltaTime)
    {
        base.Render(deltaTime);
    }



    //network call functions
    public async Task RequestNewLobby()
    {
        var response = await nm.client.SendWithResponse("{NewLobby}",null,"lobbyName");

        if (response != null)
        {
            // Populate the UI with the args returned from the server
            var lobby = response.Args.ToList()[0];
            nm.isHost = true;
            nm.myLobby = lobby;
            Console.WriteLine("Lobby receieved: " +lobby);
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
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
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