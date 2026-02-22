using FinanceTracker.Data;
using FinanceTracker.Services;
using FinanceTracker.Services.Interfaces;
using FinanceTracker.ViewModels;
using FinanceTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using FinanceTracker.Properties;
using System.Globalization;
using System.Windows;

namespace FinanceTracker;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplyLanguage();
        ApplyTheme();

        ConfigureServices();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ApplyLanguage()
    {
        var culture = new CultureInfo(Settings.Default.Language);

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private void ApplyTheme()
    {
        var theme = Settings.Default.Theme;

        var dict = new ResourceDictionary();

        dict.Source = theme == "Dark" ? new Uri("/Views/Themes/DarkTheme.xaml", UriKind.Relative) : new Uri("/Views/Themes/LightTheme.xaml", UriKind.Relative);

        Current.Resources.MergedDictionaries.Clear();
        Current.Resources.MergedDictionaries.Add(dict);
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<FinanceDbContext>();
        services.AddSingleton<IExpenseService, ExpenseService>();
        services.AddSingleton<IBudgetService, BudgetService>();
        services.AddSingleton<IChartService, ChartService>();
        services.AddSingleton<ICsvService, CsvService>();
        services.AddSingleton<IMessageService, MessageService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();
    }
}