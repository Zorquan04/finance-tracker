using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinanceTracker.Helpers;

public class VisibilityConverter : IValueConverter
{
    // Converts a value to Visibility based on its type and optional invert parameter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the "Invert" parameter is passed
        bool invert = parameter?.ToString() == "Invert";

        // Determine visibility based on value type
        bool visible = value switch
        {
            bool b => b,
            int i => i > 0,
            _ => false
        };

        // Invert visibility if requested
        if (invert)
            visible = !visible;

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    // ConvertBack is not implemented for one-way bindings
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}