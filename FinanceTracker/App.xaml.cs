using FinanceTracker.Data;
using FinanceTracker.Services;
using FinanceTracker.Services.Interfaces;
using FinanceTracker.ViewModels;
using FinanceTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;

namespace FinanceTracker;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var culture = new CultureInfo("en-GB");

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        base.OnStartup(e);

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

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}