namespace ServerSideStandalone;
using Shared;
using System.Data;
using System.Net.WebSockets;

using System.Windows;
using System.Windows.Controls;

public class User : NetworkModel
{
    Server server;
    public string name;
    public Lobby myLobby ;
    public string currentPage;
    public Guid uid;
    bool read = false;
    int skipped = 0;
    private byte[] _myInputData;

    public byte[] myInputData
    {
        get
        {   
            skipped = 0;
            read = true;
            return _myInputData;
        }
        set
        {
            if (!read)
            {
                skipped++;
            }
            read = false;
            _myInputData = value;
        }
    }
    public ClientNode node;

    public User( Server s)
    {
        this.uid = Guid.NewGuid();
        this.server = s;
        this.node = new ClientNode() { Username = "New User", IPAddress = GetAddress(), uid = uid.ToString(), CurrentPage = "Init", Lobby = "None", Latency = "placeholder ms" };
        Application.Current.Dispatcher.Invoke(() =>
        {
            ((MainWindow)Application.Current.MainWindow).Clients.Add(node);
        });
        //Initialzie the net code.

    }   
    public int GetSkipped()
    {
        return skipped;
    }
    public void Update()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            this.node.Username = name;
            this.node.CurrentPage = currentPage;
            this.node.Lobby = myLobby != null ? myLobby.Name : "None";
            this.node.IPAddress = GetAddress();
            this.node.Latency = "" + GetLatency() + " ms";
            this.node.uid = uid.ToString();
            this.node.Skipped = "" + GetSkipped();
        });
    }
    protected override async Task<string[]> HandleRecvWithResponse(string purpose, byte[] data, string[] args)
    {
        //Console.WriteLine("Recieved update that requires response." + purpose);
        switch (purpose)
            {
                case "{NewLobby}":
                    Lobby l = server.CreateLobby(args[0]);
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
                    //myLobby.UpdateState(data);
                    Console.WriteLine("Weird call for gamestate update...");
                    break;
                case "{SetName}":
                    this.name = args[0];
                    break;
                case "{SetPage}":
                    this.currentPage = args[0];
                    break;

                case "{Input}":
                    //We can handle some input on the server, but for now we just send it to the host to handle. 
                    myInputData = data;
                    break;
                case "{LeaveLobby}":
                    if (myLobby != null)
                    {
                        myLobby.RemoveUser(this);//.Remove(this);
                        
                    }
                    break;
                default:
                    Console.WriteLine("Error: Unknown Purpose: " + purpose);// new[] { "Error", "Unknown Command" };
                    break;
            }
    }
    

}