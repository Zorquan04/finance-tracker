using FinanceTracker.Helpers;
using System.Windows.Input;

namespace FinanceTracker.ViewModels;

public class MainViewModel : BaseViewModel
{
    private string _title = "Personal Finance Tracker";

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public ICommand ChangeTitleCommand { get; }

    public MainViewModel()
    {
        ChangeTitleCommand = new RelayCommand(_ => Title = "Updated Finance Tracker");
    }
}