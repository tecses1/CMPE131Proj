
namespace ServerSideStandalone;
using System.Windows;
using System.Collections.ObjectModel;
using System;
using System.Windows.Controls;
using System.Data;
using Shared;
using System.Diagnostics;

public class Lobby
{
    public string Name;
    public byte[] State;
    List<User> users = new List<User>();
    //host the game logic on the server. :D
    GameLogic gl;
    Stopwatch timer = new Stopwatch();

    public LobbyNode node;
    int tps;
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

        
        //Add a player, 0,0 is center of world.
        Player p = new Player(new Transform(0, 0, 50,50));
        p.playerNameString = user.name;
        p.uid = user.uid;
        gl.AddPlayer(p);
        p.RegisterGameLogic(gl);
        Console.WriteLine("user added: " +user.name + ", Debug: " + users.Count);
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
                // Update the lobby node information
                Application.Current.Dispatcher.Invoke(() =>
                {
                node.PlayerCount = users.Count;
                node.UserList = GetUsers();
                node.tps = "Ticks per second: " + tps;
                });

            
            timer.Reset();
        }
        ticks++;

        
        DateTime frameStamp = DateTime.Now;
        //Get the input wrappers for each player.
        for (int i = 0; i < users.Count; i++){
            if (users[i].myInputData == null)
            {
                //Console.WriteLine("Warning: null player input " + i);
                continue;
            }
            InputWrapper input = InputWrapper.FromBytes(users[i].myInputData);
            Player p = gl.getPlayerWithUID(input.owner);
            if (p != null)
            {
                p.cInput = input;
            }else Console.WriteLine("Player with " + input.owner + " not found. Input not registered!");
        }
        //update the world.
        gl.Update();

        //Create the gamestate to send back.
        State = gl.GetGameState(frameStamp);

        //Update Gamestate to clients
        UpdateState(State);

        if (DateTime.Now - clock > TimeSpan.FromSeconds(1))
        {
            //Console.WriteLine(ticks + " ticks in the last second.");
            tps = ticks;
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