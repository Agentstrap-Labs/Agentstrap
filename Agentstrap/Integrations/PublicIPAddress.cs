using System.Buffers.Binary;
using System.Net.Sockets;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Validates whether an address is a public, globally routable IP address
    /// that is suitable for an external geolocation lookup.
    ///
    /// Loopback, private, link local, CGNAT, multicast, documentation and other
    /// reserved ranges are always rejected: they either do not identify a remote
    /// server, or IPinfo cannot return a meaningful location for them.
    /// </summary>
    public static class PublicIPAddress
    {
        public static bool TryParse(string? text, out IPAddress address)
        {
            // the out parameter must be assigned for all code paths
            address = IPAddress.None;

            if (String.IsNullOrWhiteSpace(text))
                return false;

            // literals such as "2130706433" or "0x7f000001" parse on some runtimes;
            // require the textual form so nobody can smuggle numeric aliases through
            if (text.Any(c => !Uri.IsHexDigit(c) && c != '.' && c != ':' && c != '%'))
                return false;

            if (!IPAddress.TryParse(text.Trim(), out IPAddress? parsed) || parsed is null)
                return false;

            // scope/zone ids ("%eth0") never denote a routable remote endpoint
            if (parsed.IsIPv6LinkLocal && text.Contains('%', StringComparison.Ordinal))
                return false;

            // normalize "::ffff:a.b.c.d" mappings, so a v4-mapped loopback is punished
            address = parsed.IsIPv4MappedToIPv6 ? parsed.MapToIPv4() : parsed;

            return true;
        }

        public static bool IsPublic(string? text) => TryParse(text, out IPAddress address) && IsPublic(address);

        public static bool IsPublic(IPAddress address)
        {
            if (address is null)
                return false;

            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            if (IPAddress.IsLoopback(address))
                return false;

            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IsPublicIPv4(BinaryPrimitives.ReadUInt32BigEndian(address.GetAddressBytes()));

                case AddressFamily.InterNetworkV6:
                    return IsPublicIPv6(address.GetAddressBytes());

                default:
                    return false;
            }
        }

        private static bool IsPublicIPv4(uint value)
        {
            // https://www.iana.org/assignments/iana-ipv4-special-registry/
            return !InSubnet(value, 0x00000000u, 8)      // 0.0.0.0/8      this network
                && !InSubnet(value, 0x0A000000u, 8)      // 10.0.0.0/8     private
                && !InSubnet(value, 0x64400000u, 10)     // 100.64.0.0/10  CGNAT
                && !InSubnet(value, 0x7F000000u, 8)      // 127.0.0.0/8    loopback
                && !InSubnet(value, 0xA9FE0000u, 16)     // 169.254.0.0/16 link local
                && !InSubnet(value, 0xAC100000u, 12)     // 172.16.0.0/12  private
                && !InSubnet(value, 0xC0000000u, 24)     // 192.0.0.0/24   IETF assignments
                && !InSubnet(value, 0xC0000200u, 24)     // 192.0.2.0/24   documentation
                && !InSubnet(value, 0xC0586300u, 24)     // 192.88.99.0/24 6to4 relay anycast
                && !InSubnet(value, 0xC0A80000u, 16)     // 192.168.0.0/16 private
                && !InSubnet(value, 0xC6120000u, 15)     // 198.18.0.0/15  benchmarking
                && !InSubnet(value, 0xC6336400u, 24)     // 198.51.100.0/24 documentation
                && !InSubnet(value, 0xCB007100u, 24)     // 203.0.113.0/24 documentation
                && !InSubnet(value, 0xE0000000u, 4)      // 224.0.0.0/4    multicast
                && !InSubnet(value, 0xF0000000u, 4);     // 240.0.0.0/4    reserved/broadcast
        }

        private static bool IsPublicIPv6(byte[] bytes)
        {
            // https://www.iana.org/assignments/iana-ipv6-special-registry/
            if (bytes.Length != 16)
                return false;

            if (bytes.All(b => b == 0))                          // ::            unspecified
                return false;

            if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8)
                return false;                                    // 2001:db8::/32 documentation

            if ((bytes[0] & 0xFE) == 0xFC)                       // fc00::/7      unique local
                return false;

            if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)   // fe80::/10     link local
                return false;

            if (bytes[0] == 0xFF)                                // ff00::/8      multicast
                return false;

            return true;
        }

        private static bool InSubnet(uint value, uint network, int prefixLength)
        {
            int shift = 32 - Math.Clamp(prefixLength, 0, 32);
            return (value >> shift) == (network >> shift);
        }
    }
}
