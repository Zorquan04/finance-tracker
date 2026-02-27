using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models;

// Represents a single expense entry in the tracker
public class Expense
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int OrderIndex { get; set; }
}