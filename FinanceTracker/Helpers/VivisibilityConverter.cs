using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinanceTracker.Helpers;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";

        bool visible = value switch
        {
            bool b => b,
            int i => i > 0,
            _ => false
        };

        if (invert)
            visible = !visible;

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}