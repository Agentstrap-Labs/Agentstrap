using System.Collections.Concurrent;

namespace Agentstrap.Platform
{
    public static class PlatformCapabilities
    {
        private static readonly ConcurrentDictionary<string, PlatformCapability> _capabilities =
            new(StringComparer.OrdinalIgnoreCase);

        static PlatformCapabilities()
        {
            RegisterDefaults();
        }

        public static PlatformCapability Get(string name)
        {
            if (_capabilities.TryGetValue(name, out var capability))
                return capability;

            return new PlatformCapability
            {
                Name = name,
                Status = CapabilityStatus.Unsupported,
                Description = "Unknown capability.",
                Resolution = "This capability is not registered for the current platform."
            };
        }

        public static CapabilityStatus StatusOf(string name) => Get(name).Status;

        public static bool IsSupported(string name) => Get(name).Status == CapabilityStatus.Supported;

        public static bool IsAvailable(string name) => Get(name).IsAvailable;

        public static IReadOnlyDictionary<string, PlatformCapability> All => _capabilities;

        public static void Register(PlatformCapability capability)
        {
            if (capability is null) throw new ArgumentNullException(nameof(capability));
            if (string.IsNullOrWhiteSpace(capability.Name)) throw new ArgumentException("Capability name is required.", nameof(capability));
            _capabilities[capability.Name] = capability;
        }

        private static void RegisterDefaults()
        {
            bool windows = PlatformInfo.IsWindows;
            bool macos = PlatformInfo.IsMacOS;

            RegisterCap("ConfigurationManagement", windows ? CapabilityStatus.Supported : CapabilityStatus.Experimental,
                windows ? "Managed configuration and settings." : "Configuration management is experimental on this platform.",
                "Use the supported Windows build for full functionality.");

            RegisterCap("Profiles", CapabilityStatus.Supported, "Configuration profiles.", "");

            RegisterCap("Backups", CapabilityStatus.Supported, "Automatic and manual configuration backups.", "");

            RegisterCap("Extensions", CapabilityStatus.Supported, "Optional Agentstrap extensions with manifest validation.", "");

            RegisterCap("ModManagement", windows ? CapabilityStatus.Supported : CapabilityStatus.Experimental,
                windows ? "Windows mod management." : "Windows mods are not assumed compatible with macOS.",
                "Use Windows-compatible mods only.");

            RegisterCap("Diagnostics", CapabilityStatus.Supported, "Platform-aware diagnostics.", "");

            RegisterCap("UpdateReliability", CapabilityStatus.Supported, "Update validation and recovery.", "");

            RegisterCap("ThemeEditor", CapabilityStatus.Supported, "Theme customization.", "");

            RegisterCap("RobloxClientDetection", windows ? CapabilityStatus.Supported : CapabilityStatus.Experimental,
                windows ? "Detect installed Roblox client." : "Native Roblox macOS client detection is experimental.",
                "Install the native Roblox client.");

            RegisterCap("RobloxLaunchIntegration", windows ? CapabilityStatus.Supported : CapabilityStatus.Experimental,
                windows ? "Launch Roblox through Agentstrap." : "Launch integration on macOS is experimental and may be unsupported.",
                "Use documented, supported integration mechanisms only.");

            RegisterCap("FastFlags", windows ? CapabilityStatus.Supported : CapabilityStatus.Unsupported,
                windows ? "Fast Flag management." : "Windows-specific Fast Flags are not assumed to work on macOS.",
                "Not available on this platform.");

            RegisterCap("RegistrySettings", windows ? CapabilityStatus.Supported : CapabilityStatus.Unsupported,
                windows ? "Windows registry settings." : "Registry settings are unavailable on macOS.",
                "");

            RegisterCap("MacOSNativePackaging", macos ? CapabilityStatus.Supported : CapabilityStatus.Unsupported,
                macos ? "macOS application packaging." : "macOS packaging is only available when building on macOS.",
                "Build on macOS with the appropriate SDK.");

            RegisterCap("AppleSilicon", (macos && PlatformInfo.IsAppleSilicon) ? CapabilityStatus.Supported : (macos ? CapabilityStatus.Supported : CapabilityStatus.Unsupported),
                "Apple Silicon support.",
                "");
        }

        private static void RegisterCap(string name, CapabilityStatus status, string description, string resolution)
        {
            Register(new PlatformCapability { Name = name, Status = status, Description = description, Resolution = resolution });
        }
    }
}