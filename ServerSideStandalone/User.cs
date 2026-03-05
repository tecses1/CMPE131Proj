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

    protected override async Task<string[]> HandleRecvWithResponse(string purpose, string[] args)
    {
        Console.WriteLine("Recieved update that requires response." + purpose);
        switch (purpose)
            {
                case "{NewLobby}":
                    string newLobby = myServer.getLobby();
                    myLobby = newLobby;
                    return new string[] {myLobby};
                case "{JoinLobby}":
                    if (myServer.getLobby(args[0]))
                    {
                        //That lobby exists.
                        myLobby = args[0];
                        return new string[] {"{Success}"};
                        
                    }
                    else return new string[]{"{Failed}"};
                case "{SetName}":

                default:
                    return new[] { "Error", "Unknown Command" };
            }
    }
    protected override async Task HandleRecv(string purpose, string[] args)
    {
        Console.WriteLine("Recieved one time update: " + purpose);
        switch (purpose)
            {
                case "{SetName}":
                    this.name = args[0];
                    break;
                case "{SetPage}":
                    this.currentPage = args[0];
                    break;

                default:
                    Console.WriteLine("Error: Unknown Purpose: " + purpose);// new[] { "Error", "Unknown Command" };
                    break;
            }
    }
    

}