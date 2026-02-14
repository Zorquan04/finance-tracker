using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models;

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
}