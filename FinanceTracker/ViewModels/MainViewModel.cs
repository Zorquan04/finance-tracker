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
    public BudgetViewModel BudgetVM { get; }

    public MainViewModel()
    {
        ChartsVM = new ChartsViewModel();
        BudgetVM = new BudgetViewModel();
        ExpensesVM = new ExpenseViewModel(ChartsVM, BudgetVM);
        
        ShowExpensesCommand = new RelayCommand(_ => ShowExpensesRequested?.Invoke());
        ShowChartsCommand = new RelayCommand(_ => ShowChartsRequested?.Invoke());
        ShowBudgetCommand = new RelayCommand(_ => ShowBudgetRequested?.Invoke());
    }
}