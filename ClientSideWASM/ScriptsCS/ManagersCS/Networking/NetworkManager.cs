using System;
using Shared;

namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public string myLobby = "";
    public bool isHost = false;

    public NetworkManager()
    {
        client = new Client(this);

    }
    public async Task Initialize()
    {
        
        await client.ConnectToServer();
    }

}
