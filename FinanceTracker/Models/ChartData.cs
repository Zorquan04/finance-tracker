namespace FinanceTracker.Models;

public class ChartData
{
    public decimal Total { get; set; }

    public string Category { get; set; } = string.Empty;
    public decimal Average { get; set; }
    public decimal Max { get; set; }

    public DateTime? Date { get; set; }
    public int Count { get; set; }
}