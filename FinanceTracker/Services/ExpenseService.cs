using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Services;

// Service responsible for handling all operations related to expenses and categories
public class ExpenseService : IExpenseService
{
    private readonly FinanceDbContext _context;

    public ExpenseService(FinanceDbContext context)
    {
        _context = context;
    }

    // Retrieve all expenses including their category, ordered by custom OrderIndex
    public List<Expense> GetAllExpenses()
    {
        return _context.Expenses.Include(e => e.Category).OrderBy(e => e.OrderIndex).ToList();
    }

    // Retrieve all categories from the database
    public List<Category> GetAllCategories()
    {
        return _context.Categories.ToList();
    }

    // Add a new expense and assign it the next available OrderIndex
    public void AddExpense(Expense expense)
    {
        var maxIndex = _context.Expenses.Any() ? _context.Expenses.Max(e => e.OrderIndex) + 1 : 0;
        expense.OrderIndex = maxIndex;
        _context.Expenses.Add(expense);
        _context.SaveChanges();
    }

    // Update an existing expense by ID
    public void UpdateExpense(Expense expense)
    {
        var e = _context.Expenses.FirstOrDefault(x => x.Id == expense.Id);
        if (e == null) return;

        e.Name = expense.Name;
        e.Amount = expense.Amount;
        e.Date = expense.Date;
        e.CategoryId = expense.CategoryId;

        _context.SaveChanges();
    }

    // Delete an expense by ID
    public void DeleteExpense(int expenseId)
    {
        var e = _context.Expenses.FirstOrDefault(x => x.Id == expenseId);
        if (e == null) return;

        _context.Expenses.Remove(e);
        _context.SaveChanges();
    }

    // Update the order of all expenses based on the list provided
    public void UpdateOrder(List<Expense> expenses)
    {
        for (int i = 0; i < expenses.Count; i++)
        {
            var expense = _context.Expenses.First(e => e.Id == expenses[i].Id);
            expense.OrderIndex = i;
        }
        _context.SaveChanges();
    }

    // Swap the order of two expenses by their IDs
    public void SwapOrder(int id1, int id2)
    {
        var e1 = _context.Expenses.First(x => x.Id == id1);
        var e2 = _context.Expenses.First(x => x.Id == id2);

        var temp = e1.OrderIndex;
        e1.OrderIndex = e2.OrderIndex;
        e2.OrderIndex = temp;

        _context.SaveChanges();
    }

    // Calculate total expenses, optionally filtered by a predicate
    public decimal GetTotalExpenses(Func<Expense, bool>? filter = null)
    {
        var data = _context.Expenses.Include(e => e.Category).AsEnumerable();
        if (filter != null)
            data = data.Where(filter);

        return data.Sum(e => e.Amount);
    }

    // Remove all expenses and all monthly budgets from the database
    public void ClearAllExpenses()
    {
        _context.Expenses.RemoveRange(_context.Expenses);
        _context.MonthlyBudgets.RemoveRange(_context.MonthlyBudgets);
        _context.SaveChanges();
    }
}