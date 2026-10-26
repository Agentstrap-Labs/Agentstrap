using System.Text.Json;

using Agentstrap.Models.APIs;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Client for the IPWho geolocation service (https://ipwho.org).
    /// Used as a backup when the primary ipinfo.io lookup fails. An API key is
    /// required and is sent as the "apiKey" query parameter.
    /// </summary>
    public sealed class IpWhoClient
    {
        public const string DefaultBaseUrl = "https://api.ipwho.org";

        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(10);

        private readonly HttpClient _httpClient;
        private readonly TimeSpan _requestTimeout;

        public IpWhoClient(HttpClient? httpClient = null, TimeSpan? requestTimeout = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _requestTimeout = requestTimeout ?? DefaultRequestTimeout;
        }

        public static string BuildRequestUrl(string ipAddress, string apiKey) =>
            $"{DefaultBaseUrl}/ip/{Uri.EscapeDataString(ipAddress)}?apiKey={Uri.EscapeDataString(apiKey.Trim())}";

        public async Task<IPInfoLookupResult> LookupAsync(string? ipAddress, string? apiKey = null, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
                return IPInfoLookupResult.Failed(ipAddress, IPInfoLookupResult.ErrorCodes.AuthenticationError, "IPWho requires an API key");

            if (!PublicIPAddress.TryParse(ipAddress, out IPAddress address) || !PublicIPAddress.IsPublic(address))
                return IPInfoLookupResult.Failed(ipAddress, IPInfoLookupResult.ErrorCodes.InvalidIPAddress, "The address is not a public IP address");

            string canonical = address.ToString();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_requestTimeout);

            string url = BuildRequestUrl(canonical, apiKey);

            try
            {
                using var response = await _httpClient.GetAsync(url, timeout.Token);
                int status = (int)response.StatusCode;
                string body = await ReadBodyAsync(response, timeout.Token);

                if (!response.IsSuccessStatusCode)
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, $"IPWho returned HTTP {status} ({response.StatusCode})", status);

                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (!root.TryGetProperty("success", out var success) || !success.GetBoolean())
                {
                    string error = root.TryGetProperty("message", out var msg) ? (msg.GetString() ?? "IPWho lookup failed") : "IPWho lookup failed";
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, error, status);
                }

                if (!root.TryGetProperty("data", out var data))
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.MalformedResponse, "IPWho response missing data");

                var parsed = new IPInfoResponse
                {
                    Ip = canonical,
                    City = data.TryGetProperty("city", out var city) ? city.GetString() : null,
                    Region = data.TryGetProperty("region", out var region) ? region.GetString() : null,
                    Country = data.TryGetProperty("countryCode", out var country) ? country.GetString() : null,
                };

                return IPInfoLookupResult.Successful(canonical, parsed);
            }
            catch (Exception ex)
            {
                return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, $"IPWho request failed ({ex.Message})");
            }
        }

        private static async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content is null) return String.Empty;
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}