using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for AppleEmojiPage.xaml
    /// </summary>
    public partial class AppleEmojiPage
    {
        public AppleEmojiPage()
        {
            DataContext = new AppleEmojiViewModel();

            InitializeComponent();
        }
    }
}