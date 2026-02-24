using FinanceTracker.Models;

namespace FinanceTracker.Services.Interfaces;

public interface IChartService
{
    IEnumerable<CategoryStats> GetExpensesByCategory();
    IEnumerable<Expense> GetAllExpenses();
}