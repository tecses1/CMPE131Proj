namespace ClientSideWASM.Pages;

using System.ComponentModel.DataAnnotations.Schema;
using Blazorex;

public partial class Home
{
    public string userInput;
    
    protected override void OnInitialized()
    {
        // Example: Set a default supplier if null

    }
    protected override async Task OnInitializedAsync()
    {


        

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