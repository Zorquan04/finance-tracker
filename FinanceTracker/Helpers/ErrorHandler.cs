using System.Windows;
using FinanceTracker.Resources;

namespace FinanceTracker.Helpers;

public static class ErrorHandler
{
    public static void Handle(Exception ex, string userMessage)
    {
        MessageBox.Show(userMessage + "\n\n" + ex.Message, AppResources.Error_HandlerTitle, MessageBoxButton.OK, MessageBoxImage.Error);

        Console.WriteLine(AppResources.Error_HandlerMessage);
        Console.WriteLine(ex);

        Application.Current.Shutdown();
    }
}