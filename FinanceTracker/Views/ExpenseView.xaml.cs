using FinanceTracker.ViewModels;
using System.Windows.Controls;

namespace FinanceTracker.Views;

public partial class ExpenseView : UserControl
{
    public ExpenseViewModel ViewModel
    {
        get => (ExpenseViewModel)DataContext;
        set => DataContext = value;
    }

    public ExpenseView()
    {
        InitializeComponent();
    }
}