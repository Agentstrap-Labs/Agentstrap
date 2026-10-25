using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Agentstrap.Integrations;

using Xunit;

namespace Agentstrap.Tests
{
    public class BarkClientTests
    {
        private const string DeviceKey = "bark-test-device-key";

        private static HttpClient ClientWith(HttpMockHandler handler) => new(handler);

        [Fact]
        public async Task SendsJsonToBarkEndpoint()
        {
            string body = String.Empty;
            var handler = new HttpMockHandler(async (request, cancellationToken) =>
            {
                body = await request.Content!.ReadAsStringAsync(cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{ "code": 200, "message": "success" }""", Encoding.UTF8, "application/json"),
                };
            });

            var result = await new BarkClient(ClientWith(handler)).SendAsync(DeviceKey, "Test title", "Test message");

            Assert.True(result.Success);
            Assert.Equal("https://api.day.app/" + DeviceKey, handler.Requests[0].RequestUri!.ToString());
            Assert.Contains("\"title\":\"Test title\"", body);
            Assert.Contains("\"body\":\"Test message\"", body);
        }

        [Fact]
        public async Task MissingDeviceKeyAndEmptyMessageAreRejected()
        {
            var handler = HttpMockHandler.Json("""{ "code": 200 }""");
            var client = new BarkClient(ClientWith(handler));

            Assert.True((await client.SendAsync(null, "title", "message")).NotConfigured);
            Assert.False((await client.SendAsync(DeviceKey, "title", " ")).Success);
            Assert.Equal(0, handler.Count);
        }

        [Fact]
        public async Task ApiFailuresAreReported()
        {
            var handler = HttpMockHandler.Json("""{ "code": 400, "message": "invalid key" }""", HttpStatusCode.BadRequest);
            var result = await new BarkClient(ClientWith(handler)).SendAsync(DeviceKey, "title", "message");

            Assert.False(result.Success);
            Assert.Contains("invalid key", result.Error);
        }
    }
}
