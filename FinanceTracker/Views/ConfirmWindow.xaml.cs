using System.Windows;

namespace FinanceTracker.Views;

public partial class ConfirmWindow : Window
{
    public bool Result { get; private set; }

    public ConfirmWindow(string message)
    {
        InitializeComponent();
        DataContext = new { Message = message };
    }

    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        DialogResult = true;
        Close();
    }

    private void No_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        DialogResult = false;
        Close();
    }
}