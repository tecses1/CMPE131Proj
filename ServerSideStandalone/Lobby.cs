namespace ServerSideStandalone;
public class Lobby
{
    public string Name;
    public string State;

    List<User> users = new List<User>();

    public void AddUser(User user)
    {
        users.Add(user);
    }
    public bool isHost(User user)
    {
        return users[0] == user;
    }

    public void UpdateState(string newState)
    {
        State = newState;
        foreach (User u in users)
        {
            u.Send("{GameStateUpdate}", new string[] { State });
        }
    }
    public void SpawnGameObject(string gameObjectData)
    {
        //Request the host to spawn this game object.
        Console.WriteLine("Request to spawn game object: " +gameObjectData );
        users[0].Send("{SpawnGameObject}", new string[] { gameObjectData });
        
    }
    public void UpdateUser(User user, string newState)
    {
        //This is where you would update the specific user data. For now we just broadcast it to everyone.
        foreach (User u in users)
        {
            if (u == user) continue; // No need to send the update back to the user that sent it.
            u.Send("{PlayerUpdate}", new string[] { newState });
        }
    }
}