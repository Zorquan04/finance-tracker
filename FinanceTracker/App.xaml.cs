using System.Globalization;
using System.Windows;

namespace FinanceTracker;

public partial class App : Application
{
    public App()
    {
        var culture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
}