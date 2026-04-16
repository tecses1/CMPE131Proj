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
            DrawText settingsText;
            DrawText playText;
        //lobby screen
            DrawText lobbyTitle;
            DrawText currentLobyText;
            InputField lobbyInput;
            DrawText launchText;
            DrawText requestButton;
            DrawText joinButton;

            DrawText lobbyBack;
        //settings screen
            DrawText settingsTitle;
            InputField playerNameTextBox;
            
            DrawText settingsBack;

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

        Transform currentLobbyTransform = new Transform(Settings.CanvasWidth / 2, 200, 200, 50);
        currentLobyText = new DrawText("Current Lobby: None", currentLobbyTransform);
        RegisterText(currentLobyText,false);   

        Transform lobbyTextBoxTransform = new Transform(Settings.CanvasWidth / 2, 300, 400, 50);
        lobbyInput = new InputField("", lobbyTextBoxTransform);
        RegisterText(lobbyInput,true);   

        Transform requestButtonTransform = new Transform(Settings.CanvasWidth / 2-50, 400, 100, 50);
        requestButton = new DrawText("Create", requestButtonTransform);
        RegisterText(requestButton,true);   

        Transform joinButtonTransform = new Transform(Settings.CanvasWidth / 2 + 50, 400, 100, 50);
        joinButton = new DrawText("Join", joinButtonTransform);
        RegisterText(joinButton,true);   

        Transform launchTextTransform = new Transform(Settings.CanvasWidth / 2, 400, 250, 70);
        launchText = new DrawText("LAUNCH!", launchTextTransform);
        RegisterText(launchText,true);   

        Transform lobbyBackTransform = new Transform(Settings.CanvasWidth / 2, 500, 250, 50);
        lobbyBack = new DrawText("Back", lobbyBackTransform);
        RegisterText(lobbyBack,true);   

        //Settings screen
        Transform settingsTitleTransform = new Transform(Settings.CanvasWidth / 2, 100, 250, 70);
        settingsTitle = new DrawText("Settings", settingsTitleTransform);
        RegisterText(settingsTitle,false);   

        Transform settingsPlayerNameTextBoxTransform = new Transform(Settings.CanvasWidth / 2, 200, 250, 50);
        playerNameTextBox = new InputField(Settings.name, settingsPlayerNameTextBoxTransform);

        RegisterText(playerNameTextBox,true);   



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

    public async Task LobbyScreen(List<DrawText> hovered)
    {
        if (editing == lobbyInput)
        {
            //check if player clicked outside of the textbox.
            bool check = userInput.CLeftPressed && !hovered.Contains(lobbyInput); 
            if (lobbyInput.Update(userInput) || check) // handles the ticking
            {
                // Handle the case when the user presses Enter
                editing = null; // stop editing

            }
        }
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

        if (hovered.Contains(joinButton))
        {
            joinButton.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
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
        }
        else
        {
            joinButton.setTextColor(Color.White,255);
        }

        if (hovered.Contains(requestButton))
        {
            requestButton.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
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
        }
        else
        {
            requestButton.setTextColor(Color.White,255);
        }

        if (hovered.Contains(launchText))
        {
            launchText.setTextColor(Color.Blue,255);
            if (userInput.CLeftPressed)
            {
                //launch the game!
                //naviate to the game screen
                Nav.NavigateTo("/Game");

            }
        }
        else
        {
            launchText.setTextColor(Color.White,255);
        }

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


        if (hovered.Contains(lobbyInput))
        {
            lobbyInput.setFillColor(Color.LightYellow,255);
            if (userInput.CLeftPressed)
            {
                editing = lobbyInput;
                lobbyInput.Select();
            }
        }
        else
        {
            lobbyInput.setFillColor(Color.White,255);
        }
    }

    public void SettingsScreen(List<DrawText> hovered)
    {
        if (editing == playerNameTextBox)
        {
            //check if player clicked outside of the textbox.
            bool check = userInput.CLeftPressed && !hovered.Contains(playerNameTextBox); 
            if (playerNameTextBox.Update(userInput) || check) // handles the ticking
            {
                if(playerNameTextBox.placeholder == "") { //Doesn't allow blank name
                    playerNameTextBox.placeholder = "Player";
                }
                // Handle the case when the user presses Enter
                editing = null; // stop editing
                Settings.name = playerNameTextBox.placeholder; // save the new name
                SaveToStorage(); // save to local storage and send to server
                playerNameTextBox.Deselect();
            }
        }

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

        if (hovered.Contains(playerNameTextBox))
        {
            playerNameTextBox.setFillColor(Color.LightYellow,255);
            if (userInput.CLeftPressed)
            {
                editing = playerNameTextBox;
                playerNameTextBox.Select();
                if (playerNameTextBox.placeholder == "Player") {
                    playerNameTextBox.placeholder = "";
                }
            }
        }
        else
        {
            playerNameTextBox.setFillColor(Color.White,255);
        }


    }
    //update functions. Unlike game, these are called at the same rate! 
    public override void Update()
    {
        base.Update();

        //FIX THIS LTATER.
        //All classes can now reference input directly.
        this.userInput = InputManager.currentInput;

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