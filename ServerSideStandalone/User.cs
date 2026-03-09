namespace ServerSideStandalone;
using Shared;
using System.Net.WebSockets;


public class User : NetworkModel
{
    Server server;
    public string name;
    public Lobby myLobby ;
    public string currentPage;
    public Guid uid;



    public User( Server s)
    {
        this.uid = Guid.NewGuid();
        this.server = s;
        
        //Initialzie the net code.

    }   

    protected override async Task<string[]> HandleRecvWithResponse(string purpose, byte[] data, string[] args)
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
                case "{RequestUID}":
                    return new string[] {uid.ToString()};
                default:
                    return new[] { "Error", "Unknown Command" };
            }
    }
    protected override async Task HandleRecv(string purpose, byte[] data, string[] args)
    {
        //Console.WriteLine("Recieved one time update: " + purpose);
        switch (purpose)
            {
                case "{GameStateUpdate}":
                    myLobby.UpdateState(data);
                    break;
                case "{PlayerUpdate}":
                    myLobby.UpdateUser(this,data);
                    break;
                case "{SpawnGameObject}":
                    
                    myLobby.SpawnGameObject(data);
                    break;
                case "{SetName}":
                    this.name = args[0];
                    break;
                case "{SetPage}":
                    this.currentPage = args[0];
                    break;

                case "{Input}":
                    //We can handle some input on the server, but for now we just send it to the host to handle. 
                    myLobby.AddInput(this,data);
                    break;

                default:
                    Console.WriteLine("Error: Unknown Purpose: " + purpose);// new[] { "Error", "Unknown Command" };
                    break;
            }
    }
    

}