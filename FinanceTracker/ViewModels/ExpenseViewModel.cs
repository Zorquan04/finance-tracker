using FinanceTracker.Helpers;
using FinanceTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using FinanceTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.ViewModels;

public class ExpenseViewModel : BaseViewModel
{
    public ObservableCollection<Expense> Expenses { get; set; } = new();
    public ObservableCollection<Category> Categories { get; set; } = new();
    public ObservableCollection<Category> FilterCategories { get; set; } = new();

    public ICollectionView ExpensesView { get; }

    private readonly FinanceDbContext _context;

    private string _name = "";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    private decimal _totalExpenses;
    public decimal TotalExpenses
    {
        get => _totalExpenses;
        set { _totalExpenses = value; OnPropertyChanged(); }
    }

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    private Category? _selectedFilterCategory;
    public Category? SelectedFilterCategory
    {
        get => _selectedFilterCategory;
        set
        {
            _selectedFilterCategory = value;
            OnPropertyChanged();
            ExpensesView.Refresh();
            UpdateTotal();
        }
    }

    private DateTime _date = DateTime.Now;
    public DateTime Date
    {
        get => _date;
        set { _date = value; OnPropertyChanged(); }
    }

    private DateTime? _startDate;
    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            _startDate = value;
            OnPropertyChanged();
            ExpensesView.Refresh();
            UpdateTotal();
        }
    }

    private DateTime? _endDate;
    public DateTime? EndDate
    {
        get => _endDate;
        set
        {
            _endDate = value;
            OnPropertyChanged();
            ExpensesView.Refresh();
            UpdateTotal();
        }
    }

    private bool _filtersVisible;
    public bool FiltersVisible
    {
        get => _filtersVisible;
        set { _filtersVisible = value; OnPropertyChanged(); }
    }

    public ICommand AddExpenseCommand { get; }
    public ICommand ToggleFiltersCommand { get; }
    public ICommand ClearDateRangeCommand { get; }

    public ExpenseViewModel()
    {
        _context = new FinanceDbContext();

        LoadCategories();
        LoadExpenses();

        ExpensesView = CollectionViewSource.GetDefaultView(Expenses);
        ExpensesView.Filter = FilterExpenses;

        AddExpenseCommand = new RelayCommand(_ =>
        {
            if (string.IsNullOrWhiteSpace(Name) || Amount <= 0 || SelectedCategory == null)
                return;

            var newExpense = new Expense
            {
                Name = Name,
                Amount = Amount,
                Date = Date,
                CategoryId = SelectedCategory.Id
            };

            _context.Expenses.Add(newExpense);
            _context.SaveChanges();

            LoadExpenses();
            ExpensesView.Refresh();
            UpdateTotal();

            Name = "";
            Amount = 0;
            Date = DateTime.Now;
        });

        ToggleFiltersCommand = new RelayCommand(_ =>
        {
            FiltersVisible = !FiltersVisible;
        });

        ClearDateRangeCommand = new RelayCommand(_ =>
        {
            StartDate = null;
            EndDate = null;
        });

        SelectedCategory = Categories.LastOrDefault();
        SelectedFilterCategory = FilterCategories.FirstOrDefault();
    }

    private bool FilterExpenses(object obj)
    {
        if (obj is not Expense expense)
            return false;

        if (SelectedFilterCategory != null && SelectedFilterCategory.Id != 0 && expense.CategoryId != SelectedFilterCategory.Id)
            return false;

        if (StartDate.HasValue && expense.Date < StartDate.Value)
            return false;

        if (EndDate.HasValue && expense.Date > EndDate.Value)
            return false;

        return true;
    }

    private void UpdateTotal()
    {
        TotalExpenses = Expenses.Where(e => FilterExpenses(e)).Sum(e => e.Amount);
    }

    private void LoadCategories()
    {
        var categories = _context.Categories.ToList();

        Categories = new ObservableCollection<Category>(categories);

        var allCategory = new Category { Id = 0, Name = "All" };
        FilterCategories = new ObservableCollection<Category> { allCategory };
        foreach (var c in categories)
            FilterCategories.Add(c);

        OnPropertyChanged(nameof(Categories));
        OnPropertyChanged(nameof(FilterCategories));
    }

    private void LoadExpenses()
    {
        var expenses = _context.Expenses.Include(e => e.Category).ToList();

        Expenses.Clear();
        foreach (var e in expenses)
            Expenses.Add(e);
    }
}