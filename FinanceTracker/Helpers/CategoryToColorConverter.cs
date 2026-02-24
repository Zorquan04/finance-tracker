using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinanceTracker.Helpers;

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value as string;

        return category switch
        {
            "Food" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            "Bills" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            "Entertainment" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),
            "Transport" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            "Other" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}