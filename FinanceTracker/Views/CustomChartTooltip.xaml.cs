using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FinanceTracker.Views
{
    public partial class CustomChartTooltip : UserControl, IChartTooltip, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsTrendModeProperty = DependencyProperty
            .Register( nameof(IsTrendMode), typeof(bool), typeof(CustomChartTooltip), new PropertyMetadata(false));

        private TooltipData? _data;
        private TooltipSelectionMode? _selectionMode;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsTrendMode
        {
            get => (bool)GetValue(IsTrendModeProperty);
            set => SetValue(IsTrendModeProperty, value);
        }

        public TooltipData? Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public TooltipSelectionMode? SelectionMode
        {
            get => _selectionMode;
            set
            {
                _selectionMode = value;
                OnPropertyChanged();
            }
        }

        public CustomChartTooltip()
        {
            InitializeComponent();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}