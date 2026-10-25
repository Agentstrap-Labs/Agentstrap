using Agentstrap.Models.APIs;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Builds the push notification text for a server connection from the actual
    /// IPinfo record. Location fields that IPinfo did not return are never invented;
    /// the message simply degrades to whatever the service was able to report.
    /// </summary>
    public static class ServerConnectionMessageBuilder
    {
        public const string NotificationTitle = "Agentstrap \u2022 Server Connection";

        public const string DefaultLocationlessMessage = "You have connected to a server!";

        /// <summary>
        /// Example full result: "You have connected to a server from California, Los Angeles, United States!"
        /// </summary>
        public static string BuildMessage(IPInfoResponse? response, bool includeIpAddress = false, string? ipAddress = null)
        {
            string? location = BuildLocationText(response);

            string message = String.IsNullOrWhiteSpace(location)
                ? DefaultLocationlessMessage
                : $"You have connected to a server from {location}!";

            if (includeIpAddress && !String.IsNullOrWhiteSpace(ipAddress))
                message += $" Remote IP address: {ipAddress.Trim()}.";

            return message;
        }

        /// <summary>
        /// Builds the location as "Region, City, Country" using only fields that were
        /// actually returned. IPinfo sometimes reports the region name as the city when
        /// it has nothing more precise - that duplicate is dropped instead of printed twice.
        /// </summary>
        public static string? BuildLocationText(IPInfoResponse? response)
        {
            if (response is null)
                return null;

            List<string> parts = new();

            string? region = Clean(response.Region);
            string? city = Clean(response.City);
            string? country = Clean(CountryNameResolver.GetCountryName(response.Country) ?? response.Country);

            if (!String.IsNullOrWhiteSpace(region))
                parts.Add(region);

            if (!String.IsNullOrWhiteSpace(city) && !String.Equals(city, region, StringComparison.OrdinalIgnoreCase))
                parts.Add(city);

            if (!String.IsNullOrWhiteSpace(country))
                parts.Add(country);

            return parts.Count == 0 ? null : String.Join(", ", parts);
        }

        private static string? Clean(string? value)
        {
            string? trimmed = value?.Trim();

            return String.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
