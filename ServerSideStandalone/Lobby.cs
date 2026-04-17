
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
    Stopwatch tickClock = Stopwatch.StartNew();
    DateTime clock;
    Stopwatch contaminationTimer = new Stopwatch();

    int timeToUpdate_e; //extra update time
    int timeToUpdate_s; //lolad inputs and update logic
    int timeToUpdate_u; //time to update users
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
        if (users.Contains(user)) return;
        users.Add(user);
        Console.WriteLine("user added: " +user.name + ", Debug: " + users.Count);
        //GeneratePlayer(user);
    }
    public void RemoveUser(User user)
    {
        try{
        gl.getPlayerWithUID(user.uid)?.Kill();
        }catch(Exception e)
            {
                Console.WriteLine("Werird...Player not found. It was already removed?");
            }
        users.Remove(user);
        user.myLobby = null;
        user.myInputData = null;
        Console.WriteLine("user removed: " + user.name + ", Debug: " + users.Count);
        

    }
    public void GeneratePlayer(User u)
    {
                //Add a player, 0,0 is center of world.
        Player p = new Player(new Transform(0, 0, 50,50));
        p.playerNameString = u.name;
        p.uid = u.uid;
        gl.AddPlayer(p);
        p.RegisterGameLogic(gl);
        Console.WriteLine("Making new player: " + p.playerNameString);  
    }
    public bool TimeOut()
    {
        return timer.Elapsed > TimeSpan.FromSeconds(30);
    }
    public void Update()
    {
        contaminationTimer.Restart();
        Application.Current.Dispatcher.Invoke(() =>
        {
            node.PlayerCount = users.Count;
            node.UserList = GetUsers();
            node.tps = "Ticks per second: " + tps;
            node.ContaminationTime = timeToUpdate_e + timeToUpdate_s + timeToUpdate_u;
            node.ExtraTime = timeToUpdate_e;
            node.UserTime = timeToUpdate_u;
            node.UpdateTime = timeToUpdate_s;

        });
        if (isEmpty())
        {
            timer.Start();
            Console.WriteLine("lobby is empty. SKipping update.");
            return;
        }
        else
        {
            timer.Reset();
        }
        ticks++;

        
        
        //Get the input wrappers for each player.
        for (int i = 0; i < users.Count; i++){
            if (users[i].myInputData == null)
            {
                //Console.WriteLine("Warning: null player input " + i);
                continue;
            }
            InputWrapper input = InputWrapper.FromBytes(users[i].myInputData);

            Player p = gl.getPlayerWithUID(input.owner);
            if (p != null) // if a player exists, apply input
            {
                p.cInput = input;
            }
            else //otherwise, we should check if we have input and make the player
            {
                GeneratePlayer(users[i]);
                
            } 
        }
        timeToUpdate_e = (int)contaminationTimer.ElapsedMilliseconds;
        //update the world.
        gl.Update();
        timeToUpdate_s = (int)contaminationTimer.ElapsedMilliseconds;
        timeToUpdate_s -= timeToUpdate_e;

        //Create the gamestate to send back.
        //State = gl.GetGameState(frameStamp);

        //Update Gamestate to clients
        UpdateState();

        if (DateTime.Now - clock > TimeSpan.FromSeconds(1))
        {
            //Console.WriteLine(ticks + " ticks in the last second.");
            tps = ticks;
            ticks = 0;
            clock = DateTime.Now;
        }

        timeToUpdate_u = (int)contaminationTimer.ElapsedMilliseconds;
        timeToUpdate_u -= (timeToUpdate_e + timeToUpdate_s);
        


    }
    public bool isHost(User user)
    {
        return users[0] == user;
    }
    //Called when host sends over their gamestate.
    public void UpdateState()
    {
        for (int i = 0; i < users.Count; i++)
        {
            //Console.WriteLine ("Sending gamestate to user " + i);
            byte[] newState = gl.GetGameStateCulled(tickClock.ElapsedMilliseconds, users[i].uid);
            //Console.WriteLine("DEBUG: CUlled length: " + newState.Length + ", Unculled length: " + State.Length);   
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