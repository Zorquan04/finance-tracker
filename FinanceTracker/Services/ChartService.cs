using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Services;

// Service for providing data for charts
public class ChartService : IChartService
{
    private readonly FinanceDbContext _context;

    public ChartService(FinanceDbContext context)
    {
        _context = context;
    }

    // Get aggregated expense data grouped by category
    public IEnumerable<ChartData> GetExpensesByCategory()
    {
        // Load all expenses including their category, no tracking needed for read-only
        var expenses = _context.Expenses.Include(e => e.Category).AsNoTracking().ToList();

        // Group expenses by category and calculate total, average, and max
        return expenses.GroupBy(e => e.Category!.DisplayName!).Select(g => new ChartData
        {
            Category = g.Key,
            Total = g.Sum(e => e.Amount),
            Average = g.Average(e => e.Amount),
            Max = g.Max(e => e.Amount)
        }).ToList();
    }

    // Return all expenses with their categories, for trend charts
    public IEnumerable<Expense> GetAllExpenses()
    {
        return _context.Expenses.Include(e => e.Category).AsNoTracking().ToList();
    }
}