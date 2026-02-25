using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Services;

public class ChartService : IChartService
{
    private readonly FinanceDbContext _context;

    public ChartService(FinanceDbContext context)
    {
        _context = context;
    }

    public IEnumerable<ChartData> GetExpensesByCategory()
    {
        var expenses = _context.Expenses.Include(e => e.Category).AsNoTracking().ToList();

        return expenses.GroupBy(e => e.Category!.DisplayName!).Select(g => new ChartData
        {
            Category = g.Key,
            Total = g.Sum(e => e.Amount),
            Average = g.Average(e => e.Amount),
            Max = g.Max(e => e.Amount)
        }).ToList();
    }

    public IEnumerable<Expense> GetAllExpenses()
    {
        return _context.Expenses.Include(e => e.Category).AsNoTracking().ToList();
    }
}