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
    //Called when host sends over their gamestate.
    public void UpdateState(byte[] newState)
    {
        //Send state to all users NOT host.
        for (int i = 1; i < users.Count; i++)
        {
            users[i].Send("{GameStateUpdate}", newState);
        }

        //Send the next inputs to everyone.
        for (int i = 0; i < users.Count; i++)
        {
            if (playerStates[i] != null)
            {
                users[i].Send("{InputAll}", NetworkModel.SerializeJagged(playerStates));
            }
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

    public void AddInput(User user, byte[] inputData)
    {
        for (int i = 0; i < users.Count; i++)
        {
            if (user == users[i])
            {
                playerStates[i] = inputData;
            }
        }
        //send the input to the host to handle. 
        //users[0].Send("{Input}", inputData);
    }
}