using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

public class BudgetViewModel : BaseViewModel
{
    private readonly IBudgetService _budgetService;
    private readonly IMessageService _messageService;   
    public ICommand SaveBudgetCommand { get; }

    public event Action? BudgetSaved;

    public decimal UsedPercentage => MonthlyLimit == 0 ? 0 : SpentThisMonth / MonthlyLimit * 100;
    public bool HasUnsavedChanges => _monthlyLimit != _originalMonthlyLimit;
    public bool IsOverBudget => MonthlyLimit > 0 && SpentThisMonth > MonthlyLimit;

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
            OnPropertyChanged(nameof(IsOverBudget));
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
            OnPropertyChanged(nameof(IsOverBudget));
        }
    }

    private decimal _spentLastWeek;
    public decimal SpentLastWeek
    {
        get => _spentLastWeek;
        private set { _spentLastWeek = value; OnPropertyChanged(); }
    }

    private decimal _remainingBudget;
    public decimal RemainingBudget
    {
        get => _remainingBudget;
        private set { _remainingBudget = value; OnPropertyChanged(); }
    }

    public BudgetViewModel(IBudgetService budgetService, IMessageService messageService)
    {
        _budgetService = budgetService;
        _messageService = messageService;

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
        SpentLastWeek = _budgetService.GetSpentLastWeek();
        RemainingBudget = MonthlyLimit - SpentThisMonth;
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

        if (SpentThisMonth > MonthlyLimit && MonthlyLimit > 0)
            _messageService.ShowWarning("Current expenses exceed the new budget limit.", "Budget Alert");

        BudgetSaved?.Invoke();
    }

    public void Reload() => LoadBudget();
}