namespace Agentstrap.Integrations
{
    public enum ServerConnectionNotificationStatus
    {
        /// <summary>The notification was sent to Bark successfully.</summary>
        Sent,

        /// <summary>The feature is turned off in settings.</summary>
        Disabled,

        /// <summary>The address is not a public IP and cannot be geolocated.</summary>
        AddressIneligible,

        /// <summary>A notification for this endpoint was already sent within the suppression window.</summary>
        Suppressed,

        /// <summary>Required Bark credentials are missing.</summary>
        NotConfigured,

        /// <summary>IPinfo could not provide a usable record.</summary>
        LookupFailed,

        /// <summary>Bark did not accept the notification.</summary>
        SendFailed,
    }

    /// <summary>
    /// The outcome of processing one server connection. Never claims success that did
    /// not actually happen.
    /// </summary>
    public sealed class ServerConnectionNotificationResult
    {
        public string? Address { get; set; }

        public ServerConnectionNotificationStatus Status { get; set; }

        /// <summary>The text that was (or would have been) sent as the notification body.</summary>
        public string? Message { get; set; }

        public string? Error { get; set; }

        public IPInfoLookupResult? IpInfo { get; set; }

        public BarkResult? Bark { get; set; }

        public bool Success => Status == ServerConnectionNotificationStatus.Sent;

        public static ServerConnectionNotificationResult Skipped(ServerConnectionNotificationStatus status, string? address = null) => new()
        {
            Address = address,
            Status = status,
        };
    }

    /// <summary>
    /// Configurable knobs for the server connection notification pipeline. Secrets are
    /// supplied through <see cref="ServerConnectionNotificationCredentialStore"/>, not here.
    /// </summary>
    public sealed class ServerConnectionNotificationOptions
    {
        public bool Enabled { get; set; } = false;

        public bool IpInfoEnabled { get; set; } = true;

        public bool IpWhoEnabled { get; set; } = true;

        public bool IncludeIpAddress { get; set; } = false;

        /// <summary>
        /// Enables logging of full server IP addresses. Off by default so ordinary
        /// logs contain no connection history.
        /// </summary>
        public bool Diagnostics { get; set; } = false;

        /// <summary>
        /// How long a second notification for the same endpoint is suppressed, in minutes.
        /// </summary>
        public int SuppressionMinutes { get; set; } = 30;

    }
}
