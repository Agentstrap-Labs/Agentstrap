using System.IO.Compression;

namespace Agentstrap.Extensions
{
    public sealed class InstalledExtension
    {
        public ExtensionManifest Manifest { get; set; } = new();
        public string InstallPath { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public DateTime InstalledUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class ExtensionManager
    {
        public string ExtensionsDirectory { get; }

        public ExtensionManager(string extensionsDirectory)
        {
            ExtensionsDirectory = extensionsDirectory;
            Directory.CreateDirectory(extensionsDirectory);
        }

        public string GetExtensionDirectory(string id) => Path.Combine(ExtensionsDirectory, Sanitize(id));

        public IReadOnlyList<InstalledExtension> ListInstalled()
        {
            var result = new List<InstalledExtension>();
            if (!Directory.Exists(ExtensionsDirectory)) return result;
            foreach (var dir in Directory.GetDirectories(ExtensionsDirectory))
            {
                string manifestPath = Path.Combine(dir, "manifest.json");
                if (!File.Exists(manifestPath)) continue;
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    if (!ExtensionPackageValidator.ValidateManifestJson(json, out var manifest, out _) || manifest is null) continue;
                    bool enabled = !File.Exists(Path.Combine(dir, "disabled.flag"));
                    result.Add(new InstalledExtension
                    {
                        Manifest = manifest,
                        InstallPath = dir,
                        Enabled = enabled,
                        InstalledUtc = Directory.GetCreationTimeUtc(dir)
                    });
                }
                catch { }
            }
            return result;
        }

        public InstalledExtension InstallFromDirectory(string sourceDirectory)
        {
            if (!Directory.Exists(sourceDirectory)) throw new DirectoryNotFoundException(sourceDirectory);
            string manifestPath = Path.Combine(sourceDirectory, "manifest.json");
            if (!File.Exists(manifestPath)) throw new FileNotFoundException("manifest.json not found.", manifestPath);
            string json = File.ReadAllText(manifestPath);
            if (!ExtensionPackageValidator.ValidateManifestJson(json, out var manifest, out var errors) || manifest is null)
                throw new InvalidDataException(string.Join("; ", errors));

            string dest = GetExtensionDirectory(manifest.Id);
            Directory.CreateDirectory(dest);
            // copy files
            foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(sourceDirectory, file);
                if (!ExtensionPackageValidator.IsSafeRelativePath(rel, out var normalized)) throw new InvalidDataException("Unsafe path: " + rel);
                string target = Path.Combine(dest, normalized);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
            return new InstalledExtension { Manifest = manifest, InstallPath = dest, Enabled = true };
        }

        public InstalledExtension InstallFromZip(string zipPath)
        {
            if (!File.Exists(zipPath)) throw new FileNotFoundException(zipPath);
            using var archive = ZipFile.OpenRead(zipPath);
            var entryNames = archive.Entries.Select(e => e.FullName);
            if (!ExtensionPackageValidator.ValidateArchiveEntries(entryNames, out var errors))
                throw new InvalidDataException(string.Join("; ", errors));

            // find manifest entry
            var manifestEntry = archive.Entries.FirstOrDefault(e => e.FullName.Equals("manifest.json", StringComparison.OrdinalIgnoreCase));
            if (manifestEntry is null) throw new InvalidDataException("manifest.json not found in archive.");
            string manifestJson;
            using (var s = manifestEntry.Open())
            using (var r = new StreamReader(s))
                manifestJson = r.ReadToEnd();

            if (!ExtensionPackageValidator.ValidateManifestJson(manifestJson, out var manifest, out var merrors) || manifest is null)
                throw new InvalidDataException(string.Join("; ", merrors));

            string dest = GetExtensionDirectory(manifest.Id);
            Directory.CreateDirectory(dest);
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name)) continue; // directory
                if (!ExtensionPackageValidator.IsSafeRelativePath(entry.FullName, out var normalized)) throw new InvalidDataException("Unsafe path: " + entry.FullName);
                string target = Path.Combine(dest, normalized);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                using var es = entry.Open();
                using var fs = File.Create(target);
                es.CopyTo(fs);
            }
            return new InstalledExtension { Manifest = manifest, InstallPath = dest, Enabled = true };
        }

        public bool Enable(string id)
        {
            string dir = GetExtensionDirectory(id);
            string flag = Path.Combine(dir, "disabled.flag");
            if (File.Exists(flag)) File.Delete(flag);
            return Directory.Exists(dir);
        }

        public bool Disable(string id)
        {
            string dir = GetExtensionDirectory(id);
            if (!Directory.Exists(dir)) return false;
            File.WriteAllText(Path.Combine(dir, "disabled.flag"), "");
            return true;
        }

        public bool Remove(string id)
        {
            string dir = GetExtensionDirectory(id);
            if (!Directory.Exists(dir)) return false;
            Directory.Delete(dir, recursive: true);
            return true;
        }

        public InstalledExtension? Get(string id) => ListInstalled().FirstOrDefault(e => e.Manifest.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        private static string Sanitize(string id)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) id = id.Replace(c, '_');
            return id;
        }
    }
}