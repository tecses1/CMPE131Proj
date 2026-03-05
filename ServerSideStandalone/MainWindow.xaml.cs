using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerSideStandalone;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    App app = (App)Application.Current;
    public MainWindow()
    {
        InitializeComponent();
        CompositionTarget.Rendering += Update;
    }

    public void Update(object sender, EventArgs e)
    {
        Title = "Server - Connected Clients: " + app.s.GetClientCount();
        MyStatusText.Text = app.s.GetUserList();
        
        
    }
    private async void SendAlert_Click(object sender, RoutedEventArgs e)
    {
        string message = AlertTextBox.Text;

        // Assuming you have a list of connected clients
        // List<GameServer> connectedClients = ...;

        if (app.s.users.Count == 0)
        {
            MessageBox.Show("No clients connected!");
            return;
        }

        foreach (var client in app.s.users)
        {
            Console.WriteLine($"Sending alert to client {client.uid}: {message}");
            // Use the Send() method we created for Fire-and-Forget
            await client.Send("ServerAlert", message);
            
        }

        AlertTextBox.Clear();
    }
}