using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FinanceTracker.ViewModels;

public class BudgetViewModel : BaseViewModel
{
    private readonly IBudgetService _budgetService;
    private readonly IMessageService _messageService;
    public ICommand SaveBudgetCommand { get; }

    public event Action? BudgetSaved;

    public decimal UsedPercentage => MonthlyLimit == 0 ? 0 : SpentThisMonth / MonthlyLimit * 100;
    public bool HasUnsavedChanges => _monthlyLimit != _originalMonthlyLimit;
    public bool IsOverBudget => MonthlyLimit > 0 && SpentThisMonth > MonthlyLimit;

    private decimal _monthlyLimit;
    private decimal _originalMonthlyLimit;
    public decimal MonthlyLimit
    {
        get => _monthlyLimit;
        set
        {
            _monthlyLimit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasUnsavedChanges));
            OnPropertyChanged(nameof(IsOverBudget));
        }
    }

    private decimal _spentThisMonth;
    public decimal SpentThisMonth
    {
        get => _spentThisMonth;
        private set
        {
            _spentThisMonth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UsedPercentage));
            OnPropertyChanged(nameof(IsOverBudget));
            UpdateProgressBrushColor();
        }
    }

    private decimal _spentLastWeek;
    public decimal SpentLastWeek
    {
        get => _spentLastWeek;
        private set { _spentLastWeek = value; OnPropertyChanged(); }
    }

    private decimal _remainingBudget;
    public decimal RemainingBudget
    {
        get => _remainingBudget;
        private set { _remainingBudget = value; OnPropertyChanged(); }
    }

    private bool _overBudget;
    public bool OverBudget
    {
        get => _overBudget;
        set
        {
            _overBudget = value;
            OnPropertyChanged();
            UpdateProgressBrushColor();
        }
    }

    private LinearGradientBrush _progressBrush;
    public LinearGradientBrush ProgressBrush
    {
        get => _progressBrush;
        private set { _progressBrush = value; OnPropertyChanged(); }
    }

    private readonly DispatcherTimer _animationTimer;
    private double _offset = 0;

    public BudgetViewModel(IBudgetService budgetService, IMessageService messageService)
    {
        _budgetService = budgetService;
        _messageService = messageService;

        LoadBudget();
        SaveBudgetCommand = new RelayCommand(_ => SaveBudget());

        _progressBrush = new LinearGradientBrush
        {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(1, 0),
            GradientStops = new GradientStopCollection
        {
            new GradientStop(Colors.Green, 0),
            new GradientStop(Colors.LightGreen, 0.5),
            new GradientStop(Colors.Green, 1)
        },
            RelativeTransform = new TranslateTransform(_offset, 0)
        };

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        _animationTimer.Tick += (s, e) =>
        {
            _offset += 0.01;
            if (_offset > 1) _offset = -1;
            _progressBrush.RelativeTransform = new TranslateTransform(_offset, 0);
        };
        _animationTimer.Start();
        UpdateProgressBrushColor();
    }

    private void LoadBudget()
    {
        var budget = _budgetService.GetCurrentBudget();
        _originalMonthlyLimit = budget.Limit;
        MonthlyLimit = budget.Limit;
        UpdateSpent();
    }

    public void UpdateSpent()
    {
        SpentThisMonth = _budgetService.GetSpentThisMonth();
        SpentLastWeek = _budgetService.GetSpentLastWeek();
        RemainingBudget = MonthlyLimit - SpentThisMonth;
    }

    private void SaveBudget()
    {
        var now = DateTime.Now;
        var budget = new MonthlyBudget
        {
            Year = now.Year,
            Month = now.Month,
            Limit = MonthlyLimit
        };
        _budgetService.SaveBudget(budget);

        _originalMonthlyLimit = MonthlyLimit;
        UpdateSpent();

        if (SpentThisMonth > MonthlyLimit && MonthlyLimit > 0)
            _messageService.ShowWarning(AppResources.Dialog_BudgetAlert2Message, AppResources.Dialog_BudgetAlert2Title);

        BudgetSaved?.Invoke();
    }

    private void UpdateProgressBrushColor()
    {
        if (_progressBrush == null) return;

        Color color1 = IsOverBudget ? (Color)ColorConverter.ConvertFromString("#E53935") : Colors.Green;
        Color color2 = IsOverBudget ? (Color)ColorConverter.ConvertFromString("#FF8A80") : Colors.LightGreen;

        _progressBrush.GradientStops[0].Color = color1;
        _progressBrush.GradientStops[1].Color = color2;
        _progressBrush.GradientStops[2].Color = color1;
    }

    public void Reload() => LoadBudget();
}