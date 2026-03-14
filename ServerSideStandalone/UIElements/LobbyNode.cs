namespace ServerSideStandalone;
using System.ComponentModel;
using System.Runtime.CompilerServices;
public class LobbyNode : INotifyPropertyChanged
{
    public string Name { get; set; } // If this never changes after creation, auto-prop is fine
        
        private string _tps;
        public string tps 
        { 
            get => _tps; 
            set { _tps = value; OnPropertyChanged(); } 
        }

        private int _playerCount;
        public int PlayerCount 
        { 
            get => _playerCount; 
            set { _playerCount = value; OnPropertyChanged(); } 
        }

        private string _userList;
        public string UserList 
        { 
            get => _userList; 
            set { _userList = value; OnPropertyChanged(); } 
        }

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