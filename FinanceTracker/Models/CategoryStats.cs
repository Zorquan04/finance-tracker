namespace FinanceTracker.Models;

public class CategoryStats
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Average { get; set; }
    public decimal Max { get; set; }
}