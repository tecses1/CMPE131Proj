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

        private int _contaiminationTime;
        public int ContaminationTime
        {
            get => _contaiminationTime;
            set { _contaiminationTime = value; OnPropertyChanged(); }
        }

        private int _extraTime;
        public int ExtraTime
        {
            get => _extraTime;
            set { _extraTime = value; OnPropertyChanged(); }
        }

        private int _updateTime;
        public int UpdateTime
        {
            get => _updateTime;
            set { _updateTime = value; OnPropertyChanged(); }
        }

        private int _userTime;
        public int UserTime
        {
            get => _userTime;
            set { _userTime = value; OnPropertyChanged(); }
        }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}