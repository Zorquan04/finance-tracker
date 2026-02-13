using FinanceTracker.Helpers;
using FinanceTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

public class ExpenseViewModel : BaseViewModel
{
    public ObservableCollection<Expense> Expenses { get; set; } = new();
    public ObservableCollection<string> Categories { get; set; } = new()
    {
        "Food", "Transport", "Entertainment", "Bills", "Other"
    };

    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged();
        }
    }

    private string _selectedCategory = "Other";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    private DateTime _date = DateTime.Now;
    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddExpenseCommand { get; }
    public ExpenseViewModel()
    {
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
        });
    }
}