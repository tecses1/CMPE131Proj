using System;
using Shared;

namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public bool initialized;
    string stackTrace = Environment.StackTrace;
    public NetworkManager()
    {


    }
    public async Task Initialize()
    {
        client = new Client(this);
        await client.ConnectToServer();
    }
    public void Process(Packet p)
    {
        
    }

}
