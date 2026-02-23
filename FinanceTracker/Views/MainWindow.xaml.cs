using FinanceTracker.Resources;
using FinanceTracker.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FinanceTracker.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainVM;

    private readonly ExpenseView _expenseView;
    private readonly ChartView _chartView;
    private readonly BudgetView _budgetView;

    public MainWindow(MainViewModel mainVM)
    {
        InitializeComponent();

        _mainVM = mainVM;
        DataContext = _mainVM;

        _expenseView = new ExpenseView { DataContext = _mainVM.ExpensesVM };
        _chartView = new ChartView { DataContext = _mainVM.ChartVM };
        _budgetView = new BudgetView { DataContext = _mainVM.BudgetVM };

        MainContent.Content = _expenseView;

        _mainVM.ShowExpensesRequested += () => MainContent.Content = _expenseView;
        _mainVM.ShowChartsRequested += () => MainContent.Content = _chartView;
        _mainVM.ShowBudgetRequested += () => MainContent.Content = _budgetView;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        FocusManager.SetFocusedElement(this, this);
        base.OnClosing(e);

        if (_mainVM.HasUnsavedChanges)
        {
            if (!_mainVM.MessageService.Confirm(AppResources.Dialog_UnsavedChangesMessage, AppResources.Dialog_UnsavedChangesTitle))
                e.Cancel = true;
        }
    }
}