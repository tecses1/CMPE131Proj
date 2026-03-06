using Shared;

namespace ServerSideStandalone;
public class Lobby
{
    public string Name;
    public byte[] State;
    public byte[][] playerStates;

    List<User> users = new List<User>();
    
    

    public void AddUser(User user)
    {

        users.Add(user);
        playerStates = new byte[users.Count][];
        
        Console.WriteLine("user added: " + users.Count + "," + playerStates.Length);
    }
    public bool isHost(User user)
    {
        return users[0] == user;
    }

    public void UpdateState(byte[] newState)
    {
        //send state to all users. no need to keep it in memory, unless we want async updating. 
        State = newState;
        //Update();
    }
    
    public void Update()
    {
        for (int i = 0; i < users.Count; i++)
        {
            users[i].Send("{GameStateUpdate}",  State );
            //Serialize the array of player states into 

            //remove our state, we don't have to send it to the local machine.
            List<byte[]> copy = new List<byte[]>(playerStates);
            copy.RemoveAt(i);
            //Send over our the players! 
            users[i].Send("{PlayerStateUpdates}",NetworkModel.SerializeJagged(copy.ToArray()));
        }
    }
    public void SpawnGameObject(byte[] gameObjectData)
    {
        //Request the host to spawn this game object.
        Console.WriteLine("Request to spawn game object: " +gameObjectData.Length );
        users[0].Send("{SpawnGameObject}", gameObjectData );
        
    }
    public void UpdateUser(User user, byte[] newState)
    {
        //This is where you would update the specific user data. For now we just broadcast it to everyone.
        //store our spedcific data to send on next gamestate update.
        for (int i = 0; i < users.Count; i++)
        {
            if (user == users[i])
            {
                playerStates[i] = newState;
            }
        }
    }
}