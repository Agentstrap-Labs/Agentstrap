using System.Windows;

using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage
    {
        public DashboardPage()
        {
            DashboardViewModel viewModel = new();

            viewModel.RequestNavigateEvent += (_, pageType) => NavigateTo(pageType);

            DataContext = viewModel;

            InitializeComponent();
        }

        private void NavigateTo(Type pageType)
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.Navigate(pageType);
        }
    }
}
