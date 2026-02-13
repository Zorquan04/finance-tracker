using System.Windows;
using FinanceTracker.ViewModels;
using FinanceTracker.Views;

namespace FinanceTracker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel()
        };

        mainWindow.Show();
    }
}