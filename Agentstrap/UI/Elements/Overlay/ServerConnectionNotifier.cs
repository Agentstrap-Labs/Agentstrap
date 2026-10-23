using Agentstrap.Integrations;

namespace Agentstrap.UI.Elements.Overlay
{
    /// <summary>
    /// Listens to the existing activity watcher's game join/leave events and shows the
    /// server connection notification overlay whenever the player connects to a server.
    /// </summary>
    public class ServerConnectionNotifier : IDisposable
    {
        /// <summary>
        /// Used when the experience name can't be determined from any metadata.
        /// </summary>
        private const string FallbackExperienceName = "Roblox";

        private readonly ActivityWatcher _activityWatcher;

        private readonly ServerConnectionTracker _tracker = new();

        private readonly ServerConnectionOverlay _overlay;

        private readonly int _processId;

        private bool _disposed = false;

        public ServerConnectionNotifier(ActivityWatcher activityWatcher, int processId)
        {
            const string LOG_IDENT = "ServerConnectionNotifier";

            _activityWatcher = activityWatcher;
            _processId = processId;

            _overlay = new ServerConnectionOverlay();

            _activityWatcher.OnGameJoin += ActivityWatcher_OnGameJoin;
            _activityWatcher.OnGameLeave += ActivityWatcher_OnGameLeave;

            App.Logger.WriteLine(LOG_IDENT, $"Initialized for Roblox process {processId}");
        }

        /// <summary>
        /// Builds the required "City, State: Game name" notification text, falling back
        /// gracefully when either piece of metadata is unavailable.
        /// </summary>
        public static string BuildNotificationText(string? serverLocation, string? experienceName)
        {
            string location = String.IsNullOrWhiteSpace(serverLocation)
                ? Strings.ServerConnectionOverlay_UnknownLocation
                : serverLocation.Trim();

            string experience = String.IsNullOrWhiteSpace(experienceName)
                ? FallbackExperienceName
                : experienceName.Trim();

            return $"{location}: {experience}";
        }

        /// <summary>
        /// Note: this is raised from a background thread, as the activity watcher runs
        /// outside of the UI thread. All UI work is marshalled onto the dispatcher.
        /// </summary>
        private async void ActivityWatcher_OnGameJoin(object? sender, EventArgs e)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::ActivityWatcher_OnGameJoin";

            try
            {
                // the activity data instance is replaced once the game is left, so grabbing
                // a reference here captures the details of this specific connection
                ActivityData data = _activityWatcher.Data;

                if (!_tracker.TryTrackConnection(data.JobId, data.MachineAddress, out int generation))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Ignoring repeated connection event for server {data}");
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, $"Connection event received for server {data}");

                // The notification has to be visible the moment the player joins, so it is
                // shown straight away with whatever is already known locally. The city/state
                // and the experience name are resolved afterwards and patched into the same
                // notification: waiting for those lookups first could hold the notification
                // back for as long as the HTTP timeout, which made it look like it never
                // appeared at all.
                string immediateText = BuildNotificationText(null, GetCachedExperienceName(data));

                ShowOnDispatcher(generation, immediateText);

                Task<string?> experienceNameTask = ResolveExperienceNameAsync(data);
                Task<string?> serverLocationTask = ResolveServerLocationAsync(data);

                await Task.WhenAll(experienceNameTask, serverLocationTask);

                string enrichedText = BuildNotificationText(await serverLocationTask, await experienceNameTask);

                if (enrichedText == immediateText)
                    return;

                _ = _overlay.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // the user may have left this server, or connected to another one,
                    // while our lookups were still in flight - never show stale data
                    if (_disposed || !_tracker.IsCurrent(generation))
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Discarding stale connection notification update");
                        return;
                    }

                    _overlay.UpdateNotification(enrichedText);
                }));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// Shows the notification on the UI thread, discarding it if it has been
        /// superseded by a newer connection in the meantime.
        /// </summary>
        private void ShowOnDispatcher(int generation, string text)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::ShowOnDispatcher";

            _ = _overlay.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed || !_tracker.IsCurrent(generation))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Discarding stale connection notification");
                    return;
                }

                _overlay.ShowNotification(text, _processId, App.Settings.Prop.ServerConnectionOverlayDuration);
            }));
        }

        /// <summary>
        /// Returns the experience name if it is already in the local cache, so the
        /// notification can show something useful without waiting on any lookup.
        /// </summary>
        private static string? GetCachedExperienceName(ActivityData data)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::GetCachedExperienceName";

            if (data.UniverseId == 0)
                return null;

            try
            {
                return UniverseDetails.LoadFromCache(data.UniverseId)?.Data?.Name;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to read cached experience details");
                App.Logger.WriteException(LOG_IDENT, ex);

                return null;
            }
        }

        private void ActivityWatcher_OnGameLeave(object? sender, EventArgs e)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::ActivityWatcher_OnGameLeave";

            App.Logger.WriteLine(LOG_IDENT, "Game leave event received, hiding notification");

            _tracker.TrackLeave();

            _ = _overlay.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_disposed)
                    _overlay.HideNotification();
            }));
        }

        /// <summary>
        /// Resolves the name of the active experience, preferring locally cached details.
        /// Returns null if the name can't be determined.
        /// </summary>
        private static async Task<string?> ResolveExperienceNameAsync(ActivityData data)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::ResolveExperienceNameAsync";

            try
            {
                long universeId = data.UniverseId;

                // the universe id is logged shortly before the connection is confirmed, but
                // the ordering isn't guaranteed, so give it a moment to show up
                for (int i = 0; universeId == 0 && i < 15; i++)
                {
                    await Task.Delay(200);
                    universeId = data.UniverseId;
                }

                if (universeId == 0)
                    return null;

                if (UniverseDetails.LoadFromCache(universeId) is null)
                    await UniverseDetails.FetchSingle(universeId);

                return UniverseDetails.LoadFromCache(universeId)?.Data?.Name;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to get experience details");
                App.Logger.WriteException(LOG_IDENT, ex);

                return null;
            }
        }

        /// <summary>
        /// Resolves the "City, State" location of the server. Returns null if it can't
        /// be determined reliably.
        /// </summary>
        private static async Task<string?> ResolveServerLocationAsync(ActivityData data)
        {
            const string LOG_IDENT = "ServerConnectionNotifier::ResolveServerLocationAsync";

            try
            {
                if (!data.MachineAddressValid)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Server address '{data.MachineAddress}' is not publicly queryable");
                    return null;
                }

                return await data.QueryServerCityState();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to query server location");
                App.Logger.WriteException(LOG_IDENT, ex);

                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            App.Logger.WriteLine("ServerConnectionNotifier::Dispose", "Disposing server connection notifier");

            _activityWatcher.OnGameJoin -= ActivityWatcher_OnGameJoin;
            _activityWatcher.OnGameLeave -= ActivityWatcher_OnGameLeave;

            _overlay.Shutdown();

            GC.SuppressFinalize(this);
        }
    }
}
