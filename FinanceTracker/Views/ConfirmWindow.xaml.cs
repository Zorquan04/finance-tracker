using System.Windows;

namespace FinanceTracker.Views;

// Simple confirmation dialog window
public partial class ConfirmWindow : Window
{
    // Stores the result of the confirmation (true = yes, false = no)
    public bool Result { get; private set; }

    // Constructor initializes window and sets message
    public ConfirmWindow(string message)
    {
        InitializeComponent();
        DataContext = new { Message = message }; // Bind message to the UI
    }

    // "Yes" button clicked
    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        DialogResult = true; // Close the window with positive result
        Close();
    }

    // "No" button clicked
    private void No_Click(object sender, RoutedEventArgs e)
    {
        Result = false;  
        DialogResult = false; // Close the window with negative result
        Close();
    }
}