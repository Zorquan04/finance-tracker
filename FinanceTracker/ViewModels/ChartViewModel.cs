using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FinanceTracker.ViewModels;

public class ChartViewModel : BaseViewModel
{
    private readonly IChartService _chartService;
    private List<CategoryStats> _stats = new();

    public string[]? ColumnLabels { get; set; }
    public string[]? TrendLabels { get; set; }

    public bool IsTrendMode { get; set; }
    public ICommand ToggleChartCommand { get; }
    public string ToggleChartText => IsTrendMode ? AppResources.Title_ChangeColumn : AppResources.Title_ChangeTrend;
    public string XAxisTitle => IsTrendMode ? AppResources.Title_Date : AppResources.Title_Category;

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
        if (IsTrendMode)
            LoadTrendChart();
        else
            LoadColumnChart();
    }

    private void LoadColumnChart()
    {
        _stats = _chartService.GetExpensesByCategory().ToList();
        ColumnLabels = _stats.Select(s => s.Category).ToArray();

        var mapper = LiveCharts.Configurations.Mappers.Xy<CategoryStats>()
            .X((value, index) => index)
            .Y(value => (double)value.Total)
            .Fill(value => GetColorByCategory(value.Category));

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

        XFormatter = value =>
        {
            int i = (int)Math.Round(value);
            if (i >= 0 && i < ColumnLabels.Length) return ColumnLabels[i];
            return "";
        };

        OnPropertyChanged(nameof(SeriesCollection));
        OnPropertyChanged(nameof(XFormatter));
        OnPropertyChanged(nameof(XAxisTitle));
    }

    private void LoadTrendChart()
    {
        var expenses = _chartService.GetAllExpenses();

        var grouped = expenses.GroupBy(e => new { e.Category?.DisplayName, Day = e.Date.Date }).Select(g => new { Category = g.Key.DisplayName, Day = g.Key.Day, Total = g.Sum(e => e.Amount), Count = g.Count() }).OrderBy(g => g.Day).ToList(); var categories = grouped.Select(g => g.Category).Distinct(); var allDates = grouped.Select(g => g.Day).Distinct().OrderBy(d => d).ToList(); TrendLabels = allDates.Select(d => d.ToString("dd.MM")).ToArray(); SeriesCollection = new SeriesCollection(); foreach (var category in categories) { var values = new ChartValues<CategoryDay>(); foreach (var date in allDates) { var item = grouped.FirstOrDefault(g => g.Category == category && g.Day == date); values.Add(new CategoryDay { Date = date, Total = item?.Total ?? 0, Count = item?.Count ?? 0 }); } var mapper = LiveCharts.Configurations.Mappers.Xy<CategoryDay>().X(cd => cd.Date.ToOADate()).Y(cd => (double)cd.Total); SeriesCollection.Add(new LineSeries(mapper) { Title = category, Values = values, Stroke = GetColorByCategory(category), Fill = Brushes.Transparent, PointGeometrySize = 6, StrokeThickness = 2 }); }
        XFormatter = value => DateTime.FromOADate(value).ToString("dd.MM"); OnPropertyChanged(nameof(SeriesCollection)); OnPropertyChanged(nameof(XFormatter)); OnPropertyChanged(nameof(XAxisTitle));
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

    public void Refresh() => LoadChartData();
}