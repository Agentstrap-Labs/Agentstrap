using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage
    {
        public UpdatePage()
        {
            DataContext = new UpdateViewModel();

            InitializeComponent();
        }
    }
}
