using System.Collections.Concurrent;
using System.Text;

using Agentstrap.Models.APIs;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Client for the documented ipinfo.io address lookup endpoint
    /// (https://ipinfo.io/{ip}/json?token=...). Requests are always made over HTTPS
    /// with certificate validation enabled, the token is only ever passed as a query
    /// parameter of that single HTTPS request - never logged, never embedded anywhere else.
    ///
    /// Successful records and "known bad" failures are cached for a short while so
    /// reopening a server (or the overlay and push notifications asking for the same
    /// address) does not burn through the API quota.
    /// </summary>
    public sealed class IpInfoClient
    {
        public const string DefaultBaseUrl = "https://ipinfo.io";

        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(10);

        private static readonly TimeSpan SuccessCacheLifetime = TimeSpan.FromHours(6);
        private static readonly TimeSpan FailureCacheLifetime = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Upper bound for a single lookup response, well above any legitimate
        /// ipinfo.io record but small enough to stop a bogus server from flooding us.
        /// </summary>
        private const int MaxResponseBytes = 64 * 1024;

        /// <summary>
        /// A process wide client, shared by the server location overlay and the
        /// push notification feature so both benefit from the same cache.
        /// </summary>
        public static readonly IpInfoClient Shared = new();

        private readonly HttpClient _httpClient;
        private readonly TimeSpan _requestTimeout;

        private readonly ConcurrentDictionary<string, (DateTimeOffset ExpiresAt, IPInfoLookupResult Result)> _cache = new(StringComparer.OrdinalIgnoreCase);

        public IpInfoClient(HttpClient? httpClient = null, TimeSpan? requestTimeout = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _requestTimeout = requestTimeout ?? DefaultRequestTimeout;
        }

        /// <summary>
        /// Builds a documented ipinfo.io request URL. The IP address and token are
        /// URL encoded; the token must never be logged together with the URL.
        /// </summary>
        public static string BuildRequestUrl(string ipAddress, string? token = null)
        {
            var builder = new StringBuilder(DefaultBaseUrl);
            builder.Append('/');
            builder.Append(Uri.EscapeDataString(ipAddress));
            builder.Append("/json");

            if (!String.IsNullOrWhiteSpace(token))
                builder.Append("?token=").Append(Uri.EscapeDataString(token.Trim()));

            return builder.ToString();
        }

        /// <summary>
        /// Performs a lookup and returns the raw response, or null when the lookup
        /// failed for any reason (including the address not being public).
        /// </summary>
        public async Task<IPInfoResponse?> LookupResponseAsync(string? ipAddress, string? token = null, CancellationToken cancellationToken = default)
        {
            var result = await LookupAsync(ipAddress, token, cancellationToken);

            return result.Success ? result.Response : null;
        }

        /// <summary>
        /// Performs a lookup and returns a detailed result, distinguishing genuine
        /// success from refusals, timeouts, rate limits and unusable API plans.
        /// </summary>
        public async Task<IPInfoLookupResult> LookupAsync(string? ipAddress, string? token = null, CancellationToken cancellationToken = default)
        {
            string? input = ipAddress?.Trim();

            if (!PublicIPAddress.TryParse(input, out IPAddress address))
                return CacheMiss(input, input, IPInfoLookupResult.ErrorCodes.InvalidIPAddress, "The address is not a valid, publicly routable IP address");

            string canonical = address.ToString();

            if (!PublicIPAddress.IsPublic(address))
                return CacheMiss(canonical, canonical, IPInfoLookupResult.ErrorCodes.PrivateIPAddress, "The address is not a public IP address, so it cannot be geolocated");

            string cacheKey = CacheKey(canonical, token);
            if (_cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
                return cached.Result;

            IPInfoLookupResult result = await PerformRequestAsync(canonical, token, cancellationToken);

            _cache[cacheKey] = (
                result.Success ? DateTimeOffset.UtcNow + SuccessCacheLifetime : DateTimeOffset.UtcNow + FailureCacheLifetime,
                result
            );

            return result;
        }

        private async Task<IPInfoLookupResult> PerformRequestAsync(string canonical, string? token, CancellationToken cancellationToken)
        {
            const string LOG_IDENT = "IpInfoClient::PerformRequestAsync";

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_requestTimeout);

            string url = BuildRequestUrl(canonical, token);

            try
            {
                using var response = await _httpClient.GetAsync(url, timeout.Token);

                int status = (int)response.StatusCode;

                string body = await ReadBodyAsync(response, timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    string? message = ExtractErrorMessage(body) ?? $"IPinfo returned HTTP {status} ({response.StatusCode})";
                    string errorCode =
                        status == 429 ? IPInfoLookupResult.ErrorCodes.RateLimited :
                        status == 401 || status == 403 ? IPInfoLookupResult.ErrorCodes.AuthenticationError :
                        IPInfoLookupResult.ErrorCodes.HttpError;

                    return IPInfoLookupResult.Failed(canonical, errorCode, message, status);
                }

                using (var document = JsonDocument.Parse(body))
                {
                    var root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (TryExtractError(root, out string? apiError))
                            return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, apiError, status);

                        var parsed = JsonSerializer.Deserialize<IPInfoResponse>(root.GetRawText()) ?? new IPInfoResponse();

                        // the record's "ip" echo may be absent; the requested address is
                        // authoritative, so use it rather than leaving nothing behind
                        if (String.IsNullOrWhiteSpace(parsed.Ip))
                            parsed.Ip = canonical;

                        return IPInfoLookupResult.Successful(canonical, parsed);
                    }

                    throw new InvalidHTTPResponseException("Response was not a JSON object");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                if (ex is OperationCanceledException)
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.Timeout, "The IPinfo request timed out");

                if (ex is JsonException or InvalidHTTPResponseException)
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.MalformedResponse, "The IPinfo response could not be parsed");

                if (ex is HttpRequestException http)
                    return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, $"The IPinfo request failed ({http.StatusCode?.ToString() ?? "network error"})");

                App.Logger.WriteLine(LOG_IDENT, "IPinfo lookup failed");
                App.Logger.WriteException(LOG_IDENT, ex);

                return IPInfoLookupResult.Failed(canonical, IPInfoLookupResult.ErrorCodes.HttpError, "The IPinfo request failed");
            }
        }

        private static async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // stream and cap manually rather than trusting a bogus content length
            if (response.Content is null)
                throw new InvalidHTTPResponseException("Response had no content");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var buffer = new char[MaxResponseBytes];
            int read = await reader.ReadAsync(buffer, 0, buffer.Length);

            if (read == buffer.Length && reader.Peek() >= 0)
                throw new InvalidHTTPResponseException("Response exceeded the maximum accepted size");

            return new string(buffer, 0, read);
        }

        private static bool TryExtractError(JsonElement root, out string? error)
        {
            error = null;

            if (!root.TryGetProperty("error", out JsonElement errorElement))
                return false;

            if (errorElement.ValueKind == JsonValueKind.Object)
            {
                if (errorElement.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.String)
                    error = message.GetString();
                else if (errorElement.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String)
                    error = title.GetString();
            }
            else if (errorElement.ValueKind == JsonValueKind.String)
            {
                error = errorElement.GetString();
            }

            return !String.IsNullOrWhiteSpace(error);
        }

        private static string? ExtractErrorMessage(string body)
        {
            try
            {
                using var document = JsonDocument.Parse(body);

                return TryExtractError(document.RootElement, out string? error) ? error : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CacheKey(string canonical, string? token) =>
            String.IsNullOrWhiteSpace(token) ? canonical : $"{canonical}|{token.Trim()}";

        private static IPInfoLookupResult CacheMiss(string? input, string? address, string errorCode, string error) =>
            IPInfoLookupResult.Failed(address, errorCode, error);
    }
}
