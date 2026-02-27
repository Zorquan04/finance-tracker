using FinanceTracker.Helpers;
using FinanceTracker.Models;
using FinanceTracker.Resources;
using FinanceTracker.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

// ViewModel responsible for managing expenses, categories, filtering, sorting, and editing
public class ExpenseViewModel : BaseViewModel
{
    // Collections for expenses and categories
    public ObservableCollection<Expense> Expenses { get; set; } = new();
    public ObservableCollection<Category> Categories { get; set; } = new();
    public ObservableCollection<Category> FilterCategories { get; set; } = new();
    public ICollectionView ExpensesView { get; }

    // Services and related ViewModels
    private readonly IExpenseService _expenseService;
    private readonly IMessageService _messageService;
    private readonly ChartViewModel _chartVM;
    private readonly BudgetViewModel? _budgetVM;

    // Properties for binding to the UI
    private string _name = "";
    private decimal _amount;
    private decimal _totalExpenses;
    private Category? _selectedCategory;
    private Category? _selectedFilterCategory;
    private DateTime _date = DateTime.Now;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private Expense? _selectedExpense;
    private bool _isEditing;
    private bool _filtersVisible;
    private bool _sortAscending = true;
    private string? _lastSortColumn;

    public string Name 
    { 
        get => _name; 
        set => SetProperty(ref _name, value); 
    }

    public decimal Amount 
    { 
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public decimal TotalExpenses 
    { 
        get => _totalExpenses;
        set => SetProperty(ref _totalExpenses, value);
    }

    public Category? SelectedCategory
    { 
        get => _selectedCategory; 
        set => SetProperty(ref _selectedCategory, value);
    }

    public Category? SelectedFilterCategory
    {
        get => _selectedFilterCategory;
        set
        {
            if (SetProperty(ref _selectedFilterCategory, value))
                RefreshView(); // Refresh view when filter changes
        }
    }

    public DateTime Date
    { 
        get => _date; 
        set => SetProperty(ref _date, value);
    }

    public DateTime? StartDate
    { 
        get => _startDate;
        set
        { 
            if (SetProperty(ref _startDate, value)) 
                RefreshView();
        }
    }

    public DateTime? EndDate 
    { 
        get => _endDate;
        set 
        { 
            if (SetProperty(ref _endDate, value)) 
                RefreshView();
        }
    }

    public Expense? SelectedExpense
    { 
        get => _selectedExpense; 
        set => SetProperty(ref _selectedExpense, value); 
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (SetProperty(ref _isEditing, value))
            {
                // Update button texts when editing state changes
                OnPropertyChanged(nameof(AddButtonText));
                OnPropertyChanged(nameof(EditButtonText));
            }
        }
    }

    public bool FiltersVisible 
    { 
        get => _filtersVisible; 
        set => SetProperty(ref _filtersVisible, value);
    }

    public string AddButtonText => IsEditing ? AppResources.Button_Save : AppResources.Button_Add;
    public string EditButtonText => IsEditing ? AppResources.Button_Cancel : AppResources.Button_Edit;
    public bool HasUnsavedChanges => IsEditing || !string.IsNullOrWhiteSpace(Name) || Amount > 0;

    // Commands for the UI
    public ICommand AddExpenseCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand ToggleFiltersCommand { get; }
    public ICommand ClearDateRangeCommand { get; }
    public ICommand SortByNameCommand { get; }
    public ICommand SortByAmountCommand { get; }
    public ICommand SortByCategoryCommand { get; }
    public ICommand SortByDateCommand { get; }

    // Constructor initializes services, ViewModels, loads data, and sets up commands
    public ExpenseViewModel(IExpenseService expenseService, IMessageService messageService, ChartViewModel chartVM, BudgetViewModel budgetVM)
    {
        _expenseService = expenseService;
        _messageService = messageService;
        _chartVM = chartVM;
        _budgetVM = budgetVM;

        LoadCategories();
        LoadExpenses();

        ExpensesView = CollectionViewSource.GetDefaultView(Expenses);
        ExpensesView.Filter = FilterExpenses;
        ExpensesView.SortDescriptions.Add(new SortDescription(nameof(Expense.OrderIndex), ListSortDirection.Ascending));

        // Commands with action and conditions
        AddExpenseCommand = new RelayCommand(_ => SaveExpense(), _ => CanAddOrEdit());
        EditCommand = new RelayCommand(_ => { if (IsEditing) CancelEdit(); else StartEdit(); }, _ => SelectedExpense != null);
        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedExpense != null);
        MoveUpCommand = new RelayCommand(_ => SwapOrder(true), _ => SelectedExpense != null);
        MoveDownCommand = new RelayCommand(_ => SwapOrder(false), _ => SelectedExpense != null);
        ToggleFiltersCommand = new RelayCommand(_ => FiltersVisible = !FiltersVisible);
        ClearDateRangeCommand = new RelayCommand(_ => { StartDate = null; EndDate = null; });
        SortByNameCommand = new RelayCommand(_ => SortBy(e => e.Name, AppResources.Header_Name));
        SortByAmountCommand = new RelayCommand(_ => SortBy(e => e.Amount, AppResources.Header_Amount));
        SortByCategoryCommand = new RelayCommand(_ => SortBy(e => e.Category?.DisplayName ?? "", AppResources.Header_Category));
        SortByDateCommand = new RelayCommand(_ => SortBy(e => e.Date, AppResources.Header_Date));

        // Set default selected categories
        SelectedCategory = Categories.LastOrDefault();
        SelectedFilterCategory = FilterCategories.FirstOrDefault();
    }

    // Load all categories from the service
    private void LoadCategories()
    {
        try
        {
            var categories = _expenseService.GetAllCategories();
            Categories = new ObservableCollection<Category>(categories);
            FilterCategories = new ObservableCollection<Category>(new[] { new Category { Id = 0, Name = "All" } }.Concat(categories));

            OnPropertyChanged(nameof(Categories));
            OnPropertyChanged(nameof(FilterCategories));
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadCategories);
        }
    }

    // Load all expenses from the service
    private void LoadExpenses()
    {
        try
        {
            Expenses.Clear();
            foreach (var e in _expenseService.GetAllExpenses())
                Expenses.Add(e);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_LoadExpenses);
        }
    }

    // Save either a new expense or update an existing one
    private void SaveExpense()
    {
        if (IsEditing)
            UpdateExpense();
        else
            AddExpense();
    }

    // Add a new expense
    private void AddExpense()
    {
        try
        {
            _expenseService.AddExpense(new Expense
            {
                Name = Name,
                Amount = Amount,
                Date = Date,
                CategoryId = SelectedCategory!.Id
            });

            AfterDatabaseChange();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_AddExpense);
        }
    }

    // Update an existing expense
    private void UpdateExpense()
    {
        try
        {
            if (SelectedExpense == null) return;

            _expenseService.UpdateExpense(new Expense
            {
                Id = SelectedExpense.Id,
                Name = Name,
                Amount = Amount,
                Date = Date,
                CategoryId = SelectedCategory!.Id
            });

            AfterDatabaseChange();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_UpdateExpense);
        }
    }

    // Delete selected expense
    private void Delete()
    {
        try
        {
            if (SelectedExpense == null) return;
            _expenseService.DeleteExpense(SelectedExpense.Id);
            AfterDatabaseChange();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Delete);
        }
    }

    // Start editing selected expense
    private void StartEdit()
    {
        if (SelectedExpense == null) return;
        Name = SelectedExpense.Name!;
        Amount = SelectedExpense.Amount;
        Date = SelectedExpense.Date;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedExpense.CategoryId);
        IsEditing = true;
    }

    // Cancel editing
    private void CancelEdit()
    {
        ClearForm();
        SelectedExpense = null;
    }

    // Clear input form
    private void ClearForm()
    {
        Name = "";
        Amount = 0;
        Date = DateTime.Now;
        IsEditing = false;
    }

    // Check if adding or editing is valid
    private bool CanAddOrEdit() => !string.IsNullOrWhiteSpace(Name) && Amount > 0 && SelectedCategory != null;

    // Filter function for the view
    private bool FilterExpenses(object obj)
    {
        if (obj is not Expense e) return false;
        if (SelectedFilterCategory?.Id > 0 && e.CategoryId != SelectedFilterCategory.Id) return false;
        if (StartDate.HasValue && e.Date < StartDate) return false;
        if (EndDate.HasValue && e.Date > EndDate) return false;
        return true;
    }

    // Update total of filtered expenses
    private void UpdateTotal() => TotalExpenses = Expenses.Where(e => FilterExpenses(e)).Sum(e => e.Amount);

    // Check if budget limit is exceeded
    private void CheckBudgetOverflow()
    {
        if (_budgetVM == null) return;

        if (_budgetVM.SpentThisMonth > _budgetVM.MonthlyLimit && _budgetVM.MonthlyLimit > 0)
            _messageService.ShowWarning(AppResources.Dialog_BudgetAlertMessage, AppResources.DialogBudgedAlertTitle);
    }

    // Refresh the expenses view and update total
    private void RefreshView()
    {
        ExpensesView.Refresh();
        UpdateTotal();
    }

    // Sort expenses by a given key
    private void SortBy<T>(Func<Expense, T> keySelector, string columnName)
    {
        try
        {
            _sortAscending = _lastSortColumn == columnName ? !_sortAscending : true;
            _lastSortColumn = columnName;

            var sorted = _sortAscending ? Expenses.OrderBy(keySelector).ToList() : Expenses.OrderByDescending(keySelector).ToList();
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].OrderIndex = i;

            Expenses.Clear();
            foreach (var e in sorted)
                Expenses.Add(e);

            ExpensesView.Refresh();
            _expenseService.UpdateOrder(Expenses.ToList());
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Swap);
        }
    }

    // Swap the order of expenses in the list
    private void SwapOrder(bool moveUp)
    {
        try
        {
            if (SelectedExpense == null || ExpensesView is not ListCollectionView view) return;

            int index = view.IndexOf(SelectedExpense);
            int targetIndex = moveUp ? index - 1 : index + 1;
            if (targetIndex < 0 || targetIndex >= view.Count) return;

            var current = (Expense)view.GetItemAt(index);
            var target = (Expense)view.GetItemAt(targetIndex);

            _expenseService.SwapOrder(current.Id, target.Id);

            Expenses.Move(Expenses.IndexOf(current), Expenses.IndexOf(target));
            SelectedExpense = current;
            ExpensesView.Refresh();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Swap);
        }
    }

    // Actions to perform after database changes
    private void AfterDatabaseChange()
    {
        try
        {
            LoadExpenses();
            RefreshView();
            _chartVM.Refresh();
            _budgetVM?.UpdateSpent();
            CheckBudgetOverflow();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_AfterDatabaseChange);
        }
    }

    // Reload all data
    public void Reload()
    {
        try
        {
            LoadCategories();
            LoadExpenses();
            RefreshView();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, AppResources.Error_Reload);
        }
    }
}