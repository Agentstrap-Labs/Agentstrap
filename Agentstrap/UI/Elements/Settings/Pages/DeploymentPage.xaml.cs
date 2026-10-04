using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DeploymentPage.xaml
    /// </summary>
    public partial class DeploymentPage
    {
        public DeploymentPage()
        {
            DataContext = new DeploymentViewModel();

            InitializeComponent();
        }
    }
}
