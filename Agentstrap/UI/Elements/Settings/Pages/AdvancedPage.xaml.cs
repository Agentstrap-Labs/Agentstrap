using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for AdvancedPage.xaml
    /// </summary>
    public partial class AdvancedPage
    {
        public AdvancedPage()
        {
            DataContext = new AdvancedViewModel();

            InitializeComponent();
        }
    }
}
