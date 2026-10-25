using Agentstrap.Models.APIs;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// The outcome of a single ipinfo.io lookup. <see cref="Success"/> only becomes true
    /// when IPinfo answered with a usable record - a request that timed out, was refused,
    /// or could not be parsed never reports success.
    /// </summary>
    public class IPInfoLookupResult
    {
        /// <summary>
        /// The canonical address that was looked up, or the input when the address
        /// was rejected before any request was made.
        /// </summary>
        public string? Address { get; set; }

        public IPInfoResponse? Response { get; set; }

        public bool Success { get; set; }

        /// <summary>
        /// Whether the record contains enough data to state a "region, city, country"
        /// location. IPinfo plans without location data return a well formed record
        /// that simply has no city or region fields - that distinction is surfaced here
        /// so the settings interface can ask for a suitable plan instead of guessing.
        /// </summary>
        public bool CityRegionAvailable =>
            Success && !String.IsNullOrWhiteSpace(Response?.Region) && !String.IsNullOrWhiteSpace(Response?.City);

        /// <summary>
        /// Stable, machine friendly reason for a failure, e.g. "invalid_ip" or "http_401".
        /// Never contains credentials or the full request URL.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Human readable description of the failure, safe to display in the UI.
        /// </summary>
        public string? Error { get; set; }

        public int? StatusCode { get; set; }

        public bool IsRateLimited => ErrorCode == ErrorCodes.RateLimited;

        public static IPInfoLookupResult Successful(string? address, IPInfoResponse? response) => new()
        {
            Address = address,
            Response = response,
            Success = true,
        };

        public static IPInfoLookupResult Failed(string? address, string? errorCode, string? error, int? statusCode = null) => new()
        {
            Address = address,
            Success = false,
            ErrorCode = errorCode,
            Error = error,
            StatusCode = statusCode,
        };

        public sealed class ErrorCodes
        {
            public const string InvalidIPAddress = "invalid_ip";
            public const string PrivateIPAddress = "private_ip";
            public const string Timeout = "timeout";
            public const string Cancelled = "cancelled";
            public const string HttpError = "http_error";
            public const string RateLimited = "rate_limited";
            public const string AuthenticationError = "authentication_error";
            public const string MalformedResponse = "malformed_response";
            public const string ResponseTooLarge = "response_too_large";
        }
    }
}
