using System.Text.Json.Serialization;

namespace Agentstrap.Profiles
{
    public sealed class AgentstrapProfile
    {
        public int SchemaVersion { get; set; } = 1;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "New Profile";
        public string Description { get; set; } = "";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
        public string Platform { get; set; } = PlatformInfo.Current.ToString();
        public Settings Settings { get; set; } = new();
        public Dictionary<string, string> FastFlags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> ModSelections { get; set; } = new();
    }

    public sealed class AgentstrapProfileSettings
    {
        // A serializable copy of relevant settings. Stored as a dictionary to preserve unknown data.
        public Dictionary<string, JsonElement?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public static AgentstrapProfileSettings FromSettings(Settings settings)
        {
            var result = new AgentstrapProfileSettings();
            string json = JsonSerializer.Serialize(settings);
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                result.Values[prop.Name] = prop.Value.Clone();
            }
            return result;
        }

        public Settings ToSettings(Settings? fallback = null)
        {
            var result = fallback is null ? new Settings() : Clone(fallback);
            if (Values.Count == 0) return result;

            string json = JsonSerializer.Serialize(new Dictionary<string, object?>(Values.ToDictionary(
                kv => kv.Key,
                kv => (object?)kv.Value)));
            // Merge: deserialize into existing not straightforward; instead create new and copy known props via reflection
            try
            {
                var deserialized = JsonSerializer.Deserialize<Settings>(json);
                if (deserialized is not null)
                    return deserialized;
            }
            catch
            {
                // fall back to original
            }
            return result;
        }

        private static Settings Clone(Settings s) =>
            JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(s)) ?? new Settings();
    }
}