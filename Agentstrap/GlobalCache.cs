using Agentstrap.Models.APIs;

namespace Agentstrap
{
    public static class GlobalCache
    {
        /// <summary>
        /// Cached ipinfo.io records for queried Roblox server addresses.
        /// A null value means the address was already queried but no data could be retrieved.
        /// </summary>
        public static readonly Dictionary<string, IPInfoResponse?> ServerIpInfo = new();
    }
}
