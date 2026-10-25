namespace Agentstrap.Models.APIs
{
    public class IPInfoResponse
    {
        /// <summary>
        /// The address the record is for. IPinfo normally echoes it back.
        /// </summary>
        [JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        /// <summary>
        /// IPinfo reports this as a two letter ISO 3166-1 country code.
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("loc")]
        public string? Location { get; set; }

        [JsonPropertyName("org")]
        public string? Organisation { get; set; }

        [JsonPropertyName("postal")]
        public string? Postal { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }
}
