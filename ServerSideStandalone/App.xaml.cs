using System.Configuration;
using System.Data;
using System.Windows;

namespace ServerSideStandalone;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static MainWindow? mainWindow => Current.MainWindow as MainWindow;
    public Server s = new Server();

}

