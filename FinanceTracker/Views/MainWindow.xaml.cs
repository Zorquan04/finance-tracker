using FinanceTracker.Resources;
using FinanceTracker.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FinanceTracker.Views;

// Main window of the application
public partial class MainWindow : Window
{
    private readonly MainViewModel _mainVM;

    // Views for different sections
    private readonly ExpenseView _expenseView;
    private readonly ChartView _chartView;
    private readonly BudgetView _budgetView;

    // Constructor - inject main view model
    public MainWindow(MainViewModel mainVM)
    {
        InitializeComponent();

        _mainVM = mainVM;
        DataContext = _mainVM;  // Set data context for data binding

        // Initialize individual views with their respective view models
        _expenseView = new ExpenseView { DataContext = _mainVM.ExpensesVM };
        _chartView = new ChartView { DataContext = _mainVM.ChartVM };
        _budgetView = new BudgetView { DataContext = _mainVM.BudgetVM };

        // Show default view (Expenses)
        MainContent.Content = _expenseView;

        // Subscribe to view switch events
        _mainVM.ShowExpensesRequested += () => MainContent.Content = _expenseView;
        _mainVM.ShowChartsRequested += () => MainContent.Content = _chartView;
        _mainVM.ShowBudgetRequested += () => MainContent.Content = _budgetView;
    }

    // Handle window closing event
    protected override void OnClosing(CancelEventArgs e)
    {
        FocusManager.SetFocusedElement(this, this);
        base.OnClosing(e);

        // Ask user for confirmation if there are unsaved changes
        if (_mainVM.HasUnsavedChanges)
        {
            if (!_mainVM.MessageService.Confirm(AppResources.Dialog_UnsavedChangesMessage, AppResources.Dialog_UnsavedChangesTitle))
                e.Cancel = true; // Cancel closing if user chooses not to proceed
        }
    }
}