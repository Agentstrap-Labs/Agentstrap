using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for InstallPage.xaml
    /// </summary>
    public partial class InstallPage
    {
        public InstallPage()
        {
            DataContext = new InstallViewModel();

            InitializeComponent();
        }
    }
}