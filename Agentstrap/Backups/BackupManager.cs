namespace Agentstrap.Backups
{
    public sealed class BackupManager
    {
        public string BackupsDirectory { get; }

        public BackupManager(string backupsDirectory)
        {
            BackupsDirectory = backupsDirectory;
            Directory.CreateDirectory(backupsDirectory);
        }

        public string GetBackupPath(string id) => Path.Combine(BackupsDirectory, id + ".json");

        public ConfigurationBackup CreateBackup(Settings settings, string description = "")
        {
            var backup = new ConfigurationBackup
            {
                Description = string.IsNullOrWhiteSpace(description) ? "Manual backup" : description,
                Settings = CloneSettings(settings)
            };
            backup.Checksum = ComputeChecksum(backup.Settings);
            string path = GetBackupPath(backup.Id);
            backup.FilePath = path;
            Directory.CreateDirectory(BackupsDirectory);
            string json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            return backup;
        }

        public IReadOnlyList<ConfigurationBackup> ListBackups()
        {
            var result = new List<ConfigurationBackup>();
            if (!Directory.Exists(BackupsDirectory)) return result;
            foreach (var file in Directory.GetFiles(BackupsDirectory, "*.json"))
            {
                var backup = TryLoad(file);
                if (backup is not null) result.Add(backup);
            }
            return result.OrderByDescending(b => b.CreatedUtc).ToList();
        }

        public ConfigurationBackup? LoadBackup(string id)
        {
            string path = GetBackupPath(id);
            if (!File.Exists(path)) return null;
            return TryLoad(path);
        }

        public bool ValidateBackup(string path)
        {
            var backup = TryLoad(path);
            if (backup is null) return false;
            return backup.Checksum == ComputeChecksum(backup.Settings);
        }

        public Settings RestoreBackup(ConfigurationBackup backup, Settings currentSettings)
        {
            if (backup is null) throw new ArgumentNullException(nameof(backup));
            if (!ValidateBackupObject(backup)) throw new InvalidDataException("Backup integrity check failed.");
            // safety backup of current
            CreateBackup(currentSettings, "Safety backup before restore");
            return CloneSettings(backup.Settings);
        }

        public string ExportBackup(string id, string destinationPath)
        {
            var backup = LoadBackup(id) ?? throw new FileNotFoundException("Backup not found.", id);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            string json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(destinationPath, json);
            return destinationPath;
        }

        public ConfigurationBackup ImportBackup(string sourcePath)
        {
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("Backup file not found.", sourcePath);
            var backup = TryLoad(sourcePath) ?? throw new InvalidDataException("Invalid backup file.");
            if (!ValidateBackupObject(backup)) throw new InvalidDataException("Backup integrity check failed.");
            // assign new id to avoid collisions
            backup.Id = Guid.NewGuid().ToString("N");
            backup.FilePath = GetBackupPath(backup.Id);
            string json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(backup.FilePath, json);
            return backup;
        }

        public int ApplyRetention(int maxCount)
        {
            if (maxCount < 1) throw new ArgumentOutOfRangeException(nameof(maxCount));
            var backups = ListBackups();
            int removed = 0;
            foreach (var b in backups.Skip(maxCount))
            {
                try
                {
                    if (File.Exists(b.FilePath)) { File.Delete(b.FilePath); removed++; }
                }
                catch { }
            }
            return removed;
        }

        private ConfigurationBackup? TryLoad(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var backup = JsonSerializer.Deserialize<ConfigurationBackup>(json);
                if (backup is null) return null;
                if (string.IsNullOrWhiteSpace(backup.Id)) backup.Id = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(backup.FilePath)) backup.FilePath = path;
                return backup;
            }
            catch
            {
                return null;
            }
        }

        private bool ValidateBackupObject(ConfigurationBackup backup) =>
            backup.Checksum == ComputeChecksum(backup.Settings);

        private static Settings CloneSettings(Settings s) =>
            JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(s)) ?? new Settings();

        private static string ComputeChecksum(Settings settings)
        {
            string json = JsonSerializer.Serialize(settings);
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(bytes);
        }
    }
}