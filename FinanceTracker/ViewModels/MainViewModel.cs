using FinanceTracker.Helpers;

namespace FinanceTracker.ViewModels;

public class MainViewModel : BaseViewModel
{
    public event Action? ShowExpensesRequested;
    public event Action? ShowChartsRequested;
    public event Action? ShowBudgetRequested;

    public RelayCommand ShowExpensesCommand { get; }
    public RelayCommand ShowChartsCommand { get; }
    public RelayCommand ShowBudgetCommand { get; }

    public ChartsViewModel ChartsVM { get; }
    public ExpenseViewModel ExpensesVM { get; }

    public MainViewModel()
    {
        ChartsVM = new ChartsViewModel();
        ExpensesVM = new ExpenseViewModel(ChartsVM);
        var budgetVM = new BudgetViewModel();

        ShowExpensesCommand = new RelayCommand(_ => ShowExpensesRequested?.Invoke());
        ShowChartsCommand = new RelayCommand(_ => ShowChartsRequested?.Invoke());
        ShowBudgetCommand = new RelayCommand(_ => ShowBudgetRequested?.Invoke());
    }
}