using System;
using Shared;

namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;

    public NetworkManager()
    {
        client = new Client(this);
        client.ConnectToServer();
    }

    public void Process(Packet p)
    {
        
    }

}
