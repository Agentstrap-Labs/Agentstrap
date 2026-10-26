namespace Agentstrap.Integrations
{
    /// <summary>
    /// Bridges the existing activity watcher's game join events into the push
    /// notification pipeline. Subscribes once per Roblox session, never spawns extra
    /// network work when the feature is off, and disposes cleanly.
    /// </summary>
    public sealed class ServerConnectionNotificationWatcher : IDisposable
    {
        private const string LOG_IDENT = "ServerConnectionNotificationWatcher";

        private readonly ActivityWatcher _activityWatcher;
        private readonly ServerConnectionNotificationService _service;
        private bool _disposed = false;

        public ServerConnectionNotificationWatcher(ActivityWatcher activityWatcher, ServerConnectionNotificationService? service = null)
        {
            _activityWatcher = activityWatcher;
            _service = service ?? new ServerConnectionNotificationService(CreateOptionsFromSettings());

            _activityWatcher.OnGameJoin += OnGameJoin;

            App.Logger.WriteLine(LOG_IDENT, "Server connection push notifications are active");
        }

        private static ServerConnectionNotificationOptions CreateOptionsFromSettings()
        {
            var options = new ServerConnectionNotificationOptions
            {
                Enabled = true,
                IncludeIpAddress = App.Settings.Prop.ServerConnectionNotificationIncludeIpAddress,
                IpInfoEnabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpInfo,
                IpWhoEnabled = !App.Settings.Prop.ServerConnectionNotificationDisableIpWho,
                Diagnostics = App.Settings.Prop.ServerConnectionNotificationDiagnostics,
                SuppressionMinutes = App.Settings.Prop.ServerConnectionNotificationSuppressionMinutes,
            };

            return options;
        }

        private void OnGameJoin(object? sender, EventArgs e)
        {
            const string LogIdent = "ServerConnectionNotificationWatcher::OnGameJoin";

            try
            {
                ActivityData data = _activityWatcher.Data;

                if (!OptionsEnabled(data))
                    return;

                // fire and forget: the notification pipeline is fully asynchronous and
                // contains its own failures, so the log reader never waits on it
                _ = _service.ProcessAddressAsync(data.MachineAddress, data.JobId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LogIdent, ex);
            }
        }

        private static bool OptionsEnabled(ActivityData data)
        {
            try
            {
                var options = App.Settings.Prop;
                return options.ServerConnectionNotificationsEnabled
                    && data.MachineAddressValid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _activityWatcher.OnGameJoin -= OnGameJoin;

            App.Logger.WriteLine(LOG_IDENT, "Disposing server connection push notification watcher");

            GC.SuppressFinalize(this);
        }
    }
}
