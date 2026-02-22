using FinanceTracker.Resources;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace FinanceTracker.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            SetVersionText();
        }

        private void SetVersionText()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"{AppResources.Title_Version} {version?.Major}.{version?.Minor}.{version?.Build}";
        }

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