using System.Windows.Input;

using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for QoLPage.xaml
    /// </summary>
    public partial class QoLPage
    {
        public QoLPage()
        {
            DataContext = new QoLViewModel();

            InitializeComponent();
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) =>
            e.Handled = e.Text != "-" && !Int32.TryParse(e.Text, out int _);

        private void ValidateUInt32(object sender, TextCompositionEventArgs e) =>
            e.Handled = !UInt32.TryParse(e.Text, out uint _);
    }
}
