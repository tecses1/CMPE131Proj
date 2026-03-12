using System.Data;
using Shared;
using System;
using System.Diagnostics;

namespace ServerSideStandalone;
public class Lobby
{
    public string Name;
    public byte[] State;
    public InputWrapper[] playerInputs;
    List<User> users = new List<User>();
    //host the game logic on the server. :D
    GameLogic gl;
    Stopwatch timer = new Stopwatch();

    public LobbyNode node;
    int ticks;
    DateTime clock = DateTime.Now;
    public Lobby()
    {
        gl = new GameLogic();
        Console.WriteLine("Lobby created, gamelogic initialized.");
    }
    public bool isEmpty()
    {
        return this.users.Count == 0;
    }
    public void AddUser(User user)
    {

        users.Add(user);
        playerInputs = new InputWrapper[users.Count];
        
        //Add a player.
        Player p = new Player(new Transform(GameConstants.worldSizeX / 2, GameConstants.worldSizeY/2, 50,50));
        p.playerNameString = user.name;
        p.uid = user.uid;
        gl.AddPlayer(p);
        p.RegisterGameLogic(gl);
        Console.WriteLine("user added: " +user.name + ", Debug: " + users.Count + "," + playerInputs.Length);
    }
    public bool TimeOut()
    {
        return timer.Elapsed > TimeSpan.FromSeconds(30);
    }
    public void Update()
    {
        if (isEmpty())
        {
            timer.Start();
            Console.WriteLine("lobby is empty. SKipping update.");
            return;
        }
        else
        {
            try
            {
                // Update the lobby node information
                node.PlayerCount = users.Count;
                node.UserList = GetUsers();
                node.tps = "Ticks per second: " + ticks;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating lobby information: " + ex.Message);
            }
            timer.Reset();
        }
        ticks++;
        DateTime frameStamp = DateTime.Now;
        //Get the input wrappers for each player.
        for (int i = 0; i < playerInputs.Length; i++){
            if (playerInputs[i] == null)
            {
                //Console.WriteLine("Warning: null player input " + i);
                continue;
            }
            Player p = gl.getPlayerWithUID(playerInputs[i].owner);
            if (p != null)
            {
                p.cInput = playerInputs[i];
            }else Console.WriteLine("Player with " + playerInputs[i].owner + " not found. Input not registered!");
        }
        //update the world.
        gl.Update();

        //Create the gamestate to send back.
        State = gl.GetGameState(frameStamp);

        //Update Gamestate to clients
        UpdateState(State);

        if (DateTime.Now - clock > TimeSpan.FromSeconds(1))
        {
            Console.WriteLine(ticks + " ticks in the last second.");
            ticks = 0;
            clock = DateTime.Now;
        }

    }
    public bool isHost(User user)
    {
        return users[0] == user;
    }
    //Called when host sends over their gamestate.
    public void UpdateState(byte[] newState)
    {
        for (int i = 0; i < users.Count; i++)
        {
            //Console.WriteLine ("Sending gamestate to user " + i);
            users[i].Send("{GameStateUpdate}", newState);
        }

    }
    
    public void SpawnGameObject(byte[] gameObjectData)
    {
        //Request the host to spawn this game object.
        Console.WriteLine("Request to spawn game object: " +gameObjectData.Length );
        users[0].Send("{SpawnGameObject}", gameObjectData );
        
    }

    public void AddInput(User user, byte[] inputData)
    {
        for (int i = 0; i < users.Count; i++)
        {
            if (user == users[i])
            {
                playerInputs[i] = InputWrapper.FromBytes(inputData);
                //Console.WriteLine("Setting player input to" + playerInputs[i].ToString());
            }
        }
        //send the input to the host to handle. 
        //users[0].Send("{Input}", inputData);
    }

    public string GetUsers()
    {
        
        string userList = "Users in lobby:\n";
        foreach (User u in users)
        {
            userList += "- " + u.name + ", UID: " + u.uid + "\n";
        }
        return userList;
    }
}