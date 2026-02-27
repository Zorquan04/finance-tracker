using FinanceTracker.Services.Interfaces;
using FinanceTracker.Views;
using System.Windows;

namespace FinanceTracker.Services;

// Service responsible for showing messages, warnings, and confirmation dialogs
public class MessageService : IMessageService
{
    // Show an informational message box
    public void ShowInfo(string message, string title = "Info")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Show a warning message box
    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // Show a confirmation dialog with custom message and title
    // Returns true if the user confirms, false otherwise
    public bool Confirm(string message, string title = "Confirm")
    {
        var confirmWindow = new ConfirmWindow(message)
        {
            Title = title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow
        };

        return confirmWindow.ShowDialog() == true;
    }
}