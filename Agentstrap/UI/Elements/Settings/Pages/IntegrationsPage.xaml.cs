using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Agentstrap.UI.Elements.Overlay;
using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for IntegrationsPage.xaml
    /// </summary>
    public partial class IntegrationsPage
    {
        private ServerConnectionOverlay? _previewOverlay;

        public IntegrationsPage()
        {
            DataContext = new IntegrationsViewModel();
            InitializeComponent();
        }

        public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
        {
            IntegrationsViewModel viewModel = (IntegrationsViewModel)DataContext;
            viewModel.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomIntegration));
        }

        /// <summary>
        /// Shows the server connection notification over this window, so the overlay can
        /// be verified without having to join a game. This window is always used as the
        /// target: it is the one the user is looking at, so the result is guaranteed to be
        /// visible. Targeting the Roblox window instead would place the toast directly
        /// above Roblox - i.e. behind this window - and nothing would appear to happen.
        /// </summary>
        private void PreviewOverlay_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::PreviewOverlay_Click";

            try
            {
                Window? window = Window.GetWindow(this) ?? Application.Current?.MainWindow;

                if (window is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Could not resolve a window to show the preview over");
                    return;
                }

                IntPtr target = new WindowInteropHelper(window).Handle;

                if (target == IntPtr.Zero)
                {
                    App.Logger.WriteLine(LOG_IDENT, "The settings window has no native handle yet, skipping the preview");
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, $"Showing preview notification over window 0x{target.ToInt64():X}");

                _previewOverlay?.Shutdown();

                _previewOverlay = new ServerConnectionOverlay
                {
                    TargetWindowOverride = target
                };

                _previewOverlay.ShowNotification(
                    ServerConnectionNotifier.BuildNotificationText(null, null),
                    Environment.ProcessId,
                    App.Settings.Prop.ServerConnectionOverlayDuration);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
