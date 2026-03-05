namespace ClientSideWASM.Pages;

using System.ComponentModel.DataAnnotations.Schema;
using Blazorex;
using Shared;

public partial class Home
{
    public string userInput;
    public string requestedLobby = "";

    public string failedToJoinLobby = "";
    protected override void OnInitialized()
    {
        userInput = Settings.name;
        // Example: Set a default supplier if null

    }
    protected override async Task OnInitializedAsync()
    {


        

    }

    public async Task RequestNewLobby()
    {
        var response = await nm.client.SendWithResponse("GetInventory", new[] { "PlayerID_99" });

            if (response != null)
            {
                // Populate the UI with the args returned from the server
                var _items = response.Args.ToList();
                Console.WriteLine(_items);
            }
            else
            {
                // This happens if the 5-second timeout we built triggers!
                Console.WriteLine("Server didn't respond in time.");
            }
    }
    public async Task RequestJoinLobby()
    {
        await nm.client.Send("GlobalChat", "Hero123", "Hello everyone!");
    
        
    }
    //Called when button is pr essed, see above.
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