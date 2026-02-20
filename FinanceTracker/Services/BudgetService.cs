using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Services;
public class BudgetService : IBudgetService
{
    private readonly FinanceDbContext _context;

    public BudgetService(FinanceDbContext context)
    {
        _context = context;
    }

    public MonthlyBudget GetCurrentBudget()
    {
        var now = DateTime.Now;
        var budget = _context.MonthlyBudgets.AsNoTracking().FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month);

        return budget ?? new MonthlyBudget { Year = now.Year, Month = now.Month, Limit = 0 };
    }

    public void SaveBudget(MonthlyBudget budget)
    {
        var existing = _context.MonthlyBudgets.FirstOrDefault(b => b.Year == budget.Year && b.Month == budget.Month);
        if (existing != null)
            existing.Limit = budget.Limit;
        else
            _context.MonthlyBudgets.Add(budget);

        _context.SaveChanges();
    }

    public decimal GetSpentThisMonth()
    {
        var now = DateTime.Now;
        return _context.Expenses.Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month).AsEnumerable().Sum(e => e.Amount);
    }

    public decimal GetSpentLastWeek()
    {
        var now = DateTime.Now;
        var lastWeekStart = now.AddDays(-7);
        return _context.Expenses.Where(e => e.Date >= lastWeekStart && e.Date <= now).AsEnumerable().Sum(e => e.Amount);
    }
}