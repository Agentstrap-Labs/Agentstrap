namespace Agentstrap.Extensions
{
    public static class ExtensionPackageValidator
    {
        public static bool IsSafeRelativePath(string path, out string normalized)
        {
            normalized = "";
            if (string.IsNullOrWhiteSpace(path)) return false;
            // reject absolute and traversal
            if (Path.IsPathRooted(path)) return false;
            string[] parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any(p => p == ".." || p == ".")) return false;
            normalized = Path.Combine(parts);
            return true;
        }

        public static bool ValidateManifestJson(string json, out ExtensionManifest? manifest, out List<string> errors)
        {
            errors = new List<string>();
            manifest = null;
            try
            {
                manifest = JsonSerializer.Deserialize<ExtensionManifest>(json);
            }
            catch (Exception ex)
            {
                errors.Add("Manifest JSON is invalid: " + ex.Message);
                return false;
            }
            if (manifest is null)
            {
                errors.Add("Manifest is null.");
                return false;
            }
            return manifest.IsValid(out errors);
        }

        public static bool ValidateArchiveEntries(IEnumerable<string> entryNames, out List<string> errors)
        {
            errors = new List<string>();
            foreach (var name in entryNames)
            {
                if (!IsSafeRelativePath(name, out _))
                {
                    errors.Add("Unsafe archive entry path: " + name);
                }
            }
            return errors.Count == 0;
        }
    }
}