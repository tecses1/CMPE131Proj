namespace ServerSideStandalone;
using System.ComponentModel;
using System.Runtime.CompilerServices;
public class ClientNode : INotifyPropertyChanged
{
    private string _username;
    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    private string _ipAddress;
    public string IPAddress
    {
        get => _ipAddress;
        set { _ipAddress = value; OnPropertyChanged(); }
    }

    private string _uid;
    public string uid
    {
        get => _uid;
        set { _uid = value; OnPropertyChanged(); }
    }

    private string _currentPage;
    public string CurrentPage
    {
        get => _currentPage;
        set { _currentPage = value; OnPropertyChanged(); }
    }

    private string _lobby;
    public string Lobby
    {
        get => _lobby;
        set { _lobby = value; OnPropertyChanged(); }
    }
    
    private string _latency;
    public string Latency 
    { 
        get => _latency; 
        set { _latency = value; OnPropertyChanged(); } 
    }
    private string _skipped;
    public string Skipped 
    { 
        get => _skipped; 
        set { _skipped = value; OnPropertyChanged(); } 
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}