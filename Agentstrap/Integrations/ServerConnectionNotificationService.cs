namespace Agentstrap.Integrations
{
    /// <summary>
    /// Turns a detected server connection into an IPinfo lookup and a Bark push
    /// notification. Everything runs asynchronously (never blocking the bootstrapper
    /// or the UI), all failures are contained, and no step touches the network until
    /// the enablement, eligibility, credential and suppression checks have passed - so
    /// disabling the feature stops both the geolocation requests and the messages.
    /// </summary>
    public sealed class ServerConnectionNotificationService
    {
        public const string LOG_IDENT = "ServerConnectionNotificationService";

        /// <summary>
        /// Well known public address used by the settings interface to verify that the
        /// IPinfo token is accepted by the service. It is a remote (non-loopback)
        /// address on purpose: the feature never uses the "/me" self endpoint.
        /// </summary>
        public const string TestLookupAddress = "1.1.1.1";

        public const string TestNotificationMessage = "This is a test notification from Agentstrap.";

        public ServerConnectionNotificationOptions Options { get; }

        private readonly ServerConnectionNotificationCredentialStore _credentialStore;
        private readonly IpInfoClient _ipInfoClient;
        private readonly IpWhoClient _ipWhoClient;
        private readonly BarkClient _barkClient;
        private readonly NotificationRateLimiter _rateLimiter;

        public ServerConnectionNotificationService(
            ServerConnectionNotificationOptions? options = null,
            ServerConnectionNotificationCredentialStore? credentialStore = null,
            IpInfoClient? ipInfoClient = null,
            IpWhoClient? ipWhoClient = null,
            BarkClient? barkClient = null,
            NotificationRateLimiter? rateLimiter = null)
        {
            Options = options ?? new ServerConnectionNotificationOptions();
            _credentialStore = credentialStore ?? ServerConnectionNotificationCredentialStore.Default;
            _ipInfoClient = ipInfoClient ?? IpInfoClient.Shared;
            _ipWhoClient = ipWhoClient ?? new IpWhoClient();
            _barkClient = barkClient ?? BarkClient.Shared;
            _rateLimiter = rateLimiter ?? NotificationRateLimiter.FromMinutes(Options.SuppressionMinutes);
        }

        /// <summary>
        /// Processes one server connection. <paramref name="jobId"/> is informational
        /// only; suppression is keyed on the endpoint address.
        /// </summary>
        public async Task<ServerConnectionNotificationResult> ProcessAddressAsync(string? ipAddress, string? jobId = null, CancellationToken cancellationToken = default)
        {
            const string LogIdent = "ServerConnectionNotificationService::ProcessAddressAsync";

            if (!Options.Enabled)
                return ServerConnectionNotificationResult.Skipped(ServerConnectionNotificationStatus.Disabled, ipAddress);

            if (!PublicIPAddress.TryParse(ipAddress, out IPAddress address) || !PublicIPAddress.IsPublic(address))
            {
                // loopback, private, reserved and malformed addresses can never be
                // geolocated - and they must never be sent to IPinfo at all
                App.Logger.WriteLine(LogIdent, "Connection address is not a public IP, skipping notification");
                return ServerConnectionNotificationResult.Skipped(ServerConnectionNotificationStatus.AddressIneligible, ipAddress);
            }

            string canonical = address.ToString();

            if (!_rateLimiter.ShouldSend(canonical))
            {
                App.Logger.WriteLine(LogIdent, $"{DescribeAddress(canonical)} was already notified about recently, suppressing duplicate notification");
                return ServerConnectionNotificationResult.Skipped(ServerConnectionNotificationStatus.Suppressed, canonical);
            }

            var credentials = _credentialStore.Load();

            if (!credentials.HasBarkCredentials)
            {
                App.Logger.WriteLine(LogIdent, "Bark credentials are not configured, skipping notification");
                return ServerConnectionNotificationResult.Skipped(ServerConnectionNotificationStatus.NotConfigured, canonical);
            }

            IPInfoLookupResult lookup;

            if (Options.IpInfoEnabled)
            {
                lookup = await _ipInfoClient.LookupAsync(canonical, credentials.IpInfoToken, cancellationToken);
            }
            else
            {
                lookup = IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, "IPinfo is disabled");
            }

            // fall back to IPWho when the primary lookup fails (or is disabled) and a key is configured
            if (!lookup.Success && Options.IpWhoEnabled && !String.IsNullOrWhiteSpace(credentials.IpWhoApiKey))
            {
                var backup = await _ipWhoClient.LookupAsync(canonical, credentials.IpWhoApiKey, cancellationToken);
                if (backup.Success)
                    lookup = backup;
            }

            // a failed lookup never blocks the notification entirely and never pretends
            // that a location was found - the message simply degrades to what is known
            string message = ServerConnectionMessageBuilder.BuildMessage(lookup.Response, Options.IncludeIpAddress, canonical);

            if (!lookup.Success)
            {
                App.Logger.WriteLine(
                    LogIdent,
                    $"IPinfo lookup for {DescribeAddress(canonical)} failed ({lookup.ErrorCode}), sending the notification without location details");
            }

            BarkResult bark = await _barkClient.SendAsync(
                credentials.BarkPushKey,
                ServerConnectionMessageBuilder.NotificationTitle,
                message,
                cancellationToken
            );

            if (!bark.Success)
            {
                App.Logger.WriteLine(LogIdent, $"Bark delivery failed: {bark.Error}");
                return new ServerConnectionNotificationResult
                {
                    Address = canonical,
                    Status = ServerConnectionNotificationStatus.SendFailed,
                    Message = message,
                    Error = bark.Error,
                    IpInfo = lookup,
                    Bark = bark,
                };
            }

            _rateLimiter.MarkSent(canonical);

            App.Logger.WriteLine(LogIdent, $"{DescribeAddress(canonical)} push notification accepted by Bark");

            return new ServerConnectionNotificationResult
            {
                Address = canonical,
                Status = ServerConnectionNotificationStatus.Sent,
                Message = message,
                IpInfo = lookup,
                Bark = bark,
            };
        }

        /// <summary>
        /// Verifies the IPinfo token (or a candidate one from the settings interface)
        /// against a fixed, well known public address.
        /// </summary>
        public Task<IPInfoLookupResult> TestIpInfoAsync(string? token = null, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            return _ipInfoClient.LookupAsync(
                String.IsNullOrWhiteSpace(ipAddress) ? TestLookupAddress : ipAddress,
                token,
                cancellationToken
            );
        }

        /// <summary>
        /// Sends a test notification through Bark. Success means the API
        /// accepted the request for delivery.
        /// </summary>
        public Task<BarkResult> TestNotificationAsync(string? pushKey = null, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(pushKey))
            {
                var credentials = _credentialStore.Load();
                pushKey = credentials.BarkPushKey;
            }

            return _barkClient.SendAsync(
                pushKey,
                ServerConnectionMessageBuilder.NotificationTitle,
                TestNotificationMessage,
                cancellationToken
            );
        }

        /// <summary>
        /// Verifies an IPWho API key (or a candidate one from the settings interface)
        /// against a fixed, well known public address.
        /// </summary>
        public Task<IPInfoLookupResult> TestIpWhoAsync(string? apiKey = null, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            string address = String.IsNullOrWhiteSpace(ipAddress) ? TestLookupAddress : ipAddress!;

            if (String.IsNullOrWhiteSpace(apiKey))
            {
                var credentials = _credentialStore.Load();
                apiKey = credentials.IpWhoApiKey;
            }

            return _ipWhoClient.LookupAsync(address, apiKey, cancellationToken);
        }

        private string DescribeAddress(string address)
        {
            // full addresses are only written to the log when diagnostics are
            // explicitly enabled, so ordinary logs carry no connection history
            return Options.Diagnostics ? address : "connection endpoint";
        }
    }
}
