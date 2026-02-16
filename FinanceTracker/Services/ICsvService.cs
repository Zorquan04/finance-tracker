using FinanceTracker.Models;

namespace FinanceTracker.Services;

public interface ICsvService
{
    void Export(string filePath, IEnumerable<Expense> expenses, decimal monhtlyLimit);
    (List<Expense> expenses, decimal monthlyLimit) Import(string filePath);
}