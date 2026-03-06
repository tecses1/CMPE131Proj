namespace ServerSideStandalone;
using Shared;
using System.Net.WebSockets;


public class User : NetworkModel
{
    Server server;
    public string name;
    public Lobby myLobby ;
    public string currentPage;
    public int uid;



    public User(int uid, Server s)
    {
        this.uid = uid;
        this.server = s;
        
        //Initialzie the net code.

    }   

    protected override async Task<string[]> HandleRecvWithResponse(string purpose, string[] args)
    {
        //Console.WriteLine("Recieved update that requires response." + purpose);
        switch (purpose)
            {
                case "{NewLobby}":
                    Lobby l = server.CreateLobby();
                    l.AddUser(this);
                    myLobby = l;
                    return new string[] {myLobby.Name};
                case "{JoinLobby}":
                    Lobby find = server.GetLobby(args[0]);
                    if (find != null)
                    {
                        //That lobby exists.
                        find.AddUser(this);
                        myLobby = find;
                        
                        return new string[] {"{Success}"};
                        
                    }
                    else return new string[]{"{Failed}"};
                case "{IsHost}":
                    if (myLobby != null)
                    {
                        if (myLobby.isHost(this)) return new string[] {"{Yes}"};
                        else return new string[] {"{No}"};
                    }
                    return new string[] {"{NoLobby}"};
                case "{RequestGameState}":
                    if (myLobby != null)
                    {
                        return new string[] {myLobby.State};
                    }
                    return new string[] {"{NoLobby}"};
                default:
                    return new[] { "Error", "Unknown Command" };
            }
    }
    protected override async Task HandleRecv(string purpose, string[] args)
    {
        //Console.WriteLine("Recieved one time update: " + purpose);
        switch (purpose)
            {
                case "{GameUpdate}":
                    myLobby.UpdateState(args[0]);
                    break;
                case "{PlayerUpdate}":
                    myLobby.UpdateUser(this,args[0]);
                    break;
                case "{SpawnGameObject}":
                    
                    myLobby.SpawnGameObject(args[0]);
                    break;
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