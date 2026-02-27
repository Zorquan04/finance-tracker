using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FinanceTracker.Views
{
    // Custom tooltip for LiveCharts charts
    public partial class CustomChartTooltip : UserControl, IChartTooltip, INotifyPropertyChanged
    {
        // DependencyProperty to switch between normal/trend mode
        public static readonly DependencyProperty IsTrendModeProperty = DependencyProperty.Register(nameof(IsTrendMode), typeof(bool), typeof(CustomChartTooltip), new PropertyMetadata(false));

        private TooltipData? _data;
        private TooltipSelectionMode? _selectionMode;

        // Event for notifying property changes (for data binding)
        public event PropertyChangedEventHandler? PropertyChanged;

        // Indicates if tooltip is in trend mode
        public bool IsTrendMode
        {
            get => (bool)GetValue(IsTrendModeProperty);
            set => SetValue(IsTrendModeProperty, value);
        }

        // Tooltip data provided by LiveCharts
        public TooltipData? Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(); // Notify binding when data changes
            }
        }

        // Selection mode of the tooltip
        public TooltipSelectionMode? SelectionMode
        {
            get => _selectionMode;
            set
            {
                _selectionMode = value;
                OnPropertyChanged(); // Notify binding when selection mode changes
            }
        }

        // Constructor initializes the UI components
        public CustomChartTooltip()
        {
            InitializeComponent();
        }

        // Helper method to raise PropertyChanged event
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}