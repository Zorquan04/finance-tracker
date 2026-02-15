using FinanceTracker.ViewModels;
using System.Windows.Controls;

namespace FinanceTracker.Views;

public partial class ChartsView : UserControl
{
    private ChartsViewModel? _viewModel;
    public ChartsViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            DataContext = _viewModel;
        }
    }

    public ChartsView()
    {
        InitializeComponent();
    }
}