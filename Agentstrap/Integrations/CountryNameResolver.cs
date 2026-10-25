using System.Collections.Concurrent;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Turns the two letter ISO country codes IPinfo returns into readable country
    /// names (via the .NET region database). A code that cannot be resolved is never
    /// invented into something else - the caller falls back to showing the code as-is.
    /// </summary>
    public static class CountryNameResolver
    {
        private static readonly ConcurrentDictionary<string, string?> Cache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the readable name for an ISO 3166-1 alpha-2 country code,
        /// or null when no reliable name can be determined.
        /// </summary>
        public static string? GetCountryName(string? countryCode)
        {
            if (String.IsNullOrWhiteSpace(countryCode))
                return null;

            string code = countryCode.Trim();

            if (!Regex.IsMatch(code, @"^[A-Za-z]{2}$"))
                return null;

            return Cache.GetOrAdd(code, static key =>
            {
                try
                {
                    // RegionInfo resolves every officially assigned ISO code; the
                    // invariant EnglishName keeps the notification language stable
                    var region = new RegionInfo(key);

                    return String.IsNullOrWhiteSpace(region.EnglishName) ? null : region.EnglishName;
                }
                catch (Exception)
                {
                    // user assigned / unassigned codes ("XK" and friends) have no mapping
                    return null;
                }
            });
        }
    }
}
