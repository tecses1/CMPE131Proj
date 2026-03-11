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
        if (!nm.client.isConnected()){
            Nav.NavigateTo("/");
        }
        await nm.client.Send("{SetName}",null,Settings.name);
        await nm.client.Send("{SetPage}",null,this.GetType().Name);
    }

    public async Task RequestNewLobby()
    {
        var response = await nm.client.SendWithResponse("{NewLobby}");

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
        var response = await nm.client.SendWithResponse("{JoinLobby}",null,requestedLobby);

        if (response != null)
        {
            var args = response.Args.ToList();
            var resp = args[0];//what did the server say
            if (resp == "{Success}")
            {
                this.nm.myLobby = requestedLobby;
            }
            else
            {
                failedToJoinLobby = "Failed to find the Lobby. Please check the name and try again.";
            }
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("Server didn't respond in time.");
        }
    
        
    }
    //Called when button is pr essed, see above.
    private async Task SaveToStorage()
    {
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            // Saves the current value of 'userInput' to local storage
            
            Settings.name = userInput;
            await nm.client.Send("{SetName}",null,Settings.name);
            await Save();
        }

    }
    //Reference method. Called from OnIntializedAsync().
    async Task Save(){
        await LocalStorage.SetItemAsStringAsync("settingsSaveKey",Settings.Save());

    }
}