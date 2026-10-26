using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for GlobalSettingsPage.xaml
    /// </summary>
    public partial class GlobalSettingsPage
    {
        public GlobalSettingsPage()
        {
            DataContext = new GlobalSettingsViewModel();

            InitializeComponent();
            Loaded += (_, _) => ((GlobalSettingsViewModel)DataContext).AttachBorderlessStatusListener();
            Unloaded += (_, _) => ((GlobalSettingsViewModel)DataContext).DetachBorderlessStatusListener();
        }
    }
}
