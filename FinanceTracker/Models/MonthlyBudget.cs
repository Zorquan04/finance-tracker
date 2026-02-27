using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models;

// Represents the budget set for a specific month and year
public class MonthlyBudget
{
    [Key]
    public int Id { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public decimal Limit { get; set; }
}