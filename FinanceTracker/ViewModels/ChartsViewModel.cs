using FinanceTracker.Data;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.ViewModels;

public class ChartsViewModel : BaseViewModel
{
    private readonly FinanceDbContext _context;

    public SeriesCollection? SeriesCollection { get; set; }
    public string[]? Labels { get; set; }

    public Func<double, string>? YFormatter { get; set; }

    public ChartsViewModel()
    {
        _context = new FinanceDbContext();
        YFormatter = value => $"{value:N2} zł";

        LoadChartData();
    }

    private void LoadChartData()
    {
        var expenses = _context.Expenses.AsNoTracking().Include(e => e.Category).ToList();
        var expensesByCategory = expenses.GroupBy(e => e.Category!.Name).Select(g => new {Category = g.Key, Total = g.Sum(e => e.Amount)}).ToList();

        Labels = expensesByCategory.Select(e => e.Category!).ToArray();

        SeriesCollection = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Expenses",
                Values = new ChartValues<decimal>(expensesByCategory.Select(e => e.Total))
            }
        };

        OnPropertyChanged(nameof(SeriesCollection));
        OnPropertyChanged(nameof(Labels));
    }

    public void Refresh()
    {
        LoadChartData();
    }
}