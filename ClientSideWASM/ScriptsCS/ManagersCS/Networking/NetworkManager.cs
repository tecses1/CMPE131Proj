using System;
using Shared;

namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public string myLobby = "";

    public NetworkManager()
    {


    }
    public async Task Initialize()
    {
        client = new Client(this);
        await client.ConnectToServer();
    }

}
