using FinanceTracker.Models;

namespace FinanceTracker.Services.Interfaces;

public interface IExpenseService
{
    List<Expense> GetAllExpenses();
    List<Category> GetAllCategories();
    void AddExpense(Expense expense);
    void UpdateExpense(Expense expense);
    void DeleteExpense(int expenseId);
    void UpdateOrder(List<Expense> expenses);
    void SwapOrder(int id1, int id2);
    decimal GetTotalExpenses(Func<Expense, bool>? filter = null);
    void ClearAllExpenses();
}