using FinanceTracker.Models;

namespace FinanceTracker.Services.Interfaces;

public interface IBudgetService
{
    MonthlyBudget GetCurrentBudget();
    void SaveBudget(MonthlyBudget budget);
    decimal GetSpentThisMonth();
    void UpdateSpent();
}