namespace ServerSideStandalone;
using System.ComponentModel;
using System.Runtime.CompilerServices;
public class ClientNode : INotifyPropertyChanged
{
    public string Username { get; set; }
    public string IPAddress { get; set; }
    
    private string _latency;
    public string Latency 
    { 
        get => _latency; 
        set { _latency = value; OnPropertyChanged(); } 
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}