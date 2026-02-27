using FinanceTracker.Resources;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace FinanceTracker.Views
{
    // Window showing information about the application
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent(); // Initialize XAML components
            SetVersionText();      // Display app version
        }

        // Set version text using assembly version
        private void SetVersionText()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"{AppResources.Title_Version} {version?.Major}.{version?.Minor}.{version?.Build}";
        }

        // Open GitHub page in default browser
        private void OpenGitHub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Zorquan04",
                UseShellExecute = true
            });
        }
    }
}