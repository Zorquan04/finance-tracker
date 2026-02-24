using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Controls;

namespace FinanceTracker.Views
{
    public partial class CustomChartTooltip : UserControl, IChartTooltip
    {
        public CustomChartTooltip()
        {
            InitializeComponent();
            DataContext = this;
        }

        private TooltipData? _data;

        public event PropertyChangedEventHandler? PropertyChanged;
        public TooltipSelectionMode? SelectionMode { get; set; }
        public IEnumerable<DataPointViewModel>? DataPoints => Data?.Points;

        public TooltipData? Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
                OnPropertyChanged(nameof(DataPoints));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}