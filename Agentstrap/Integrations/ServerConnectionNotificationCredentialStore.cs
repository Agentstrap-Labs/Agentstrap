namespace Agentstrap.Integrations
{
    /// <summary>
    /// A snapshot of the credentials needed to send server connection notifications.
    /// Values live only in memory while a lookup or delivery is in progress.
    /// </summary>
    public sealed class ServerConnectionNotificationCredentials
    {
        public string? IpInfoToken { get; set; }

        public string? BarkPushKey { get; set; }

        public bool HasIpInfoToken => !String.IsNullOrWhiteSpace(IpInfoToken);

        public bool HasBarkCredentials => !String.IsNullOrWhiteSpace(BarkPushKey);

        public void Clear()
        {
            IpInfoToken = null;
            BarkPushKey = null;
        }
    }

    /// <summary>
    /// Persists the notification credentials (IPinfo token and Bark push key) using
    /// Windows DPAPI, protected for the current Windows user.
    ///
    /// The Bark push key is protected with Windows DPAPI for the current user.
    /// </summary>
    public sealed class ServerConnectionNotificationCredentialStore
    {
        public const string IpInfoTokenKey = "ipinfo-token";
        public const string BarkPushKeyName = "bark-push-key";

        private static readonly string DefaultDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            App.ProjectName,
            "Notifications",
            "Credentials"
        );

        public static readonly ServerConnectionNotificationCredentialStore Default = new();

        public SecureCredentialStore Credentials { get; }

        public ServerConnectionNotificationCredentialStore(string? directory = null)
        {
            Credentials = new SecureCredentialStore(directory ?? DefaultDirectory);
        }

        public string? IpInfoToken
        {
            get => Credentials.Load(IpInfoTokenKey);
            set => SetSecret(IpInfoTokenKey, value);
        }

        public string? BarkPushKey
        {
            get => Credentials.Load(BarkPushKeyName);
            set => SetSecret(BarkPushKeyName, value);
        }

        public ServerConnectionNotificationCredentials Load() => new()
        {
            IpInfoToken = IpInfoToken,
            BarkPushKey = this.BarkPushKey,
        };

        public void Save(ServerConnectionNotificationCredentials credentials)
        {
            IpInfoToken = credentials?.IpInfoToken;
            BarkPushKey = credentials?.BarkPushKey;
        }

        /// <summary>
        /// Removes every notification credential; used by the "clear" actions in the
        /// settings interface.
        /// </summary>
        public void Clear() => Credentials.Clear();

        private void SetSecret(string key, string? secret)
        {
            if (String.IsNullOrWhiteSpace(secret))
            {
                Credentials.Remove(key);
                return;
            }

            Credentials.Save(key, secret);
        }
    }
}
