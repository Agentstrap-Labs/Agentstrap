namespace Agentstrap.Integrations
{
    /// <summary>
    /// The outcome of a Bark push request. A successful response means Bark accepted
    /// the message for delivery.
    /// </summary>
    public class BarkResult
    {
        public bool Success { get; set; }

        /// <summary>
        /// Optional request id for services that return one.
        /// </summary>
        public string? RequestId { get; set; }

        public int? StatusCode { get; set; }

        /// <summary>
        /// Stable machine friendly reason for a failure
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Human readable description of the failure, safe to show in the UI - it never
        /// includes the Bark device key.
        /// </summary>
        public string? Error { get; set; }

        public bool IsRateLimited => ErrorCode == ErrorCodes.RateLimited;

        public bool NotConfigured => ErrorCode == ErrorCodes.NotConfigured;

        public static BarkResult Successful(int? statusCode, string? requestId) => new()
        {
            Success = true,
            StatusCode = statusCode,
            RequestId = requestId,
        };

        public static BarkResult Failed(string errorCode, string error, int? statusCode = null) => new()
        {
            Success = false,
            ErrorCode = errorCode,
            Error = error,
            StatusCode = statusCode,
        };

        public sealed class ErrorCodes
        {
            public const string NotConfigured = "not_configured";
            public const string InvalidMessage = "invalid_message";
            public const string Timeout = "timeout";
            public const string Cancelled = "cancelled";
            public const string RateLimited = "rate_limited";
            public const string AuthenticationError = "authentication_error";
            public const string HttpError = "http_error";
            public const string InvalidResponse = "invalid_response";
            public const string MalformedResponse = "malformed_response";
        }
    }
}
