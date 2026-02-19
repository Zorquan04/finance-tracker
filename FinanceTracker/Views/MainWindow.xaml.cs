using FinanceTracker.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace FinanceTracker.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainVM;

    public MainWindow()
    {
        InitializeComponent();

        _mainVM = new MainViewModel();
        DataContext = _mainVM;

        var expenseView = new ExpenseView { ViewModel = _mainVM.ExpensesVM };
        MainContent.Content = expenseView;

        _mainVM.ShowExpensesRequested += () => MainContent.Content = new ExpenseView { ViewModel = _mainVM.ExpensesVM };
        _mainVM.ShowChartsRequested += () => MainContent.Content = new ChartsView { ViewModel = _mainVM.ChartsVM };
        _mainVM.ShowBudgetRequested += () => MainContent.Content = new BudgetView();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is MainViewModel vm)
        {
            if (vm.ExpensesVM.HasUnsavedChanges)
            {
                var confirm = new ConfirmWindow("You have unsaved changes. Exit anyway?");
                confirm.Owner = this;

                bool? result = confirm.ShowDialog();

                if (result != true)
                    e.Cancel = true;
            }
        }
    }
}