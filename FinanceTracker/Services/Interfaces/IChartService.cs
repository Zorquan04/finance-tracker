using FinanceTracker.Models;

namespace FinanceTracker.Services.Interfaces;

public interface IChartService
{
    IEnumerable<ChartData> GetExpensesByCategory();
    IEnumerable<Expense> GetAllExpenses();
}