using System.Windows;
using FinanceTracker.Resources;

namespace FinanceTracker.Helpers;

public static class ErrorHandler
{
    // Handle exceptions by showing a message to the user and logging details
    public static void Handle(Exception ex, string userMessage)
    {
        // Show error message in a MessageBox
        MessageBox.Show(userMessage + "\n\n" + ex.Message, AppResources.Error_HandlerTitle, MessageBoxButton.OK, MessageBoxImage.Error);

        // Log the generic handler message and full exception details to the console
        Console.WriteLine(AppResources.Error_HandlerMessage);
        Console.WriteLine(ex);

        // Shut down the application to prevent further errors
        Application.Current.Shutdown();
    }
}