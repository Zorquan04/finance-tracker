using FinanceTracker.Data;
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

    public IEnumerable<(string Category, decimal Total)> GetExpensesByCategory()
    {
        var expenses = _context.Expenses.Include(e => e.Category).AsNoTracking().ToList();
        return expenses.GroupBy(e => e.Category!.DisplayName!).Select(g => (Category: g.Key, Total: g.Sum(e => e.Amount))).ToList();
    }
}