using System.Windows;
using System.Windows.Controls;

using Agentstrap.Integrations;
using Agentstrap.UI.ViewModels.Settings;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for ServerConnectionNotificationsPage.xaml
    /// </summary>
    public partial class ServerConnectionNotificationsPage
    {
        private readonly ServerConnectionNotificationsViewModel _viewModel =
            new ServerConnectionNotificationsViewModel();

        private readonly ServerConnectionNotificationService _service = new();
        private readonly ServerConnectionNotificationCredentialStore _credentialStore =
            ServerConnectionNotificationCredentialStore.Default;

        public ServerConnectionNotificationsPage()
        {
            DataContext = _viewModel;

            InitializeComponent();

            LoadCredentialFields();
        }

        /// <summary>
        /// Fills the masked fields with what is currently configured. Complete secrets
        /// are never re-shown after they have been saved - the boxes are left empty and
        /// a "saved" marker is displayed instead, so a peeked-at settings window does
        /// not leak the token.
        /// </summary>
        private void LoadCredentialFields()
        {
            IpInfoTokenBox.Password = String.Empty;
            BarkPushKeyBox.Password = String.Empty;

            _viewModel.OnCredentialsChanged();
            UpdateFieldStates();
        }

        private void UpdateFieldStates() => _viewModel.OnCredentialsChanged();

        private void SaveCredentials_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "ServerConnectionNotificationsPage::SaveCredentials_Click";

            try
            {
                // blur-only secrets: an empty box keeps the previously saved value
                if (!String.IsNullOrWhiteSpace(IpInfoTokenBox.Password))
                    _credentialStore.IpInfoToken = IpInfoTokenBox.Password.Trim();

                if (!String.IsNullOrWhiteSpace(BarkPushKeyBox.Password))
                    _credentialStore.BarkPushKey = BarkPushKeyBox.Password.Trim();

                IpInfoTokenBox.Password = String.Empty;
                BarkPushKeyBox.Password = String.Empty;

                _viewModel.OnCredentialsChanged();
                UpdateFieldStates();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                _viewModel.SetTestStatus("The credentials could not be saved.");
            }
        }

        private void ClearCredentials_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "ServerConnectionNotificationsPage::ClearCredentials_Click";

            try
            {
                _credentialStore.Clear();

                IpInfoTokenBox.Password = String.Empty;
                BarkPushKeyBox.Password = String.Empty;

                TestIpInfoStatus.Text = String.Empty;
                TestNotificationStatus.Text = String.Empty;

                _viewModel.OnCredentialsChanged();
                UpdateFieldStates();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private async void TestIpInfo_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "ServerConnectionNotificationsPage::TestIpInfo_Click";

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

                var result = await _service.TestIpInfoAsync(token);

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
            const string LOG_IDENT = "ServerConnectionNotificationsPage::TestNotification_Click";

            var button = (System.Windows.Controls.Button)sender;

            try
            {
                button.IsEnabled = false;
                TestNotificationStatus.Text = "Sending test notification...";

                string? pushKey = String.IsNullOrWhiteSpace(BarkPushKeyBox.Password)
                    ? null
                    : BarkPushKeyBox.Password.Trim();
                var result = await _service.TestNotificationAsync(pushKey);

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
    }
}
