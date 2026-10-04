namespace Agentstrap.Utility
{
    /// <summary>
    /// A single entry in the installed mods manifest.
    /// </summary>
    public class InstalledMod
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "0.0.0";
        public string? UpdateUrl { get; set; } = null;
        public string? ReleaseApiUrl { get; set; } = null;
        public string? LocalPath { get; set; } = null;
    }

    /// <summary>
    /// A mod that has an available update.
    /// </summary>
    public class ModUpdateInfo
    {
        public string Name => Mod.Name;
        public string InstalledVersion => Mod.Version;
        public string LatestVersion => Latest;
        public InstalledMod Mod { get; init; } = new();
        public string Latest { get; init; } = "0.0.0";
    }

    /// <summary>
    /// Reads the installed mods manifest and reports which mods have newer
    /// versions available.
    /// </summary>
    /// <remarks>
    /// The manifest lives at <c>Modifications/Mods/manifest.json</c>. Mods that
    /// provide a <see cref="InstalledMod.ReleaseApiUrl"/> are queried for the
    /// newest version; mods without one are skipped rather than reported as
    /// outdated so the Update tab does not show false positives.
    /// </remarks>
    internal static class ModUpdateScanner
    {
        internal const string ManifestDirectory = "Mods";
        internal const string ManifestFile = "manifest.json";

        internal static string ManifestLocation =>
            Path.Combine(Paths.Modifications, ManifestDirectory, ManifestFile);

        internal static bool ManifestExists =>
            File.Exists(ManifestLocation);

        internal static List<InstalledMod> ReadManifest()
        {
            var mods = new List<InstalledMod>();

            if (!ManifestExists)
                return mods;

            string contents = File.ReadAllText(ManifestLocation);

            var parsed = JsonSerializer.Deserialize<List<InstalledMod>>(
                contents,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (parsed is null)
                return mods;

            foreach (var mod in parsed)
            {
                if (String.IsNullOrWhiteSpace(mod.Id))
                    continue;

                if (String.IsNullOrWhiteSpace(mod.Name))
                    mod.Name = mod.Id;

                if (String.IsNullOrWhiteSpace(mod.Version))
                    mod.Version = "0.0.0";

                mods.Add(mod);
            }

            return mods;
        }

        internal static async Task<List<ModUpdateInfo>> GetOutdatedModsAsync()
        {
            var outdated = new List<ModUpdateInfo>();

            foreach (var mod in ReadManifest())
            {
                if (String.IsNullOrWhiteSpace(mod.ReleaseApiUrl))
                    continue;

                string? latest = await GetLatestVersionAsync(mod);

                if (latest is null)
                    continue;

                if (Compare(latest, mod.Version) > 0)
                    outdated.Add(new ModUpdateInfo { Mod = mod, Latest = latest });
            }

            return outdated;
        }

        internal static async Task<int> CountOutdatedModsAsync() =>
            (await GetOutdatedModsAsync()).Count;

        private static async Task<string?> GetLatestVersionAsync(InstalledMod mod)
        {
            const string LOG_IDENT = "ModUpdateScanner::GetLatestVersionAsync";

            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(15)
                };

                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    $"{App.ProjectName}/{App.Version}"
                );

                string json = await client.GetStringAsync(mod.ReleaseApiUrl!);

                using var document = JsonDocument.Parse(json);

                // Support the common shapes: { "tag_name": "v1.2.3" } and { "version": "1.2.3" }
                foreach (string name in new[] { "tag_name", "version", "latest_version" })
                {
                    if (document.RootElement.TryGetProperty(name, out var element) &&
                        element.ValueKind == JsonValueKind.String)
                    {
                        return element.GetString();
                    }
                }

                App.Logger.WriteLine(LOG_IDENT, $"Unrecognised response shape for '{mod.Id}'");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            return null;
        }

        private static int Compare(string left, string right)
        {
            if (Version.TryParse(Trim(left), out var l) &&
                Version.TryParse(Trim(right), out var r))
            {
                return l.CompareTo(r);
            }

            return String.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string Trim(string value)
        {
            string result = value.Trim();

            if (result.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                result = result[1..];

            int cut = result.IndexOfAny(new[] { '-', '+' });

            return cut >= 0 ? result[..cut] : result;
        }
    }
}