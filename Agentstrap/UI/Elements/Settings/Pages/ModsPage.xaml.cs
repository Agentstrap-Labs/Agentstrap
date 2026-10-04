using System.IO;
using System.Windows;
using System.Windows.Media;

using Agentstrap.Utility;
using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage
    {
        private static readonly Brush DropIdleBrush =
            (Brush)Application.Current.FindResource("CardStrokeColorDefaultBrush");

        private static readonly Brush DropActiveBrush =
            (Brush)Application.Current.FindResource("ControlStrongStrokeColorDefaultBrush");

        private ModsViewModel? ViewModel;

        public ModsPage()
        {
            DataContext = new ModsViewModel();
            InitializeComponent();
        }

        private void SkyboxDropTarget_DragEnter(object sender, DragEventArgs e)
        {
            if (!HasUsableImage(e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Copy;
            SkyboxDropTarget.BorderBrush = DropActiveBrush;
            e.Handled = true;
        }

        private void SkyboxDropTarget_DragLeave(object sender, DragEventArgs e)
        {
            SkyboxDropTarget.BorderBrush = DropIdleBrush;
        }

        private void SkyboxDropTarget_Drop(object sender, DragEventArgs e)
        {
            SkyboxDropTarget.BorderBrush = DropIdleBrush;

            if (!HasUsableImage(e))
                return;

            ViewModel ??= DataContext as ModsViewModel;
            ViewModel?.ImportCustomSkybox(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
        }

        /// <summary>
        /// Only single decodable image files are accepted, so the drag never has to
        /// be rejected halfway through the conversion.
        /// </summary>
        private static bool HasUsableImage(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return false;

            return files.Length == 1 && SkyboxImageConverter.IsSupportedImage(files[0]);
        }
    }
}