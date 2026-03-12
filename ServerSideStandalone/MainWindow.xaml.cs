
namespace ServerSideStandalone;
using System.Windows;
using System.Collections.ObjectModel;
using System;
using System.Windows.Controls;
public partial class MainWindow : Window
{
    App app = (App)Application.Current;
    public ObservableCollection<ClientNode> Clients { get; set; } = new ObservableCollection<ClientNode>();
    public ObservableCollection<LobbyNode> Lobbies { get; set; } = new ObservableCollection<LobbyNode>();
    public MainWindow()
    {
        InitializeComponent();
        ClientList.ItemsSource = Clients;
        LobbyList.ItemsSource = Lobbies;
        app.s.mainWindow = this; // Let the server access the GUI for updates
        LogToTerminal("Server GUI initialized and ready.");
    }

    public void Update(object sender, EventArgs e)
    {
        Title = "Server - Connected Clients: " + app.s.GetClientCount();
        
        
    }
    // Logic for the "Close Lobby" button inside the Expandable
    private void OnCloseLobbyClick(object sender, RoutedEventArgs e)
    {
        // Find WHICH lobby the button belonged to
        if (sender is Button btn && btn.DataContext is LobbyNode lobbyToClose)
        {
            LogToTerminal($"Closing Lobby: {lobbyToClose.Name}");
            // Your server logic to kick players and remove lobby here:
            app.s.CloseLobby(lobbyToClose.Lobby);
            Lobbies.Remove(lobbyToClose);
            return;
        }
        
    }
    public void LogToTerminal(string message)
    {
            // Since the server thread is likely different from the UI thread
        Dispatcher.Invoke(() =>
        {
            TerminalOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            TerminalOutput.ScrollToEnd();
        });
    }

    private void OnCommandSubmit(object sender, RoutedEventArgs e)
    {
        string cmd = TerminalInput.Text;
        if (string.IsNullOrWhiteSpace(cmd)) return;

        LogToTerminal($"> {cmd}");
        ProcessCommand(cmd);
        TerminalInput.Clear();
    }

    private void OnInputKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter) OnCommandSubmit(null, null);
    }

    private void ProcessCommand(string cmd)
    {
        // Very basic command processing. You can expand this as needed.
        string[] parts = cmd.Split(' ');
        LogToTerminal(app.s.Command(parts));

    }
}