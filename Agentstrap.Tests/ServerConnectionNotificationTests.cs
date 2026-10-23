using System.Threading.Tasks;

using Agentstrap.Integrations;
using Agentstrap.Models.APIs;
using Agentstrap.Models.Entities;
using Agentstrap.UI.Elements.Overlay;

using Xunit;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Covers the logic behind the Roblox server connection notification: the required
    /// "City, State: Game name" text format and its fallbacks, connection event
    /// deduplication, stale lookup rejection, and server location parsing.
    /// None of these tests require a running Roblox client.
    /// </summary>
    public class ServerConnectionNotificationTests
    {
        #region Notification text format
        [Fact]
        public void NotificationTextShowsCityStateAndGameName()
        {
            string text = ServerConnectionNotifier.BuildNotificationText("Dallas, Texas", "Brookhaven RP");

            Assert.Equal("Dallas, Texas: Brookhaven RP", text);
        }

        [Fact]
        public void NotificationTextShowsSecondCityStateAndGameName()
        {
            string text = ServerConnectionNotifier.BuildNotificationText("Chicago, Illinois", "Adopt Me!");

            Assert.Equal("Chicago, Illinois: Adopt Me!", text);
        }

        [Fact]
        public void NotificationTextFallsBackToUnknownLocationWhenLocationMissing()
        {
            string text = ServerConnectionNotifier.BuildNotificationText(null, "Brookhaven RP");

            Assert.Equal("Unknown location: Brookhaven RP", text);
        }

        [Fact]
        public void NotificationTextFallsBackToUnknownLocationWhenLocationBlank()
        {
            string text = ServerConnectionNotifier.BuildNotificationText("   ", "Adopt Me!");

            Assert.Equal("Unknown location: Adopt Me!", text);
        }

        [Fact]
        public void NotificationTextFallsBackToRobloxWhenGameNameMissing()
        {
            string text = ServerConnectionNotifier.BuildNotificationText("Chicago, Illinois", null);

            Assert.Equal("Chicago, Illinois: Roblox", text);
        }

        [Fact]
        public void NotificationTextFallsBackToUnknownLocationAndRobloxWhenEverythingMissing()
        {
            string text = ServerConnectionNotifier.BuildNotificationText(null, null);

            Assert.Equal("Unknown location: Roblox", text);
        }

        [Fact]
        public void NotificationTextTrimsSurroundingWhitespace()
        {
            string text = ServerConnectionNotifier.BuildNotificationText("  Chicago, Illinois  ", " Adopt Me! ");

            Assert.Equal("Chicago, Illinois: Adopt Me!", text);
        }
        #endregion

        #region Connection tracking
        [Fact]
        public void FirstConnectionIsTracked()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out int generation));
            Assert.True(tracker.IsCurrent(generation));
        }

        [Fact]
        public void RepeatedEventForActiveServerIsIgnored()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out _));
            Assert.False(tracker.TryTrackConnection("job-1", "203.0.113.1", out _));
        }

        [Fact]
        public void ConnectingToDifferentServerIsTracked()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out _));

            // a teleport to another server is preceded by a leave event
            tracker.TrackLeave();

            Assert.True(tracker.TryTrackConnection("job-2", "203.0.113.2", out _));
        }

        [Fact]
        public void ReconnectingToSameServerAfterLeaveIsTracked()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out _));

            // reconnecting after a connection interruption is a genuine connection event,
            // even if it lands on the very same server
            tracker.TrackLeave();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out _));
        }

        [Fact]
        public void LeavingServerInvalidatesPendingLookups()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out int generation));
            Assert.True(tracker.IsCurrent(generation));

            tracker.TrackLeave();

            Assert.False(tracker.IsCurrent(generation));
        }

        [Fact]
        public void NewConnectionInvalidatesLookupsForPreviousServer()
        {
            var tracker = new ServerConnectionTracker();

            Assert.True(tracker.TryTrackConnection("job-1", "203.0.113.1", out int firstGeneration));

            tracker.TrackLeave();
            Assert.True(tracker.TryTrackConnection("job-2", "203.0.113.2", out int secondGeneration));

            // a late result from the first server must not overwrite the second server's notification
            Assert.False(tracker.IsCurrent(firstGeneration));
            Assert.True(tracker.IsCurrent(secondGeneration));
        }
        #endregion

        #region Server location
        [Fact]
        public async Task CityStateLocationIsParsedFromCachedServerRecord()
        {
            GlobalCache.ServerIpInfo["198.51.100.1"] = new IPInfoResponse
            {
                City = "Chicago",
                Region = "Illinois",
                Country = "US"
            };

            var data = new ActivityData { MachineAddress = "198.51.100.1" };

            Assert.Equal("Chicago, Illinois", await data.QueryServerCityState());
        }

        [Fact]
        public async Task FullServerLocationKeepsExistingFormat()
        {
            GlobalCache.ServerIpInfo["198.51.100.2"] = new IPInfoResponse
            {
                City = "Chicago",
                Region = "Illinois",
                Country = "US"
            };

            var data = new ActivityData { MachineAddress = "198.51.100.2" };

            // the tray notification and server details dialog format must stay as it was
            Assert.Equal("Chicago, Illinois, US", await data.QueryServerLocation());
        }

        [Fact]
        public async Task CityStateLocationIsNullWhenNoDistinctCityIsReported()
        {
            GlobalCache.ServerIpInfo["198.51.100.3"] = new IPInfoResponse
            {
                City = "Illinois",
                Region = "Illinois",
                Country = "US"
            };

            var data = new ActivityData { MachineAddress = "198.51.100.3" };

            Assert.Null(await data.QueryServerCityState());
        }

        [Fact]
        public async Task CityStateLocationIsNullWhenLookupFailed()
        {
            // a null cached record represents a previously failed lookup
            GlobalCache.ServerIpInfo["198.51.100.4"] = null;

            var data = new ActivityData { MachineAddress = "198.51.100.4" };

            Assert.Null(await data.QueryServerCityState());
            Assert.Null(await data.QueryServerLocation());
        }

        [Fact]
        public async Task CityStateLocationIsNullForPrivateServerAddress()
        {
            // private addresses are not publicly queryable - this must not hit the network
            var data = new ActivityData { MachineAddress = "10.0.0.5" };

            Assert.Null(await data.QueryServerCityState());
        }

        [Fact]
        public async Task CityStateLocationIsNullForEmptyAddress()
        {
            var data = new ActivityData { MachineAddress = "" };

            Assert.Null(await data.QueryServerCityState());
        }
        #endregion
    }
}
