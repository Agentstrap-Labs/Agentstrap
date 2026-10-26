using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Agentstrap.Integrations;
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

        private readonly ServerConnectionNotificationService _notificationService = new();
        private readonly ServerConnectionNotificationCredentialStore _credentialStore =
            ServerConnectionNotificationCredentialStore.Default;

        public IntegrationsPage()
        {
            DataContext = new IntegrationsViewModel();
            InitializeComponent();

            IpInfoTokenBox.Password = String.Empty;
            BarkPushKeyBox.Password = String.Empty;
            IpWhoApiKeyBox.Password = String.Empty;

            _notificationService.Options.IpInfoEnabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpInfo;
            _notificationService.Options.IpWhoEnabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpWho;

            DisableIpInfoButton.Content = App.Settings.Prop.ServerConnectionNotificationDisableIpInfo ? "Enable IPinfo" : "Disable IPinfo";
            DisableIpWhoButton.Content = App.Settings.Prop.ServerConnectionNotificationDisableIpWho ? "Enable IPWho" : "Disable IPWho";

            var viewModel = (IntegrationsViewModel)DataContext;
            viewModel.ServerConnectionNotifications.OnCredentialsChanged();
            UpdateFieldStates();
        }

        public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
        {
            IntegrationsViewModel viewModel = (IntegrationsViewModel)DataContext;
            viewModel.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomIntegration));
        }

        private ServerConnectionNotificationsViewModel NotificationViewModel =>
            ((IntegrationsViewModel)DataContext).ServerConnectionNotifications;

        private void UpdateFieldStates() => NotificationViewModel.OnCredentialsChanged();

        private void SaveCredentials_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::SaveCredentials_Click";

            try
            {
                if (!String.IsNullOrWhiteSpace(IpInfoTokenBox.Password))
                    _credentialStore.IpInfoToken = IpInfoTokenBox.Password.Trim();

                if (!String.IsNullOrWhiteSpace(BarkPushKeyBox.Password))
                    _credentialStore.BarkPushKey = BarkPushKeyBox.Password.Trim();

                if (!String.IsNullOrWhiteSpace(IpWhoApiKeyBox.Password))
                    _credentialStore.IpWhoApiKey = IpWhoApiKeyBox.Password.Trim();

                IpInfoTokenBox.Password = String.Empty;
                BarkPushKeyBox.Password = String.Empty;
                IpWhoApiKeyBox.Password = String.Empty;

                NotificationViewModel.OnCredentialsChanged();
                UpdateFieldStates();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                NotificationViewModel.SetTestStatus("The credentials could not be saved.");
            }
        }

        private void ClearCredentials_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::ClearCredentials_Click";

            try
            {
                _credentialStore.Clear();

                IpInfoTokenBox.Password = String.Empty;
                BarkPushKeyBox.Password = String.Empty;
                IpWhoApiKeyBox.Password = String.Empty;

                TestIpInfoStatus.Text = String.Empty;
                TestNotificationStatus.Text = String.Empty;

                NotificationViewModel.OnCredentialsChanged();
                UpdateFieldStates();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private async void TestIpInfo_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::TestIpInfo_Click";

            var button = (System.Windows.Controls.Button)sender;
            string? token = null;

            try
            {
                button.IsEnabled = false;
                TestIpInfoStatus.Text = "Testing IPinfo...";

                if (!String.IsNullOrWhiteSpace(IpInfoTokenBox.Password))
                    token = IpInfoTokenBox.Password.Trim();
                else
                {
                    var credentials = _credentialStore.Load();
                    token = credentials.IpInfoToken;
                }

                var result = await _notificationService.TestIpInfoAsync(token);

                if (result.Success)
                {
                    if (result.CityRegionAvailable)
                    {
                        string? location = ServerConnectionMessageBuilder.BuildLocationText(result.Response);
                        TestIpInfoStatus.Text = $"IPinfo is working. The configured plan provides city and region data (test lookup: {location}).";
                    }
                    else
                    {
                        TestIpInfoStatus.Text = "The IPinfo token was accepted, but the plan's response did not include city and region fields. City and region data needs a plan that covers location details - notifications will still be sent, without a location.";
                    }
                }
                else
                {
                    TestIpInfoStatus.Text = $"IPinfo test failed: {result.Error}";
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                TestIpInfoStatus.Text = "IPinfo test failed unexpectedly.";
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::TestNotification_Click";

            var button = (System.Windows.Controls.Button)sender;

            try
            {
                button.IsEnabled = false;
                TestNotificationStatus.Text = "Sending test notification...";

                string? pushKey = String.IsNullOrWhiteSpace(BarkPushKeyBox.Password)
                    ? null
                    : BarkPushKeyBox.Password.Trim();
                var result = await _notificationService.TestNotificationAsync(pushKey);

                if (result.Success)
                    TestNotificationStatus.Text = "Bark accepted the test notification for delivery.";
                else if (result.NotConfigured)
                    TestNotificationStatus.Text = "Bark is not configured: save a push key first.";
                else
                    TestNotificationStatus.Text = $"Bark test failed: {result.Error}";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                TestNotificationStatus.Text = "The Bark test failed unexpectedly.";
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void TestIpWho_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "IntegrationsPage::TestIpWho_Click";

            var button = (System.Windows.Controls.Button)sender;

            try
            {
                button.IsEnabled = false;
                TestIpWhoStatus.Text = "Testing IPWho...";

                string? apiKey = String.IsNullOrWhiteSpace(IpWhoApiKeyBox.Password)
                    ? null
                    : IpWhoApiKeyBox.Password.Trim();

                var result = await _notificationService.TestIpWhoAsync(apiKey);

                if (result.Success)
                {
                    string? location = ServerConnectionMessageBuilder.BuildLocationText(result.Response);
                    TestIpWhoStatus.Text = $"IPWho is working. Test lookup: {location}.";
                }
                else
                {
                    TestIpWhoStatus.Text = $"IPWho test failed: {result.Error}";
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                TestIpWhoStatus.Text = "IPWho test failed unexpectedly.";
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private void DisableIpInfo_Click(object sender, RoutedEventArgs e)
        {
            bool disabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpInfo;
            App.Settings.Prop.ServerConnectionNotificationDisableIpInfo = disabled;
            App.Settings.Save();

            _notificationService.Options.IpInfoEnabled = !disabled;
            DisableIpInfoButton.Content = disabled ? "Enable IPinfo" : "Disable IPinfo";
        }

        private void DisableIpWho_Click(object sender, RoutedEventArgs e)
        {
            bool disabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpWho;
            App.Settings.Prop.ServerConnectionNotificationDisableIpWho = disabled;
            App.Settings.Save();

            _notificationService.Options.IpWhoEnabled = !disabled;
            DisableIpWhoButton.Content = disabled ? "Enable IPWho" : "Disable IPWho";
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