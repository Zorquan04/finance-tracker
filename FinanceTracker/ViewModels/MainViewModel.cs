using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace FinanceTracker.ViewModels;

public class MainViewModel : BaseViewModel
{
    public event Action? ShowExpensesRequested;
    public event Action? ShowChartsRequested;
    public event Action? ShowBudgetRequested;

    private readonly ICsvService _csvService;
    private readonly IMessageService _messageService;
    private readonly IThemeService _themeService;
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;
    private readonly IChartService _chartService;
    private string? _currentFilePath;

    public RelayCommand ShowExpensesCommand { get; }
    public RelayCommand ShowChartsCommand { get; }
    public RelayCommand ShowBudgetCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand ExitCommand { get; }
    public RelayCommand ChangeThemeCommand { get; }
    public RelayCommand ChangeLanguageCommand { get; }
    public RelayCommand ClearDataCommand { get; }
    public RelayCommand ShowAboutCommand { get; }
    public RelayCommand OpenUserGuideCommand { get; }

    public ChartViewModel ChartVM { get; }
    public ExpenseViewModel ExpensesVM { get; }
    public BudgetViewModel BudgetVM { get; }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isStatusVisible;
    public bool IsStatusVisible
    {
        get => _isStatusVisible;
        set => SetProperty(ref _isStatusVisible, value);
    }

    public bool HasUnsavedChanges => ExpensesVM?.HasUnsavedChanges == true || BudgetVM?.HasUnsavedChanges == true;
    public IMessageService MessageService => _messageService;

    public MainViewModel(ICsvService csvService, IMessageService messageService, IThemeService themeService, IExpenseService expenseService, IBudgetService budgetService, IChartService chartService)
    {
        _csvService = csvService;
        _messageService = messageService;
        _themeService = themeService;
        _expenseService = expenseService;
        _budgetService = budgetService;
        _chartService = chartService;

        ChartVM = new ChartViewModel(_chartService);
        BudgetVM = new BudgetViewModel(_budgetService, _messageService);
        ExpensesVM = new ExpenseViewModel(_expenseService, _messageService, ChartVM, BudgetVM);

        ShowExpensesCommand = new RelayCommand(_ => ShowExpensesRequested?.Invoke());
        ShowChartsCommand = new RelayCommand(_ => ShowChartsRequested?.Invoke());
        ShowBudgetCommand = new RelayCommand(_ => ShowBudgetRequested?.Invoke());

        SaveCommand = new RelayCommand(_ => Save());
        SaveAsCommand = new RelayCommand(_ => SaveAs());
        OpenCommand = new RelayCommand(_ => Open());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

        ChangeThemeCommand = new RelayCommand(_ => ChangeTheme());
        ChangeLanguageCommand = new RelayCommand(_ => ChangeLanguage());
        ClearDataCommand = new RelayCommand(_ => ClearData());

        ShowAboutCommand = new RelayCommand(_ => ShowAbout());
        OpenUserGuideCommand = new RelayCommand(_ => OpenUserGuide());

        BudgetVM.BudgetSaved += () => ShowSuccess("Saved changes");
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            SaveAs();
            return;
        }

        ExportToFile(_currentFilePath);
    }

    private void SaveAs()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() == true)
        {
            _currentFilePath = dialog.FileName;
            ExportToFile(_currentFilePath);
        }
    }

    private void Open()
    {
        var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
        if (dialog.ShowDialog() != true) return;
        _currentFilePath = dialog.FileName;

        var (expenses, limit) = _csvService.Import(_currentFilePath);

        _expenseService.ClearAllExpenses();

        var existingCategories = _expenseService.GetAllCategories();

        foreach (var e in expenses)
        {
            var categoryName = e.Category?.Name;

            if (string.IsNullOrWhiteSpace(categoryName))
                continue;

            var existingCategory = existingCategories
                .FirstOrDefault(c => c.Name == categoryName);

            if (existingCategory == null)
            {
                existingCategory = new Category
                {
                    Name = categoryName
                };

                _expenseService.AddCategory(existingCategory);
                existingCategories.Add(existingCategory);
            }

            e.CategoryId = existingCategory.Id;
            e.Category = null;

            _expenseService.AddExpense(e);
        }

        if (limit > 0)
        {
            var budget = new MonthlyBudget
            {
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                Limit = limit
            };
            _budgetService.SaveBudget(budget);
        }

        ExpensesVM.Reload();
        ChartVM.Refresh();
        BudgetVM.Reload();
    }

    private void ExportToFile(string path)
    {
        var expenses = _expenseService.GetAllExpenses();
        var limit = _budgetService.GetCurrentBudget()?.Limit ?? 0;

        _csvService.Export(path, expenses, limit);

        ShowSuccess("Saved changes");
    }

    private void ClearData()
    {
        if (!_messageService.Confirm("This will permanently delete all expenses. Continue?", "Clear Data"))
            return;

        _expenseService.ClearAllExpenses();

        ExpensesVM.Reload();
        ChartVM.Refresh();
        BudgetVM.Reload();

        ShowSuccess("All data cleared");
    }

    private void ChangeTheme()
    {
       _themeService.ToggleTheme();
    }

    private void ChangeLanguage()
    {
        var confirmed = _messageService.Confirm("Switch language? Application will restart.", "Language");

        if (!confirmed)
            return;

        var newCulture = Thread.CurrentThread.CurrentCulture.Name == "en-GB" ? "pl-PL" : "en-GB";

        var exePath = Process.GetCurrentProcess().MainModule!.FileName!;

        Thread.CurrentThread.CurrentCulture = new CultureInfo(newCulture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(newCulture);

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true
        });

        Application.Current.Shutdown();
    }

    private void ShowAbout()
    {
        var aboutWindow = new Views.AboutWindow
        {
            Owner = Application.Current.MainWindow
        };

        aboutWindow.ShowDialog();
    }

    private void OpenUserGuide()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Zorquan04/finance-tracker",
            UseShellExecute = true
        });
    }

    public async void ShowSuccess(string message)
    {
        StatusMessage = message;
        IsStatusVisible = true;

        await Task.Delay(2000);

        IsStatusVisible = false;
    }
}