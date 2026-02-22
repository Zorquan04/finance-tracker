using FinanceTracker.Services.Interfaces;
using FinanceTracker.Resources;
using LiveCharts;
using LiveCharts.Wpf;

namespace FinanceTracker.ViewModels;

public class ChartViewModel : BaseViewModel
{
    private readonly IChartService _chartService;

    public SeriesCollection? SeriesCollection { get; set; }
    public string[]? Labels { get; set; }

    public Func<double, string>? YFormatter { get; set; }

    public ChartViewModel(IChartService chartService)
    {
        _chartService = chartService;
        YFormatter = value => $"{value:N2} zł";

        LoadChartData();
    }

    private void LoadChartData()
    {
        var data = _chartService.GetExpensesByCategory().ToList();

        Labels = data.Select(d => d.Category).ToArray();

        SeriesCollection = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = AppResources.Title_Expenses,
                Values = new ChartValues<decimal>(data.Select(d => d.Total))
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