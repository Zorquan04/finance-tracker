using FinanceTracker.Data;
using FinanceTracker.Properties;
using FinanceTracker.Services;
using FinanceTracker.Services.Interfaces;
using FinanceTracker.ViewModels;
using FinanceTracker.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;

namespace FinanceTracker;

// Main application class for WPF
public partial class App : Application
{
    // Global service provider for dependency injection
    public static IServiceProvider Services { get; private set; } = null!;

    // Entry point when the application starts
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplyLanguage();  // Set application language
        ApplyTheme();     // Apply selected theme (Light/Dark)

        ConfigureServices();  // Setup dependency injection

        // Resolve and show the main window
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    // Apply current language settings from user preferences
    private void ApplyLanguage()
    {
        var culture = new CultureInfo(Settings.Default.Language);

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    // Apply Light or Dark theme based on user preferences
    private void ApplyTheme()
    {
        var theme = Settings.Default.Theme;

        var dict = new ResourceDictionary();

        dict.Source = theme == "Dark" ? new Uri("/Views/Themes/DarkTheme.xaml", UriKind.Relative) : new Uri("/Views/Themes/LightTheme.xaml", UriKind.Relative);

        Current.Resources.MergedDictionaries.Clear();
        Current.Resources.MergedDictionaries.Add(dict);
    }

    // Setup all services and view models for dependency injection
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register database context and services
        services.AddSingleton<FinanceDbContext>();
        services.AddSingleton<IExpenseService, ExpenseService>();
        services.AddSingleton<IBudgetService, BudgetService>();
        services.AddSingleton<IChartService, ChartService>();
        services.AddSingleton<ICsvService, CsvService>();
        services.AddSingleton<IMessageService, MessageService>();

        // Register main view model and window
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        // Build the service provider
        Services = services.BuildServiceProvider();

        // Ensure the database is created and migrations are applied
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
            context.Database.Migrate();
        }
    }
}