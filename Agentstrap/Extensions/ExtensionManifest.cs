namespace Agentstrap.Extensions
{
    public sealed class ExtensionManifest
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string Description { get; set; } = "";
        public string Author { get; set; } = "";
        public string MinimumAgentstrapVersion { get; set; } = "";
        public List<string> SupportedOperatingSystems { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public string EntryType { get; set; } = "declarative";
        public List<string> Dependencies { get; set; } = new();
        public string? EntryPath { get; set; }

        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Id)) errors.Add("Id is required.");
            if (string.IsNullOrWhiteSpace(Name)) errors.Add("Name is required.");
            if (string.IsNullOrWhiteSpace(Version)) errors.Add("Version is required.");
            if (string.IsNullOrWhiteSpace(MinimumAgentstrapVersion)) errors.Add("MinimumAgentstrapVersion is required.");
            if (SupportedOperatingSystems.Count == 0) errors.Add("SupportedOperatingSystems is required.");
            // validate id format
            if (!string.IsNullOrWhiteSpace(Id) && !System.Text.RegularExpressions.Regex.IsMatch(Id, "^[a-zA-Z0-9._-]+$"))
                errors.Add("Id contains invalid characters.");
            return errors.Count == 0;
        }
    }
}