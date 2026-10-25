using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Agentstrap.Integrations;

using Xunit;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Covers the ipinfo.io integration: response parsing, public/private address
    /// gating, IPv4/IPv6 handling, caching, malformed responses, HTTP errors and
    /// timeouts. All HTTP traffic is mocked - no real network access happens here.
    /// </summary>
    public class IpInfoClientTests
    {
        #region Helpers

        private static HttpClient ClientWith(HttpMockHandler handler) => new(handler);

        private static HttpResponseMessage JsonResponse(string json) =>
            new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        #endregion

        #region Successful response parsing

        [Fact]
        public async Task SuccessfulResponse_ContainsCityRegionAndCountry()
        {
            var handler = HttpMockHandler.Json("""
                {
                    "ip": "1.2.3.4",
                    "city": "Los Angeles",
                    "region": "California",
                    "country": "US",
                    "loc": "34.0522,-118.2437",
                    "postal": "90001",
                    "timezone": "America/Los_Angeles"
                }
                """);

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.2.3.4");

            Assert.True(result.Success, result.Error);
            Assert.NotNull(result.Response);
            Assert.Equal("Los Angeles", result.Response!.City);
            Assert.Equal("California", result.Response.Region);
            Assert.Equal("US", result.Response.Country);
            Assert.True(result.CityRegionAvailable);
        }

        [Fact]
        public async Task ResponseWithoutCityOrRegion_ReportsSuccessButNoRegionAvailable()
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.5", "country": "US" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.2.3.5");

            Assert.True(result.Success);
            Assert.Null(result.Response!.City);
            Assert.Null(result.Response.Region);
            Assert.False(result.CityRegionAvailable, "a plan without city/region data must not claim to have it");
        }

        [Fact]
        public async Task NullFieldValues_AreHandledGracefully()
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.6", "city": null, "region": null, "country": null }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.2.3.6");

            Assert.True(result.Success);
            Assert.Null(result.Response!.City);
        }

        [Fact]
        public async Task ResponseAddressEcho_IsNormalizedFromTheRequestedAddress()
        {
            var handler = HttpMockHandler.Json("""{ "city": "Chicago", "region": "Illinois", "country": "US" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.2.3.7");

            Assert.True(result.Success);
            Assert.Equal("1.2.3.7", result.Address);
            Assert.Equal("1.2.3.7", result.Response!.Ip);
        }
        #endregion

        #region Address validation

        [Theory]
        [InlineData("not-an-ip")]
        [InlineData("abc")]
        [InlineData("999.999.999.999")]
        [InlineData("1..2")]
        [InlineData("https://evil.example.com")]
        [InlineData("")]
        [InlineData(null)]
        public async Task InvalidIPAddresses_AreRejectedWithoutNetworkRequest(string? address)
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.1.1.1", "city": "Should", "region": "Not", "country": "BeReached" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync(address);

            Assert.False(result.Success);
            Assert.Equal(IPInfoLookupResult.ErrorCodes.InvalidIPAddress, result.ErrorCode);
            Assert.Equal(0, handler.Count);
        }

        [Theory]
        [InlineData("127.0.0.1")]
        [InlineData("10.0.0.1")]
        [InlineData("192.168.1.50")]
        [InlineData("172.16.1.2")]
        [InlineData("172.31.255.255")]
        [InlineData("169.254.1.1")]
        [InlineData("100.64.1.1")]
        [InlineData("224.0.0.1")]
        [InlineData("255.255.255.255")]
        [InlineData("0.0.0.0")]
        [InlineData("198.51.100.7")]
        [InlineData("203.0.113.9")]
        [InlineData("::1")]
        [InlineData("fe80::1")]
        [InlineData("fc00::1")]
        [InlineData("fd12:3456::1")]
        [InlineData("ff02::1")]
        [InlineData("2001:db8::1")]
        public async Task PrivateAndReservedAddresses_AreRejectedWithoutNetworkRequest(string address)
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.4", "city": "Should", "region": "Not", "country": "BeReached" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync(address);

            Assert.False(result.Success);
            Assert.Equal(IPInfoLookupResult.ErrorCodes.PrivateIPAddress, result.ErrorCode);
            Assert.Equal(0, handler.Count);
        }

        [Theory]
        [InlineData("8.8.8.8")]
        [InlineData("2606:4700:4700::1111")]
        public async Task PublicIPv4AndIPv6Addresses_AreAccepted(string address)
        {
            var handler = HttpMockHandler.Json("""{ "ip": "8.8.8.8", "city": "Mountain View", "region": "California", "country": "US" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync(address);

            Assert.True(result.Success, result.Error);
        }

        [Fact]
        public async Task IPv4MappedLoopback_IsTreatedAsLoopback()
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.4" }""");

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("::ffff:127.0.0.1");

            Assert.False(result.Success);
            Assert.Equal(0, handler.Count);
        }
        #endregion

        #region Errors, tokens and caching

        [Fact]
        public async Task InvalidToken_ReportsAuthenticationErrorWithoutEchoingTheToken()
        {
            var handler = HttpMockHandler.Json("""{ "error": { "title": "Rate limited", "message": "The token was invalid" } }""", HttpStatusCode.Forbidden);

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.1.1.1", "bad-token");

            Assert.False(result.Success);
            Assert.Equal(IPInfoLookupResult.ErrorCodes.AuthenticationError, result.ErrorCode);
            Assert.Equal(403, result.StatusCode);
            Assert.DoesNotContain("bad-token", result.Error);
        }

        [Fact]
        public async Task RateLimitedResponses_AreReported()
        {
            var handler = HttpMockHandler.Json("""{ "error": { "message": "Too many requests" } }""", HttpStatusCode.TooManyRequests);

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.1.1.1", "tok");

            Assert.False(result.Success);
            Assert.True(result.IsRateLimited);
        }

        [Fact]
        public async Task MalformedResponses_DoNotReportSuccess()
        {
            var bodies = new[]
            {
                "<html><body>blocked</body></html>",
                "{not json",
            };

            foreach (string body in bodies)
            {
                var handler = HttpMockHandler.Body("text/html", body);
                var client = new IpInfoClient(ClientWith(handler));

                var result = await client.LookupAsync("1.1.1.1");

                Assert.False(result.Success, $"body '{body}' wrongly reported success");
                Assert.Null(result.Response);
            }
        }

        [Fact]
        public async Task ServerErrors_AreReportedWithoutEchoingTheToken()
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.4" }""", HttpStatusCode.InternalServerError);

            var client = new IpInfoClient(ClientWith(handler));
            var result = await client.LookupAsync("1.1.1.1", "secret-token");

            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.DoesNotContain("secret-token", result.Error);
        }

        [Fact]
        public async Task NetworkFailures_AreReported()
        {
            var handler = new HttpMockHandler((_, _) => throw new HttpRequestException("connection refused"));
            var client = new IpInfoClient(new HttpClient(handler));

            var result = await client.LookupAsync("1.1.1.1");

            Assert.False(result.Success);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task Timeouts_AreReportedAsTimeouts()
        {
            var handler = HttpMockHandler.Delayed(TimeSpan.FromSeconds(2), JsonResponse("""{ "ip": "1.1.1.1", "city": "Chicago", "region": "Illinois", "country": "US" }"""));

            var client = new IpInfoClient(new HttpClient(handler), requestTimeout: TimeSpan.FromMilliseconds(75));
            var result = await client.LookupAsync("1.1.1.1");

            Assert.False(result.Success);
            Assert.Equal(IPInfoLookupResult.ErrorCodes.Timeout, result.ErrorCode);
        }

        [Fact]
        public async Task SuccessfulLookups_AreCached()
        {
            var handler = HttpMockHandler.Json("""{ "ip": "1.2.3.8", "city": "Austin", "region": "Texas", "country": "US" }""");

            var client = new IpInfoClient(ClientWith(handler));

            var first = await client.LookupAsync("1.2.3.8");
            var second = await client.LookupAsync("1.2.3.8");

            Assert.True(first.Success);
            Assert.True(second.Success);
            Assert.Equal(1, handler.Count);
        }

        [Fact]
        public async Task LookupResponseAsync_ReturnsNullOnFailure()
        {
            var client = new IpInfoClient(ClientWith(HttpMockHandler.Json("""{}""", HttpStatusCode.BadRequest)));

            Assert.Null(await client.LookupResponseAsync("1.1.1.1"));
        }

        [Fact]
        public void RequestUrl_UsesHttpsAndEncodesTheAddressAndToken()
        {
            string url = IpInfoClient.BuildRequestUrl("2606:4700:4700::1111", "t ok&=en");

            Assert.StartsWith("https://ipinfo.io/", url);
            Assert.Contains("%3A", url);
            Assert.Contains("token=t%20ok%26%3Den", url);
        }
        #endregion

        #region Response clamping

        [Fact]
        public async Task OversizedResponses_AreRejected()
        {
            string filler = new('x', 128 * 1024);
            var handler = HttpMockHandler.Json($$"""{ "ip": "1.2.3.4", "junk": "{{filler}}" }""");

            var client = new IpInfoClient(ClientWith(handler));

            var result = await client.LookupAsync("1.2.3.4");

            Assert.False(result.Success);
            Assert.Equal(IPInfoLookupResult.ErrorCodes.MalformedResponse, result.ErrorCode);
        }
        #endregion
    }
}
