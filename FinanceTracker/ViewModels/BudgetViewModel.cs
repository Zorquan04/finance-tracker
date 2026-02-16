using FinanceTracker.Data;
using FinanceTracker.Helpers;
using FinanceTracker.Models;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

public class BudgetViewModel : BaseViewModel
{
    private readonly FinanceDbContext _context;

    private decimal _monthlyLimit;
    public decimal MonthlyLimit
    {
        get => _monthlyLimit;
        set { _monthlyLimit = value; OnPropertyChanged(nameof(MonthlyLimit)); }
    }

    private decimal _spentThisMonth;
    public decimal SpentThisMonth
    {
        get => _spentThisMonth;
        set { _spentThisMonth = value; OnPropertyChanged(nameof(SpentThisMonth)); OnPropertyChanged(nameof(UsedPercentage)); }
    }

    public decimal UsedPercentage => MonthlyLimit == 0 ? 0 : SpentThisMonth / MonthlyLimit * 100;

    public ICommand SaveBudgetCommand { get; }

    public BudgetViewModel()
    {
        _context = new FinanceDbContext();

        LoadBudget();
        SaveBudgetCommand = new RelayCommand(_ => SaveBudget());
    }

    private void LoadBudget()
    {
        var now = DateTime.Now;
        var budget = _context.MonthlyBudgets.FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month);

        MonthlyLimit = budget?.Limit ?? 0;
        UpdateSpent();
    }

    public void UpdateSpent()
    {
        var now = DateTime.Now;
        SpentThisMonth = _context.Expenses.Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month).AsEnumerable().Sum(e => e.Amount);
    }

    private void SaveBudget()
    {
        var now = DateTime.Now;
        var budget = _context.MonthlyBudgets.FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month);
        if (budget != null)
            budget.Limit = MonthlyLimit;
        else
        {
            _context.MonthlyBudgets.Add(new MonthlyBudget
            {
                Year = now.Year,
                Month = now.Month,
                Limit = MonthlyLimit
            });
        }
        _context.SaveChanges();
        UpdateSpent();
    }

    public void Reload()
    {
        LoadBudget();
    }
}