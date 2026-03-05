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
}