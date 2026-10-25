namespace Agentstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Backs the "Server Connection Notifications" settings page. Credentials are
    /// handled by <see cref="Agentstrap.Integrations.ServerConnectionNotificationCredentialStore"/>
    /// (DPAPI protected) - they never appear in the saved settings file or in logs;
    /// this viewmodel only shows masked presence indicators.
    /// </summary>
    public class ServerConnectionNotificationsViewModel : NotifyPropertyChangedViewModel
    {
        public const string LOG_IDENT = "ServerConnectionNotificationsViewModel";

        private readonly Integrations.ServerConnectionNotificationCredentialStore _credentialStore =
            Integrations.ServerConnectionNotificationCredentialStore.Default;

        public Integrations.ServerConnectionNotificationCredentialStore CredentialStore => _credentialStore;

        private string _barkPushKey = String.Empty;
        private string _pushKeySavedHint = String.Empty;

        public string BarkPushKey
        {
            get => _barkPushKey;
            set
            {
                _barkPushKey = value ?? String.Empty;
                OnPropertyChanged(nameof(BarkPushKey));
            }
        }

        public string PushKeySavedHint
        {
            get => _pushKeySavedHint;
            private set
            {
                _pushKeySavedHint = value;
                OnPropertyChanged(nameof(PushKeySavedHint));
            }
        }

        private string _lastTestStatus = String.Empty;

        public bool Enabled
        {
            get => App.Settings.Prop.ServerConnectionNotificationsEnabled;
            set { App.Settings.Prop.ServerConnectionNotificationsEnabled = value; OnPropertyChanged(nameof(Enabled)); }
        }

        public bool IncludeIpAddress
        {
            get => App.Settings.Prop.ServerConnectionNotificationIncludeIpAddress;
            set { App.Settings.Prop.ServerConnectionNotificationIncludeIpAddress = value; OnPropertyChanged(nameof(IncludeIpAddress)); }
        }

        public bool Diagnostics
        {
            get => App.Settings.Prop.ServerConnectionNotificationDiagnostics;
            set { App.Settings.Prop.ServerConnectionNotificationDiagnostics = value; OnPropertyChanged(nameof(Diagnostics)); }
        }

        public int SuppressionMinutes
        {
            get => App.Settings.Prop.ServerConnectionNotificationSuppressionMinutes;
            set { App.Settings.Prop.ServerConnectionNotificationSuppressionMinutes = Math.Clamp(value, 0, 1440); OnPropertyChanged(nameof(SuppressionMinutes)); }
        }

        /// <summary>
        /// Status line describing which parts of the feature are configured. Never
        /// shows a complete secret - only whether something is set and, after a test,
        /// what actually happened.
        /// </summary>
        public string StatusText
        {
            get
            {
                try
                {
                    var credentials = _credentialStore.Load();

                    string bark;

                    if (credentials.HasBarkCredentials)
                    {
                        bark = "Bark: configured";
                    }
                    else
                    {
                        bark = "Bark: push key missing";
                    }

                    string ipinfo = credentials.HasIpInfoToken
                        ? "IPinfo: token saved"
                        : "IPinfo: no token set (lookups run on the public quota and may be rate limited)";

                    string status = String.IsNullOrWhiteSpace(_lastTestStatus)
                        ? ""
                        : _lastTestStatus;

                    return $"{ipinfo}\n{bark}" + (String.IsNullOrWhiteSpace(status) ? "" : $"\n{status}");
                }
                catch (Exception)
                {
                    return "The notification credentials could not be read.";
                }
            }
        }

        public void SetTestStatus(string status)
        {
            _lastTestStatus = status ?? String.Empty;
            OnPropertyChanged(nameof(StatusText));
        }

        public void OnCredentialsChanged()
        {
            _lastTestStatus = String.Empty;
            OnPropertyChanged(nameof(StatusText));

            try
            {
                bool saved = !String.IsNullOrWhiteSpace(_credentialStore.BarkPushKey);
                PushKeySavedHint = saved ? "Push key saved." : "No Push key saved.";
                BarkPushKey = String.Empty;
            }
            catch (Exception)
            {
                PushKeySavedHint = String.Empty;
            }
        }
    }
}
