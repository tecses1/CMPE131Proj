namespace ServerSideStandalone;
using Shared;
using System.Net.WebSockets;


public class User : NetworkModel
{
    Server myServer;
    public string name;
    public string myLobby = "";
    public string currentPage;
    public int uid;


    public User(int uid, Server s)
    {
        this.uid = uid;
        this.myServer = s;
        
        //Initialzie the net code.

    }   

    protected override async Task<string[]> HandleRequestAsync(string purpose, string[] args)
    {
        switch (purpose)
            {
                case "GetInventory":
                    string playerId = args[0];
                    
                    // Look up data in your WPF server's database/list
                    string[] items = {"This", "Hurts", "My", "Brain"}; 
                    
                    // Returning this array triggers the automatic response logic 
                    // in the base NetworkModel, which sends it back with the matching ID.
                    return items;

                case "GlobalChat":
                    string username = args[0];
                    string message = args[1];
                    
                    Console.WriteLine($"[CHAT] {username}: {message}");
                    return null;

                default:
                    return new[] { "Error", "Unknown Command" };
            }
    }
    

}