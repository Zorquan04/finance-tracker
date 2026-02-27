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
    private List<ChartData> _stats = new();

    public string[] Labels => IsTrendMode ? TrendLabels ?? Array.Empty<string>() : ColumnLabels ?? Array.Empty<string>();
    public string[]? ColumnLabels { get; set; }
    public string[]? TrendLabels { get; set; }

    public bool IsTrendMode { get; set; }
    public ICommand ToggleChartCommand { get; }
    public string ToggleChartText => IsTrendMode ? AppResources.Title_ChangeColumn : AppResources.Title_ChangeTrend;
    public string XAxisTitle => IsTrendMode ? AppResources.Title_Date : AppResources.Title_Category;
    public double AxisXMax { get; set; }

    public SeriesCollection? SeriesCollection { get; set; }
    public Func<double, string>? YFormatter { get; set; }
    public Func<double, string>? XFormatter { get; set; }

    public ChartViewModel(IChartService chartService)
    {
        _chartService = chartService;
        YFormatter = value => $"{value:N2} zł";
        ToggleChartCommand = new RelayCommand(_ => ToggleChartMode());
        LoadChartData();
    }

    private void LoadChartData()
    {
        try
        {
            if (IsTrendMode)
                LoadTrendChart();
            else
                LoadColumnChart();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadChartData);
        }
    }

    private void LoadColumnChart()
    {
        try
        {
            _stats = _chartService.GetExpensesByCategory().ToList();
            ColumnLabels = _stats.Select(s => s.Category).ToArray();

            var dataPoints = _stats.Select(s => new ChartData
            {
                Category = s.Category,
                Total = s.Total,
                Average = s.Average,
                Max = s.Max,
                Count = 0,
                Date = null
            }).ToList();

            var mapper = LiveCharts.Configurations.Mappers.Xy<ChartData>().X((value, index) => index).Y(value => (double)value.Total).Fill(value => GetColorByCategory(value.Category));

            SeriesCollection = new SeriesCollection(mapper)
            {
                new ColumnSeries
                {
                    Title = AppResources.Title_Expenses,
                    Values = new ChartValues<ChartData>(dataPoints),
                    MaxColumnWidth = 60,
                    DataLabels = false
                }
            };

            AxisXMax = double.NaN;

            XFormatter = value =>
            {
                int i = (int)Math.Round(value);
                if (i >= 0 && i < ColumnLabels.Length)
                    return ColumnLabels[i];
                return "";
            };

            OnPropertyChanged(nameof(SeriesCollection));
            OnPropertyChanged(nameof(XFormatter));
            OnPropertyChanged(nameof(XAxisTitle));
            OnPropertyChanged(nameof(Labels));
            OnPropertyChanged(nameof(AxisXMax));
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadColumnChart);
        }
    }

    private void LoadTrendChart()
    {
        try
        {
            var expenses = _chartService.GetAllExpenses();

            var grouped = expenses.GroupBy(e => new { e.Category?.DisplayName, Day = e.Date.Date })
                .Select(g => new
                {
                    Category = g.Key.DisplayName!,
                    Day = g.Key.Day,
                    Total = g.Sum(e => e.Amount),
                    Count = g.Count()
                }).OrderBy(g => g.Day).ToList();

            if (!grouped.Any())
            {
                SeriesCollection = new SeriesCollection();
                TrendLabels = Array.Empty<string>();
                return;
            }

            var categories = grouped.Select(g => g.Category).Distinct().ToList();

            var allExpenseDates = grouped.Select(g => g.Day).ToList();
            var minDate = new DateTime(allExpenseDates.Min().Year, allExpenseDates.Min().Month, 1);
            var maxDate = new DateTime(allExpenseDates.Max().Year, allExpenseDates.Max().Month, DateTime.DaysInMonth(allExpenseDates.Max().Year, allExpenseDates.Max().Month));

            var allDates = Enumerable.Range(0, (maxDate - minDate).Days + 1).Select(offset => minDate.AddDays(offset)).ToList();

            TrendLabels = allDates.Select(d => d.ToString("dd.MM")).ToArray();
            SeriesCollection = new SeriesCollection();
            AxisXMax = allDates.Count - 1;

            foreach (var category in categories)
            {
                var values = new ChartValues<ChartData>
            {
                new ChartData
                {
                    Category = category,
                    Date = minDate,
                    Total = 0,
                    Count = 0
                }
            };

                var categoryItems = grouped.Where(g => g.Category == category).OrderBy(g => g.Day).ToList();

                foreach (var item in categoryItems)
                {
                    values.Add(new ChartData
                    {
                        Category = category,
                        Date = item.Day,
                        Total = item.Total,
                        Count = item.Count
                    });
                }

                var mapper = LiveCharts.Configurations.Mappers.Xy<ChartData>().X((cd, index) => allDates.IndexOf(cd.Date!.Value)).Y(cd => (double)cd.Total);

                SeriesCollection.Add(new LineSeries(mapper)
                {
                    Title = category,
                    Values = values,
                    Stroke = GetColorByCategory(category),
                    Fill = Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 6,
                    StrokeThickness = 2,
                    LineSmoothness = 1
                });
            }

            XFormatter = value =>
            {
                int i = (int)Math.Round(value);
                if (i >= 0 && i < allDates.Count)
                    return allDates[i].ToString("dd.MM");
                return "";
            };

            OnPropertyChanged(nameof(SeriesCollection));
            OnPropertyChanged(nameof(XFormatter));
            OnPropertyChanged(nameof(XAxisTitle));
            OnPropertyChanged(nameof(AxisXMax));
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadTrendChart);
        }
    }

    public void ToggleChartMode()
    {
        try
        {
            IsTrendMode = !IsTrendMode;
            LoadChartData();
            OnPropertyChanged(nameof(IsTrendMode));
            OnPropertyChanged(nameof(ToggleChartText));
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Toggle);
        }
    }

    public Brush GetColorByCategory(string? category)
    {
        if (category == AppResources.Category_Food)
            return new SolidColorBrush(Color.FromRgb(76, 175, 80));
        if (category == AppResources.Category_Bills)
            return new SolidColorBrush(Color.FromRgb(244, 67, 54));
        if (category == AppResources.Category_Entertainment)
            return new SolidColorBrush(Color.FromRgb(156, 39, 176));
        if (category == AppResources.Category_Transport)
            return new SolidColorBrush(Color.FromRgb(33, 150, 243));
        if (category == AppResources.Category_Other)
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        return Brushes.Gray;
    }

    public void Refresh()
    {
        try
        {
            LoadChartData();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Refresh);
        }
    }
}