using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace FinanceTracker.ViewModels;

// Main ViewModel for the application, handles commands, file operations, and communicates with services
public class MainViewModel : BaseViewModel
{
    // Events to notify the view to switch between sections
    public event Action? ShowExpensesRequested;
    public event Action? ShowChartsRequested;
    public event Action? ShowBudgetRequested;

    // Services injected via constructor
    private readonly ICsvService _csvService;
    private readonly IMessageService _messageService;
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;
    private readonly IChartService _chartService;
    private string? _currentFilePath;

    // Commands exposed to the UI
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

    // Sub ViewModels
    public ChartViewModel ChartVM { get; }
    public ExpenseViewModel ExpensesVM { get; }
    public BudgetViewModel BudgetVM { get; }

    // Status bar properties
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

    // Indicates if there are unsaved changes in any section
    public bool HasUnsavedChanges => ExpensesVM?.HasUnsavedChanges == true || BudgetVM?.HasUnsavedChanges == true;
    public IMessageService MessageService => _messageService;

    // Constructor, initializes services, ViewModels, and commands
    public MainViewModel(ICsvService csvService, IMessageService messageService, IExpenseService expenseService, IBudgetService budgetService, IChartService chartService)
    {
        _csvService = csvService;
        _messageService = messageService;
        _expenseService = expenseService;
        _budgetService = budgetService;
        _chartService = chartService;

        ChartVM = new ChartViewModel(_chartService);
        BudgetVM = new BudgetViewModel(_budgetService, _messageService);
        ExpensesVM = new ExpenseViewModel(_expenseService, _messageService, ChartVM, BudgetVM);

        // Section switching commands
        ShowExpensesCommand = new RelayCommand(_ => ShowExpensesRequested?.Invoke());
        ShowChartsCommand = new RelayCommand(_ => ShowChartsRequested?.Invoke());
        ShowBudgetCommand = new RelayCommand(_ => ShowBudgetRequested?.Invoke());

        // File operations commands
        SaveCommand = new RelayCommand(_ => Save());
        SaveAsCommand = new RelayCommand(_ => SaveAs());
        OpenCommand = new RelayCommand(_ => Open());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

        // Settings commands
        ChangeThemeCommand = new RelayCommand(_ => ChangeTheme());
        ChangeLanguageCommand = new RelayCommand(_ => ChangeLanguage());
        ClearDataCommand = new RelayCommand(_ => ClearData());

        // Help & About commands
        ShowAboutCommand = new RelayCommand(_ => ShowAbout());
        OpenUserGuideCommand = new RelayCommand(_ => OpenUserGuide());

        // Hook event to show success message after budget save
        BudgetVM.BudgetSaved += () => ShowSuccess(AppResources.Title_ChangesSaved);
    }

    // Saves current data to the current file path
    private void Save()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveAs(); // If no file selected, trigger Save As
                return;
            }

            ExportToFile(_currentFilePath);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Save);
        }
    }

    // Opens SaveFileDialog to select file path, then exports data
    private void SaveAs()
    {
        try
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
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_SaveAs);
        }
    }

    // Opens OpenFileDialog, imports CSV data, and populates services
    private void Open()
    {
        try
        {
            var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
            if (dialog.ShowDialog() != true) return;
            _currentFilePath = dialog.FileName;

            var (expenses, limit) = _csvService.Import(_currentFilePath);

            _expenseService.ClearAllExpenses();

            var existingCategories = _expenseService.GetAllCategories();
            var otherCategory = existingCategories.First(c => c.Name == CategoryType.Other.ToString());

            foreach (var e in expenses)
            {
                CategoryType parsedType;
                if (!Enum.TryParse(e.Category?.Name, true, out parsedType))
                    parsedType = CategoryType.Other;

                var matchedCategory = existingCategories.FirstOrDefault(c => c.Name == parsedType.ToString());
                var finalCategory = matchedCategory ?? otherCategory;

                e.CategoryId = finalCategory.Id;
                e.Category = null;

                _expenseService.AddExpense(e);
            }

            // Set budget limit if exists
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

            // Refresh all ViewModels
            ExpensesVM.Reload();
            ChartVM.Refresh();
            BudgetVM.Reload();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Open);
        }
    }

    // Helper to export data to CSV
    private void ExportToFile(string path)
    {
        try
        {
            var expenses = _expenseService.GetAllExpenses();
            var limit = _budgetService.GetCurrentBudget()?.Limit ?? 0;

            _csvService.Export(path, expenses, limit);

            ShowSuccess(AppResources.Title_ChangesSaved);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Export);
        }
    }

    // Clears all data after user confirmation
    private void ClearData()
    {
        try
        {
            if (!_messageService.Confirm(AppResources.Dialog_ClearDataMessage, AppResources.Dialog_ClearDataTitle)) return;

            _expenseService.ClearAllExpenses();

            ExpensesVM.Reload();
            ChartVM.Refresh();
            BudgetVM.Reload();

            ShowSuccess(AppResources.TItle_AllDataCleared);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Clear);
        }
    }

    // Switch between light and dark themes
    private void ChangeTheme()
    {
        var confirmed = _messageService.Confirm(AppResources.Dialog_ChangeThemeMessege, AppResources.Dialog_ChangeThemeTitle);

        if (!confirmed) return;

        var newTheme = Properties.Settings.Default.Theme == "Light" ? "Dark" : "Light";

        Properties.Settings.Default.Theme = newTheme;
        Properties.Settings.Default.Save();

        RestartApplication();
    }

    // Switch language between English and Polish
    private void ChangeLanguage()
    {
        var confirmed = _messageService.Confirm(AppResources.Dialog_ChangeLanguageMessage, AppResources.Dialog_ChangeLanguageTitle);

        if (!confirmed) return;

        var newLang = Properties.Settings.Default.Language == "en-GB" ? "pl-PL" : "en-GB";

        Properties.Settings.Default.Language = newLang;
        Properties.Settings.Default.Save();

        RestartApplication();
    }

    // Open About window
    private void ShowAbout()
    {
        var aboutWindow = new Views.AboutWindow
        {
            Owner = Application.Current.MainWindow
        };

        aboutWindow.ShowDialog();
    }

    // Open online user guide
    private void OpenUserGuide()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Zorquan04/finance-tracker",
            UseShellExecute = true
        });
    }

    // Restarts the application after changing settings
    private void RestartApplication()
    {
        var exePath = Process.GetCurrentProcess().MainModule!.FileName!;

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true
        });

        Application.Current.Shutdown();
    }

    // Show temporary status message at the bottom
    public async void ShowSuccess(string message)
    {
        StatusMessage = message;
        IsStatusVisible = true;

        await Task.Delay(2000);

        IsStatusVisible = false;
    }
}