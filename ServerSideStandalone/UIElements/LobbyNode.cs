namespace ServerSideStandalone;
using System.ComponentModel;
using System.Runtime.CompilerServices;
public class LobbyNode : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string tps {get; set;}
    public int PlayerCount { get; set; }
    public string UserList { get; set; }
    public Lobby Lobby;

    private string _uptime;
    public string Uptime
    {
        get => _uptime;
        set { _uptime = value; OnPropertyChanged(); }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}