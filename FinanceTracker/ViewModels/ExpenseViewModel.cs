using FinanceTracker.Helpers;
using FinanceTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

namespace FinanceTracker.ViewModels;

public class ExpenseViewModel : BaseViewModel
{
    public ObservableCollection<Expense> Expenses { get; set; } = new();
    public ObservableCollection<string> Categories { get; set; } = new()
    {
        "Food", "Transport", "Entertainment", "Bills", "Other"
    };

    public ObservableCollection<string> FilterCategories { get; } = new() 
    { 
        "All", "Food", "Transport", "Entertainment", "Bills", "Other" 
    };

    public ICollectionView ExpensesView { get; }

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

    private string _selectedCategory = "Other";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    private string _selectedFilterCategory = "All";
    public string SelectedFilterCategory
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

    private DateTime? _startDate = null;
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

    private DateTime? _endDate = null;
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

    private bool _filtersVisible = false;
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
        ExpensesView = CollectionViewSource.GetDefaultView(Expenses);
        ExpensesView.Filter = FilterExpenses;

        AddExpenseCommand = new RelayCommand(_ =>
        {
            if (string.IsNullOrWhiteSpace(Name) || Amount <= 0)
                return;

            Expenses.Add(new Expense
            {
                Name = this.Name,
                Amount = this.Amount,
                Category = this.SelectedCategory,
                Date = this.Date
            });

            Name = "";
            Amount = 0;
            SelectedCategory = "Other";
            Date = DateTime.Now;

            ExpensesView.Refresh();
            UpdateTotal();
        });

        ToggleFiltersCommand = new RelayCommand(_ => {FiltersVisible = !FiltersVisible; });
        ClearDateRangeCommand = new RelayCommand(_ => {StartDate = null; EndDate = null; });
    }

    private bool FilterExpenses(object obj)
    {
        if (obj is not Expense expense)
            return false;

        if (SelectedFilterCategory != "All" && expense.Category != SelectedFilterCategory)
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
}