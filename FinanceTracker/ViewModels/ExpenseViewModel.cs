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
    private readonly ChartsViewModel _chartsVM;
    private readonly BudgetViewModel? _budgetVM;

    private bool _sortAscending = true;
    private string? _lastSortColumn;

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

    private Expense? _selectedExpense;
    public Expense? SelectedExpense
    {
        get => _selectedExpense;
        set
        {
            _selectedExpense = value;
            OnPropertyChanged();
        }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged();
        }
    }

    private bool _filtersVisible;
    public bool FiltersVisible
    {
        get => _filtersVisible;
        set 
        { 
            _filtersVisible = value; 
            OnPropertyChanged(); 
        }
    }

    public ICommand AddExpenseCommand { get; }
    public ICommand ToggleFiltersCommand { get; }
    public ICommand ClearDateRangeCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand SortByNameCommand { get; }
    public ICommand SortByAmountCommand { get; }
    public ICommand SortByCategoryCommand { get; }
    public ICommand SortByDateCommand { get; }

    public ExpenseViewModel(ChartsViewModel chartsVM, BudgetViewModel budgetVM)
    {
        _context = new FinanceDbContext();
        _chartsVM = chartsVM;
        _budgetVM = budgetVM;

        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedExpense != null);
        EditCommand = new RelayCommand(_ => StartEdit(), _ => SelectedExpense != null);

        LoadCategories();
        LoadExpenses();

        ExpensesView = CollectionViewSource.GetDefaultView(Expenses);
        ExpensesView.Filter = FilterExpenses;

        ExpensesView.SortDescriptions.Clear();
        ExpensesView.SortDescriptions.Add(new SortDescription(nameof(Expense.OrderIndex), ListSortDirection.Ascending));

        AddExpenseCommand = new RelayCommand(_ => SaveExpense(), _ => !string.IsNullOrWhiteSpace(Name) && Amount > 0 && SelectedCategory != null);

        SortByNameCommand = new RelayCommand(_ => SortBy(e => e.Name, "Name"));
        SortByAmountCommand = new RelayCommand(_ => SortBy(e => e.Amount, "Amount"));
        SortByCategoryCommand = new RelayCommand(_ => SortBy(e => e.Category?.Name ?? "", "Category"));
        SortByDateCommand = new RelayCommand(_ => SortBy(e => e.Date, "Date"));

        ToggleFiltersCommand = new RelayCommand(_ =>
        {
            FiltersVisible = !FiltersVisible;
        });

        ClearDateRangeCommand = new RelayCommand(_ =>
        {
            StartDate = null;
            EndDate = null;
        });

        MoveUpCommand = new RelayCommand(_ =>
        {
            if (SelectedExpense == null) return;
            var index = Expenses.IndexOf(SelectedExpense);
            if (index <= 0) return;

            SwapOrder(index, index - 1);
        }, _ => SelectedExpense != null);

        MoveDownCommand = new RelayCommand(_ =>
        {
            if (SelectedExpense == null) return;
            var index = Expenses.IndexOf(SelectedExpense);
            if (index >= Expenses.Count - 1) return;

            SwapOrder(index, index + 1);
        }, _ => SelectedExpense != null);

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
        FilterCategories = new ObservableCollection<Category>(new[] { new Category { Id = 0, Name = "All" } }.Concat(categories));

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

    private void RefreshView()
    {
        ExpensesView.Refresh();
        UpdateTotal();
    }

    public void Reload()
    {
        LoadCategories();
        LoadExpenses();
        ExpensesView.Refresh();
        UpdateTotal();
    }

    private void SaveExpense()
    {
        if (string.IsNullOrWhiteSpace(Name) || Amount <= 0 || SelectedCategory == null)
            return;

        if (IsEditing)
            UpdateExpense();
        else
            AddExpense();
    }

    private void AddExpense()
    {
        var maxIndex = Expenses.Any() ? Expenses.Max(e => e.OrderIndex) + 1 : 0;

        var newExpense = new Expense
        {
            Name = Name,
            Amount = Amount,
            Date = Date,
            CategoryId = SelectedCategory!.Id,
            OrderIndex = maxIndex
        };

        _context.Expenses.Add(newExpense);
        _context.SaveChanges();

        LoadExpenses();
        RefreshView();
        _chartsVM.Refresh();
        _budgetVM?.UpdateSpent();

        ClearForm();
    }


    private void UpdateExpense()
    {
        if (SelectedExpense == null)
            return;

        var expense = _context.Expenses.FirstOrDefault(e => e.Id == SelectedExpense.Id);

        if (expense == null)
            return;

        expense.Name = Name;
        expense.Amount = Amount;
        expense.Date = Date;
        expense.CategoryId = SelectedCategory!.Id;

        _context.SaveChanges();

        IsEditing = false;

        LoadExpenses();
        RefreshView();
        _chartsVM.Refresh();
        _budgetVM?.UpdateSpent();

        ClearForm();
    }

    private void StartEdit()
    {
        if (SelectedExpense == null)
            return;

        Name = SelectedExpense.Name!;
        Amount = SelectedExpense.Amount;
        Date = SelectedExpense.Date;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedExpense.CategoryId);

        IsEditing = true;
    }

    private void Delete()
    {
        if (SelectedExpense == null)
            return;

        var expense = _context.Expenses.FirstOrDefault(e => e.Id == SelectedExpense.Id);

        if (expense == null)
            return;

        _context.Expenses.Remove(expense);
        _context.SaveChanges();

        LoadExpenses();
        RefreshView();
        _chartsVM.Refresh();
        _budgetVM?.UpdateSpent();
    }

    private void SwapOrder(int index1, int index2)
    {
        var temp = Expenses[index1].OrderIndex;
        Expenses[index1].OrderIndex = Expenses[index2].OrderIndex;
        Expenses[index2].OrderIndex = temp;

        Expenses.Move(index1, index2);

        ExpensesView.Refresh();
    }

    private void ClearForm()
    {
        Name = "";
        Amount = 0;
        Date = DateTime.Now;
        IsEditing = false;
    }

    private void SortBy<T>(Func<Expense, T> keySelector, string columnName)
    {
        if (_lastSortColumn == columnName)
            _sortAscending = !_sortAscending;
        else
            _sortAscending = true;

        _lastSortColumn = columnName;

        var sorted = _sortAscending ? Expenses.OrderBy(keySelector).ToList() : Expenses.OrderByDescending(keySelector).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].OrderIndex = i;
        }

        Expenses.Clear();
        foreach (var e in sorted)
            Expenses.Add(e);

        ExpensesView.Refresh();
        _context.SaveChanges();
    }
}