using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FinanceTracker.ViewModels;

// ViewModel for managing the monthly budget, spent amounts, and progress bar
public class BudgetViewModel : BaseViewModel
{
    private readonly IBudgetService _budgetService;
    private readonly IMessageService _messageService;

    // Command to save the monthly budget
    public ICommand SaveBudgetCommand { get; }

    // Event triggered after successfully saving the budget
    public event Action? BudgetSaved;

    // Backing fields
    private decimal _monthlyLimit;
    private decimal _originalMonthlyLimit;
    private decimal _spentThisMonth;
    private decimal _spentLastWeek;
    private decimal _remainingBudget;
    private bool _overBudget;
    private LinearGradientBrush _progressBrush;
    private readonly DispatcherTimer _animationTimer;
    private double _offset = 0;

    // Monthly budget limit set by the user
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

    // Total spent this month
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

    // Total spent in the last 7 days
    public decimal SpentLastWeek
    {
        get => _spentLastWeek;
        private set { _spentLastWeek = value; OnPropertyChanged(); }
    }

    // Remaining budget for the current month
    public decimal RemainingBudget
    {
        get => _remainingBudget;
        private set { _remainingBudget = value; OnPropertyChanged(); }
    }

    // Flag indicating if the user is over the budget
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

    // Percentage of budget used
    public decimal UsedPercentage => MonthlyLimit == 0 ? 0 : SpentThisMonth / MonthlyLimit * 100;

    // True if the monthly limit has been modified but not saved
    public bool HasUnsavedChanges => _monthlyLimit != _originalMonthlyLimit;

    // True if spent this month exceeds the monthly limit
    public bool IsOverBudget => MonthlyLimit > 0 && SpentThisMonth > MonthlyLimit;

    // Gradient brush for progress bar, changes color based on budget usage
    public LinearGradientBrush ProgressBrush
    {
        get => _progressBrush;
        private set { _progressBrush = value; OnPropertyChanged(); }
    }

    // Constructor initializes services, commands, and animated progress brush
    public BudgetViewModel(IBudgetService budgetService, IMessageService messageService)
    {
        _budgetService = budgetService;
        _messageService = messageService;

        // Load initial budget values
        LoadBudget();

        // Initialize save command
        SaveBudgetCommand = new RelayCommand(_ => SaveBudget());

        // Initialize gradient brush for progress bar
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

        // Timer for animating the gradient movement
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _animationTimer.Tick += (s, e) =>
        {
            _offset += 0.01;
            if (_offset > 1) 
                _offset = -1;
            _progressBrush.RelativeTransform = new TranslateTransform(_offset, 0);
        };
        _animationTimer.Start();

        UpdateProgressBrushColor();
    }

    // Loads current budget from the service
    private void LoadBudget()
    {
        try
        {
            var budget = _budgetService.GetCurrentBudget();
            _originalMonthlyLimit = budget.Limit;
            MonthlyLimit = budget.Limit;
            UpdateSpent();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadBudget);
        }
    }

    // Updates spent values and remaining budget from the service
    public void UpdateSpent()
    {
        try
        {
            SpentThisMonth = _budgetService.GetSpentThisMonth();
            SpentLastWeek = _budgetService.GetSpentLastWeek();
            RemainingBudget = MonthlyLimit - SpentThisMonth;
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_UpdateSpent);
        }
    }

    // Saves the current monthly budget to the service
    private void SaveBudget()
    {
        try
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

            // Alert if budget exceeded
            if (SpentThisMonth > MonthlyLimit && MonthlyLimit > 0)
                _messageService.ShowWarning(AppResources.Dialog_BudgetAlert2Message, AppResources.Dialog_BudgetAlert2Title);

            BudgetSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_SaveBudget);
        }
    }

    // Updates progress brush colors based on current budget usage
    private void UpdateProgressBrushColor()
    {
        if (_progressBrush == null) return;

        Color color1 = IsOverBudget ? (Color)ColorConverter.ConvertFromString("#E53935") : Colors.Green;
        Color color2 = IsOverBudget ? (Color)ColorConverter.ConvertFromString("#FF8A80") : Colors.LightGreen;

        _progressBrush.GradientStops[0].Color = color1;
        _progressBrush.GradientStops[1].Color = color2;
        _progressBrush.GradientStops[2].Color = color1;
    }

    // Reloads budget data from the service
    public void Reload() => LoadBudget();
}