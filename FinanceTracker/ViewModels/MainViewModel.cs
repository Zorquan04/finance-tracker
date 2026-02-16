using FinanceTracker.Data;
using FinanceTracker.Helpers;
using FinanceTracker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;

namespace FinanceTracker.ViewModels;

public class MainViewModel : BaseViewModel
{
    public event Action? ShowExpensesRequested;
    public event Action? ShowChartsRequested;
    public event Action? ShowBudgetRequested;

    private readonly ICsvService _csvService;
    private string? _currentFilePath;

    public RelayCommand ShowExpensesCommand { get; }
    public RelayCommand ShowChartsCommand { get; }
    public RelayCommand ShowBudgetCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand ExitCommand { get; }

    public ChartsViewModel ChartsVM { get; }
    public ExpenseViewModel ExpensesVM { get; }
    public BudgetViewModel BudgetVM { get; }

    public MainViewModel()
    {
        _csvService = new CsvService();

        ChartsVM = new ChartsViewModel();
        BudgetVM = new BudgetViewModel();
        ExpensesVM = new ExpenseViewModel(ChartsVM, BudgetVM);
        
        ShowExpensesCommand = new RelayCommand(_ => ShowExpensesRequested?.Invoke());
        ShowChartsCommand = new RelayCommand(_ => ShowChartsRequested?.Invoke());
        ShowBudgetCommand = new RelayCommand(_ => ShowBudgetRequested?.Invoke());

        SaveCommand = new RelayCommand(_ => Save());
        SaveAsCommand = new RelayCommand(_ => SaveAs());
        OpenCommand = new RelayCommand(_ => Open());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
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
        var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        _currentFilePath = dialog.FileName;

        var (expenses, limit) = _csvService.Import(_currentFilePath);

        using var context = new FinanceDbContext();

        context.Expenses.RemoveRange(context.Expenses);
        context.MonthlyBudgets.RemoveRange(context.MonthlyBudgets);
        context.SaveChanges();

        var categoriesCache = context.Categories.ToList();

        foreach (var e in expenses)
        {
            var categoryName = e.Category?.Name ?? "Other";
            var existingCategory = context.Categories.FirstOrDefault(c => c.Name == categoryName);

            if (existingCategory == null)
            {
                existingCategory = new Models.Category

                {
                    Name = categoryName
                };

                context.Categories.Add(existingCategory);
                categoriesCache.Add(existingCategory);
            }

            e.CategoryId = existingCategory.Id;
            e.Category = null;

            context.Expenses.Add(e);
        }

        context.SaveChanges();

        if (limit > 0)
        {
            var now = DateTime.Now;

            context.MonthlyBudgets.Add(new Models.MonthlyBudget
            {
                Year = now.Year,
                Month = now.Month,
                Limit = limit
            });
        }

        context.SaveChanges();

        ExpensesVM.Reload();
        ChartsVM.Refresh();
        BudgetVM.Reload();
    }

    private void ExportToFile(string path)
    {
        using var context = new FinanceDbContext();

        var expenses = context.Expenses.Include(e => e.Category).ToList();

        var now = DateTime.Now;

        var budget = context.MonthlyBudgets.FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month);

        var limit = budget?.Limit ?? 0;

        _csvService.Export(path, expenses, limit);
    }
}