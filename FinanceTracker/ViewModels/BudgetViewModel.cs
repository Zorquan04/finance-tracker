using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

public class BudgetViewModel : BaseViewModel, IUnsavedChanges
{
    private readonly IBudgetService _budgetService;

    private decimal _monthlyLimit;
    private decimal _originalMonthlyLimit;
    public decimal MonthlyLimit
    {
        get => _monthlyLimit;
        set
        {
            _monthlyLimit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    private decimal _spentThisMonth;
    public decimal SpentThisMonth
    {
        get => _spentThisMonth;
        private set
        {
            _spentThisMonth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UsedPercentage));
        }
    }

    public decimal UsedPercentage => MonthlyLimit == 0 ? 0 : SpentThisMonth / MonthlyLimit * 100;
    public bool HasUnsavedChanges => _monthlyLimit != _originalMonthlyLimit;

    public ICommand SaveBudgetCommand { get; }

    public BudgetViewModel(IBudgetService budgetService)
    {
        _budgetService = budgetService;

        LoadBudget();
        SaveBudgetCommand = new RelayCommand(_ => SaveBudget());
    }

    private void LoadBudget()
    {
        var budget = _budgetService.GetCurrentBudget();
        _originalMonthlyLimit = budget.Limit;
        MonthlyLimit = budget.Limit;
        UpdateSpent();
    }

    public void UpdateSpent()
    {
        SpentThisMonth = _budgetService.GetSpentThisMonth();
    }

    private void SaveBudget()
    {
        var now = DateTime.Now;
        var budget = new MonthlyBudget
        {
            Year = now.Year,
            Month = now.Month,
            Limit = MonthlyLimit
        };
        _budgetService.SaveBudget(budget);

        _originalMonthlyLimit = MonthlyLimit;
        UpdateSpent();
    }

    public void Reload() => LoadBudget();
}