namespace ClientSideWASM.Pages;

using System.ComponentModel.DataAnnotations.Schema;
using Blazorex;
using Shared;

public partial class Home
{
    public string userInput;
    public string lobbyName;
    public string responseLobby;
    
    protected override void OnInitialized()
    {
        userInput = Settings.name;
        // Example: Set a default supplier if null
        Packet p = new Packet();
        p.customMessage = "{PageUpdate}" + '\x1F' + "Home";
        nm.client.QueueToSend(p);

    }
    protected override async Task OnInitializedAsync()
    {


        

    }

    public void Request()
    {
        Packet p = new Packet();
        p.customMessage = "requestLobby";

        nm.client.QueueToSend(p);
    }
    //Called when button is pressed, see above.
    private async Task SaveToStorage()
    {
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            // Saves the current value of 'userInput' to local storage
            Settings.name = userInput;
            await Save();
        }
    }
    //Reference method. Called from OnIntializedAsync().
    async Task Save(){
        await LocalStorage.SetItemAsStringAsync("settingsSaveKey",Settings.Save());

    }
}