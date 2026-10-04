using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DiagnosticsPage.xaml
    /// </summary>
    public partial class DiagnosticsPage
    {
        public DiagnosticsPage()
        {
            DataContext = new DiagnosticsViewModel();

            InitializeComponent();
        }
    }
}
