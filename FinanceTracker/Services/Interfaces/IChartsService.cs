namespace FinanceTracker.Services.Interfaces;

public interface IChartService
{
    IEnumerable<(string Category, decimal Total)> GetExpensesByCategory();
}