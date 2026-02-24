using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces; 
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Input;
using System.Windows.Media;

namespace FinanceTracker.ViewModels;

public class ChartViewModel : BaseViewModel
{
    private readonly IChartService _chartService;
    private List<CategoryStats> _stats = new();
    public ICommand ToggleChartCommand { get; }
    public string ToggleChartText => IsTrendMode ? AppResources.Title_ChangeColumn : AppResources.Title_ChangeTrend;

    public SeriesCollection? SeriesCollection { get; set; }
    public bool IsTrendMode { get; set; }

    public string[]? Labels { get; set; }

    public Func<double, string>? YFormatter { get; set; }

    public ChartViewModel(IChartService chartService)
    {
        _chartService = chartService;
        YFormatter = value => $"{value:N2} zł";

        ToggleChartCommand = new RelayCommand(_ => ToggleChartMode());

        LoadChartData();
    }

    private void LoadChartData()
    {
        if (IsTrendMode)
            LoadTrendChart();
        else
            LoadColumnChart();
    }

    private void LoadColumnChart()
    {
        _stats = _chartService.GetExpensesByCategory().ToList();
        Labels = _stats.Select(s => s.Category).ToArray();

        var mapper = LiveCharts.Configurations.Mappers.Xy<CategoryStats>().X((value, index) => index).Y(value => (double)value.Total).Fill(value => GetColorByCategory(value.Category));

        SeriesCollection = new SeriesCollection(mapper)
        {
            new ColumnSeries
            {
                Title = AppResources.Title_Expenses,
                Values = new ChartValues<CategoryStats>(_stats),
                MaxColumnWidth = 60,
                DataLabels = false
            }
        };

        OnPropertyChanged(nameof(SeriesCollection));
        OnPropertyChanged(nameof(Labels));
    }

    private void LoadTrendChart()
    {
        var expenses = _chartService.GetAllExpenses();
        var grouped = expenses.GroupBy(e => new { e.Category?.DisplayName, Month = new DateTime(e.Date.Year, e.Date.Month, 1) }).Select(g => new
            {
                g.Key.DisplayName,
                g.Key.Month,
                Total = g.Sum(e => e.Amount)
            }).ToList();

        var months = grouped.Select(g => g.Month).Distinct().OrderBy(d => d).ToList();

        Labels = months.Select(m => m.ToString("MM/yyyy")).ToArray();

        var categories = grouped.Select(g => g.DisplayName).Distinct();

        SeriesCollection = new SeriesCollection();

        foreach (var category in categories)
        {
            var values = new ChartValues<double>();

            foreach (var month in months)
            {
                var value = grouped.FirstOrDefault(g => g.DisplayName == category && g.Month == month)?.Total ?? 0;

                values.Add((double)value);
            }

            SeriesCollection.Add(new LineSeries
            {
                Title = category,
                Values = values,
                Stroke = GetColorByCategory(category),
                Fill = Brushes.Transparent,
                PointGeometrySize = 8,
                StrokeThickness = 2
            });
        }

        OnPropertyChanged(nameof(SeriesCollection));
        OnPropertyChanged(nameof(Labels));
    }

    public void ToggleChartMode()
    {
        IsTrendMode = !IsTrendMode;
        LoadChartData();
        OnPropertyChanged(nameof(IsTrendMode));
        OnPropertyChanged(nameof(ToggleChartText));
    }

    public Brush GetColorByCategory(string? category)
    {
        return category switch
        {
            "Food" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            "Bills" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            "Entertainment" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),
            "Transport" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            "Other" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
            _ => Brushes.Gray
        };
    }

    public void Refresh() => LoadChartData();
}