using System;
using Shared;

namespace ClientSideWASM;

//Connection to server, and updates will happen here!

public class NetworkManager
{
    public Client client;
    public string myLobby = "";
    public bool isHost = false;
    public byte[] gameState;
    public byte[][] playerStates = {};
    public List<byte[]> objsToAdd = new List<byte[]>();

    public NetworkManager()
    {
        client = new Client(this);

    }
    public async Task Initialize()
    {
        
        await client.ConnectToServer();
    }

}
