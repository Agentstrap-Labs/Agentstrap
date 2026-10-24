namespace Agentstrap.Profiles
{
    public sealed class ProfileManager
    {
        public string ProfilesDirectory { get; }

        public ProfileManager(string profilesDirectory)
        {
            ProfilesDirectory = profilesDirectory;
            Directory.CreateDirectory(profilesDirectory);
        }

        public string GetProfilePath(string name) =>
            Path.Combine(ProfilesDirectory, SanitizeFileName(name) + ".json");

        public IReadOnlyList<string> ListProfiles()
        {
            if (!Directory.Exists(ProfilesDirectory)) return Array.Empty<string>();
            return Directory.GetFiles(ProfilesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList()!;
        }

        public AgentstrapProfile CreateFromSettings(string name, Settings settings, string description = "")
        {
            var profile = new AgentstrapProfile
            {
                Name = name,
                Description = description,
                Settings = CloneSettings(settings),
                Platform = PlatformInfo.Current.ToString()
            };
            return profile;
        }

        public AgentstrapProfile CreateFromTemplate(string name, string description = "")
        {
            return new AgentstrapProfile { Name = name, Description = description };
        }

        public void SaveProfile(AgentstrapProfile profile)
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Name)) throw new ArgumentException("Profile name is required.", nameof(profile));
            profile.ModifiedUtc = DateTime.UtcNow;
            Directory.CreateDirectory(ProfilesDirectory);
            string path = GetProfilePath(profile.Name);
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public AgentstrapProfile? LoadProfile(string name)
        {
            string path = GetProfilePath(name);
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                var profile = JsonSerializer.Deserialize<AgentstrapProfile>(json);
                if (profile is null) return null;
                if (string.IsNullOrWhiteSpace(profile.Name)) profile.Name = name;
                return profile;
            }
            catch
            {
                return null;
            }
        }

        public bool DeleteProfile(string name)
        {
            string path = GetProfilePath(name);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        public AgentstrapProfile DuplicateProfile(string sourceName, string newName)
        {
            var source = LoadProfile(sourceName) ?? throw new FileNotFoundException("Profile not found.", sourceName);
            var copy = CloneProfile(source);
            copy.Id = Guid.NewGuid().ToString("N");
            copy.Name = newName;
            copy.CreatedUtc = DateTime.UtcNow;
            copy.ModifiedUtc = DateTime.UtcNow;
            SaveProfile(copy);
            return copy;
        }

        public bool RenameProfile(string oldName, string newName)
        {
            var profile = LoadProfile(oldName);
            if (profile is null) return false;
            profile.Name = newName;
            SaveProfile(profile);
            // remove old file
            string oldPath = GetProfilePath(oldName);
            if (File.Exists(oldPath) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
                File.Delete(oldPath);
            return true;
        }

        public string ExportProfile(string name, string destinationPath)
        {
            var profile = LoadProfile(name) ?? throw new FileNotFoundException("Profile not found.", name);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(destinationPath, json);
            return destinationPath;
        }

        public AgentstrapProfile ImportProfile(string sourcePath)
        {
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("Profile file not found.", sourcePath);
            string json = File.ReadAllText(sourcePath);
            var profile = JsonSerializer.Deserialize<AgentstrapProfile>(json) ?? throw new InvalidDataException("Invalid profile file.");
            ValidateProfile(profile);
            return profile;
        }

        public void ValidateProfile(AgentstrapProfile profile)
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Name)) throw new ArgumentException("Profile name is required.");
            if (profile.SchemaVersion < 1) throw new ArgumentException("Invalid profile schema version.");
            // detect incompatible platform-specific settings: if profile platform != current, mark but don't fail
        }

        public AgentstrapProfile? FindProfile(string name) => LoadProfile(name);

        private static Settings CloneSettings(Settings s) =>
            JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(s)) ?? new Settings();

        private static AgentstrapProfile CloneProfile(AgentstrapProfile p) =>
            JsonSerializer.Deserialize<AgentstrapProfile>(JsonSerializer.Serialize(p)) ?? new AgentstrapProfile();

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "profile" : name;
        }
    }
}