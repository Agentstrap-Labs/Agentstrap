using System.Runtime.InteropServices;

namespace Agentstrap.Platform
{
    public static class PlatformInfo
    {
        public static PlatformKind Current
        {
            get
            {
                if (OperatingSystem.IsWindows()) return PlatformKind.Windows;
                if (OperatingSystem.IsMacOS()) return PlatformKind.MacOS;
                if (OperatingSystem.IsLinux()) return PlatformKind.Linux;
                return PlatformKind.Unknown;
            }
        }

        public static bool IsWindows => Current == PlatformKind.Windows;
        public static bool IsMacOS => Current == PlatformKind.MacOS;
        public static bool IsLinux => Current == PlatformKind.Linux;

        public static string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();

        public static bool IsAppleSilicon =>
            IsMacOS && RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64;

        public static string OSDescription => RuntimeInformation.OSDescription;

        public static string UserProfilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static string LocalAppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string ApplicationDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}