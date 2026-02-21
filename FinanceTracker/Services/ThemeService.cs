using FinanceTracker.Services.Interfaces;
using System.Windows;

namespace FinanceTracker.Services;

public class ThemeService : IThemeService
{
    private bool _isDark;

    public void ToggleTheme()
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries.Clear();

        var newTheme = new ResourceDictionary
        {
            Source = new Uri(_isDark ? "Views/Themes/LightTheme.xaml" : "Views/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        dictionaries.Add(newTheme);
        _isDark = !_isDark;
    }
}