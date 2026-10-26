using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Text.Json;

using Agentstrap.Integrations;
using Agentstrap.Models.APIs;
using Agentstrap.Models.Persistable;

using Xunit;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Covers the server connection push notification pipeline end to end with mocked
    /// HTTP: eligibility, suppression, credential gates, honest success reporting and
    /// the notification text format, plus a check that no secret ever reaches the
    /// Agentstrap log without diagnostics enabled.
    /// </summary>
    public class ServerConnectionNotificationFeatureTests
    {
        private static string NewTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "AgentstrapNotificationFeatureTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static HttpClient ClientWith(HttpMockHandler handler) => new(handler);

        private static ServerConnectionNotificationCredentialStore FreshStore(bool withCredentials = true)
        {
            var store = new ServerConnectionNotificationCredentialStore(NewTempDirectory());

            if (withCredentials)
            {
                store.IpInfoToken = "notification-test-ipinfo-token";
                store.BarkPushKey = "notification-test-push-key";
            }

            return store;
        }

        #region Notification message formatting

        [Fact]
        public void MessageUsesRegionCityAndCountryWhenAllArePresent()
        {
            var response = new IPInfoResponse
            {
                Region = "California",
                City = "Los Angeles",
                Country = "US",
            };

            string message = ServerConnectionMessageBuilder.BuildMessage(response);

            Assert.Equal("You have connected to a server from California, Los Angeles, United States!", message);
        }

        [Fact]
        public void MessageDropsTheCityWhenItEqualsTheRegion()
        {
            // ipinfo reports the region name as the city when it has nothing more precise
            var response = new IPInfoResponse
            {
                Region = "Illinois",
                City = "Illinois",
                Country = "US",
            };

            string message = ServerConnectionMessageBuilder.BuildMessage(response);

            Assert.Equal("You have connected to a server from Illinois, United States!", message);
        }

        [Fact]
        public void MessageDegradesGracefullyWhenFieldsAreMissing()
        {
            var onlyCountry = new IPInfoResponse { Country = "DE" };
            Assert.Equal("You have connected to a server from Germany!", ServerConnectionMessageBuilder.BuildMessage(onlyCountry));

            var onlyRegion = new IPInfoResponse { Region = "California" };
            Assert.Equal("You have connected to a server from California!", ServerConnectionMessageBuilder.BuildMessage(onlyRegion));

            var nothing = new IPInfoResponse();
            Assert.Equal("You have connected to a server!", ServerConnectionMessageBuilder.BuildMessage(nothing));

            Assert.Equal("You have connected to a server!", ServerConnectionMessageBuilder.BuildMessage(null));
        }

        [Fact]
        public void MessageIncludesTheRemoteIpOnlyWhenRequested()
        {
            var response = new IPInfoResponse
            {
                Region = "California",
                City = "Los Angeles",
                Country = "US",
            };

            string withoutIp = ServerConnectionMessageBuilder.BuildMessage(response, includeIpAddress: false, ipAddress: "1.2.3.4");
            string withIp = ServerConnectionMessageBuilder.BuildMessage(response, includeIpAddress: true, ipAddress: "1.2.3.4");

            Assert.Equal("You have connected to a server from California, Los Angeles, United States!", withoutIp);
            Assert.Contains("Remote IP address: 1.2.3.4", withIp);
        }

        [Fact]
        public void ResolvedCountryNamesAreReadable()
        {
            Assert.Equal("United States", ServerConnectionMessageBuilder.BuildLocationText(new IPInfoResponse { Country = "US" }));

            // an unreadable input is never turned into an invented country name
            Assert.Null(CountryNameResolver.GetCountryName(null));
            Assert.Null(CountryNameResolver.GetCountryName(""));
            Assert.Null(CountryNameResolver.GetCountryName("123"));
        }
        #endregion

        #region Pipeline behaviour

        private static ServerConnectionNotificationService CreateService(
            bool enabled = true,
            ServerConnectionNotificationCredentialStore? store = null,
            IpInfoClient? ipInfoClient = null,
            BarkClient? barkClient = null,
            NotificationRateLimiter? rateLimiter = null,
            bool includeIpAddress = false,
            bool diagnostics = false,
            int suppressionMinutes = 30)
        {
            var options = new ServerConnectionNotificationOptions
            {
                Enabled = enabled,
                SuppressionMinutes = suppressionMinutes,
                IncludeIpAddress = includeIpAddress,
                Diagnostics = diagnostics,
            };

            return new ServerConnectionNotificationService(options, store ?? FreshStore(), ipInfoClient, null, barkClient, rateLimiter);
        }

        private static IpInfoClient LookupReturning(string city = "Los Angeles", string region = "California", string country = "US") =>
            new(ClientWith(HttpMockHandler.Json($$"""
                {
                    "ip": "1.2.3.4",
                    "city": "{{city}}",
                    "region": "{{region}}",
                    "country": "{{country}}"
                }
                """)));

        private static BarkClient BarkSuccess() =>
            new(ClientWith(HttpMockHandler.Json("""{ "code": 200, "message": "success" }""")));

        private static BarkClient BarkFailure(HttpStatusCode statusCode = HttpStatusCode.BadRequest, string body = """{ "code": 400, "message": "invalid key" }""") =>
            new(ClientWith(HttpMockHandler.Json(body, statusCode)));

        private static string BarkField(string body, string field) =>
            JsonDocument.Parse(body).RootElement.GetProperty(field).GetString() ?? String.Empty;

        [Fact]
        public async Task DisabledFeatureSendsNothingAndLooksUpNothing()
        {
            var ipHandler = HttpMockHandler.Json("""{ "ip": "1.2.3.4" }""");
            var barkHandler = HttpMockHandler.Json("""{ "status": 1 }""");

            var service = CreateService(
                enabled: false,
                store: FreshStore(),
                ipInfoClient: new IpInfoClient(ClientWith(ipHandler)),
                barkClient: new BarkClient(ClientWith(barkHandler)));

            var result = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.False(result.Success);
            Assert.Equal(ServerConnectionNotificationStatus.Disabled, result.Status);
            Assert.Equal(0, ipHandler.Count);
            Assert.Equal(0, barkHandler.Count);
        }

        [Fact]
        public async Task IneligibleAddressesSkipTheEntirePipeline()
        {
            var ipHandler = HttpMockHandler.Json("""{ "ip": "1.2.3.4" }""");
            var barkHandler = HttpMockHandler.Json("""{ "status": 1 }""");

            var service = CreateService(ipInfoClient: new IpInfoClient(ClientWith(ipHandler)), barkClient: new BarkClient(ClientWith(barkHandler)));

            string?[] addresses = { "127.0.0.1", "10.0.0.5", "192.168.1.2", "not-an-ip", "", null };

            foreach (string? address in addresses)
            {
                var result = await service.ProcessAddressAsync(address, "job-1");

                Assert.Equal(ServerConnectionNotificationStatus.AddressIneligible, result.Status);
            }

            Assert.Equal(0, ipHandler.Count);
            Assert.Equal(0, barkHandler.Count);
        }

        [Fact]
        public async Task SuccessfulPipelineSendsTheExpectedNotification()
        {
            string barkTitle = "";
            string barkMessage = "";
            var barkHandler = new HttpMockHandler(async (request, token) =>
            {
                string body = await request.Content!.ReadAsStringAsync(token);
                barkTitle = BarkField(body, "title");
                barkMessage = BarkField(body, "body");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{ "code": 200, "message": "success" }""", Encoding.UTF8, "application/json")
                };
            });

            var service = CreateService(ipInfoClient: LookupReturning(), barkClient: new BarkClient(new HttpClient(barkHandler)));

            var result = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.True(result.Success, result.Error);
            Assert.Equal(ServerConnectionNotificationStatus.Sent, result.Status);
            Assert.Equal(ServerConnectionMessageBuilder.NotificationTitle, barkTitle);
            Assert.Equal("You have connected to a server from California, Los Angeles, United States!", barkMessage);
            Assert.NotNull(result.IpInfo);
            Assert.NotNull(result.Bark);
        }

        [Fact]
        public async Task LookupFailuresNeverProduceFalseSuccess()
        {
            var service = CreateService(
                ipInfoClient: new IpInfoClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.InternalServerError))),
                barkClient: BarkSuccess());

            var result = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            // the notification was delivered, but the record must not pretend the
            // geolocation lookup succeeded - the message carries no invented location
            Assert.True(result.Success);
            Assert.False(result.IpInfo!.Success);
            Assert.Equal("You have connected to a server!", result.Message);
        }

        [Fact]
        public async Task BarkFailuresAreReportedHonestly()
        {
            var service = CreateService(ipInfoClient: LookupReturning(), barkClient: BarkFailure());

            var result = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.False(result.Success);
            Assert.Equal(ServerConnectionNotificationStatus.SendFailed, result.Status);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task MissingBarkCredentialsAreReportedAsNotConfigured()
        {
            var ipHandler = HttpMockHandler.Json("""{ "ip": "1.2.3.4" }""");
            var barkHandler = HttpMockHandler.Json("""{ "status": 1 }""");

            var service = CreateService(store: FreshStore(withCredentials: false), ipInfoClient: new IpInfoClient(ClientWith(ipHandler)), barkClient: new BarkClient(ClientWith(barkHandler)));

            var result = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.Equal(ServerConnectionNotificationStatus.NotConfigured, result.Status);
            Assert.Equal(0, ipHandler.Count);
            Assert.Equal(0, barkHandler.Count);
        }

        [Fact]
        public async Task RepeatConnectionsWithinTheWindowAreSuppressed()
        {
            var pushHandler = HttpMockHandler.Json("""{ "code": 200, "message": "success" }""");
            var service = CreateService(
                ipInfoClient: LookupReturning(),
                barkClient: new BarkClient(ClientWith(pushHandler)),
                suppressionMinutes: 30);

            var first = await service.ProcessAddressAsync("1.2.3.4", "job-1");
            var second = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.True(first.Success, first.Error ?? first.Status.ToString());
            Assert.Equal(ServerConnectionNotificationStatus.Suppressed, second.Status);
            Assert.False(second.Success);
            Assert.Equal(1, pushHandler.Count);
        }

        [Fact]
        public async Task SuppressionWindowOnlyExpiresAfterTheConfiguredInterval()
        {
            var pushHandler = HttpMockHandler.Json("""{ "code": 200, "message": "success" }""");
            DateTimeOffset now = DateTimeOffset.UtcNow;

            var limiter = NotificationRateLimiter.FromMinutes(10, () => now);

            var service = CreateService(ipInfoClient: LookupReturning(), barkClient: new BarkClient(ClientWith(pushHandler)), rateLimiter: limiter);

            var first = await service.ProcessAddressAsync("1.2.3.4", "job-1");
            Assert.True(first.Success, first.Error ?? first.Status.ToString());

            now += TimeSpan.FromMinutes(9);
            var third = await service.ProcessAddressAsync("1.2.3.4", "job-2");
            Assert.Equal(ServerConnectionNotificationStatus.Suppressed, third.Status);

            now += TimeSpan.FromMinutes(1);
            var fourth = await service.ProcessAddressAsync("1.2.3.4", "job-3");
            Assert.True(fourth.Success);

            Assert.Equal(2, pushHandler.Count);
        }

        [Fact]
        public async Task FailedNotificationsAreNotMarkedAsSent()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var limiter = NotificationRateLimiter.FromMinutes(10, () => now);

            var service = CreateService(
                ipInfoClient: LookupReturning(),
                barkClient: BarkFailure(),
                rateLimiter: limiter);

            var first = await service.ProcessAddressAsync("1.2.3.4", "job-1");
            now += TimeSpan.FromSeconds(5);
            var second = await service.ProcessAddressAsync("1.2.3.4", "job-1");

            Assert.False(first.Success);
            Assert.False(second.Success);
            Assert.NotEqual(ServerConnectionNotificationStatus.Suppressed, second.Status);
        }

        [Fact]
        public void TestLookupUsesAFixedPublicAddressNeverTheUsersOwnIp()
        {
            // guards against accidentally switching over to the "/me" endpoint: the
            // documented test address is a literal, well known public IP address
            Assert.Equal("1.1.1.1", ServerConnectionNotificationService.TestLookupAddress);
        }
        #endregion

        #region Logs never leak credentials or full IP addresses

        [Fact]
        public async Task StandardLogsNeverContainTokensUserKeysOrFullIps()
        {
            string logToken = "super-secret-ipinfo-token";
            string logPushKey = "super-secret-bark-push-key";
            string fullIp = "203.44.99.77";

            var store = new ServerConnectionNotificationCredentialStore(NewTempDirectory());
            store.IpInfoToken = logToken;
            store.BarkPushKey = logPushKey;

            int historyBefore = App.Logger.History.Count;

            var service = CreateService(
                ipInfoClient: new IpInfoClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.Forbidden))),
                barkClient: new BarkClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.Forbidden))),
                store: store,
                diagnostics: false);

            await service.ProcessAddressAsync(fullIp, "job-log");

            string logText = String.Join('\n', App.Logger.History.Skip(historyBefore));

            Assert.DoesNotContain(logToken, logText);
            Assert.DoesNotContain(logToken, logText);
            Assert.DoesNotContain(logPushKey, logText);
            Assert.DoesNotContain(fullIp, logText);

            // diagnostics explicitly enable full IP logging
            var diagnosticService = CreateService(
                ipInfoClient: new IpInfoClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.Forbidden))),
                barkClient: new BarkClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.Forbidden))),
                store: store,
                diagnostics: true);

            int historyAfterFirstPass = App.Logger.History.Count;

            await diagnosticService.ProcessAddressAsync(fullIp, "job-log-2");

            string logAfterDiagnostics = String.Join('\n', App.Logger.History.Skip(historyAfterFirstPass));
            Assert.Contains(fullIp, logAfterDiagnostics);
        }
        #endregion

        #region Settings integration

        [Fact]
        public void SettingsExposeTheNotificationOptions()
        {
            var settings = new Settings();

            settings.ServerConnectionNotificationsEnabled = true;
            settings.ServerConnectionNotificationIncludeIpAddress = true;
            settings.ServerConnectionNotificationSuppressionMinutes = 7;

            Assert.True(settings.ServerConnectionNotificationsEnabled);
            Assert.True(settings.ServerConnectionNotificationIncludeIpAddress);
            Assert.False(settings.ServerConnectionNotificationDiagnostics);
        }
        #endregion
    }
}
