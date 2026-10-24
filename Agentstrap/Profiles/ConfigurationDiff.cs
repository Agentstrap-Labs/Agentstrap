namespace Agentstrap.Profiles
{
    public sealed class ConfigurationDifference
    {
        public string Path { get; init; } = "";
        public string ChangeType { get; init; } = ""; // Added, Removed, Changed
        public string? OldValue { get; init; }
        public string? NewValue { get; init; }
    }

    public static class ConfigurationComparer
    {
        public static List<ConfigurationDifference> CompareSettings(Settings left, Settings right)
        {
            var result = new List<ConfigurationDifference>();
            string leftJson = JsonSerializer.Serialize(left);
            string rightJson = JsonSerializer.Serialize(right);
            using var leftDoc = JsonDocument.Parse(leftJson);
            using var rightDoc = JsonDocument.Parse(rightJson);
            CompareElements("", leftDoc.RootElement, rightDoc.RootElement, result);
            return result;
        }

        public static List<ConfigurationDifference> CompareProfiles(AgentstrapProfile left, AgentstrapProfile right) =>
            CompareSettings(left.Settings, right.Settings);

        public static List<ConfigurationDifference> CompareToBackup(Settings current, Settings backup) =>
            CompareSettings(current, backup);

        private static void CompareElements(string path, JsonElement left, JsonElement right, List<ConfigurationDifference> result)
        {
            if (left.ValueKind != right.ValueKind)
            {
                result.Add(new ConfigurationDifference
                {
                    Path = path,
                    ChangeType = "Changed",
                    OldValue = left.ToString(),
                    NewValue = right.ToString()
                });
                return;
            }

            if (left.ValueKind == JsonValueKind.Object)
            {
                var leftProps = left.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var rightProps = right.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                foreach (var key in leftProps.Keys.Union(rightProps.Keys).OrderBy(k => k))
                {
                    string childPath = string.IsNullOrEmpty(path) ? key : path + "." + key;
                    if (!rightProps.ContainsKey(key))
                    {
                        result.Add(new ConfigurationDifference { Path = childPath, ChangeType = "Removed", OldValue = leftProps[key].ToString(), NewValue = null });
                    }
                    else if (!leftProps.ContainsKey(key))
                    {
                        result.Add(new ConfigurationDifference { Path = childPath, ChangeType = "Added", OldValue = null, NewValue = rightProps[key].ToString() });
                    }
                    else
                    {
                        CompareElements(childPath, leftProps[key], rightProps[key], result);
                    }
                }
                return;
            }

            if (left.ValueKind == JsonValueKind.Array)
            {
                if (left.GetRawText() != right.GetRawText())
                {
                    result.Add(new ConfigurationDifference { Path = path, ChangeType = "Changed", OldValue = left.ToString(), NewValue = right.ToString() });
                }
                return;
            }

            if (!JsonElementsEqual(left, right))
            {
                result.Add(new ConfigurationDifference { Path = path, ChangeType = "Changed", OldValue = left.ToString(), NewValue = right.ToString() });
            }
        }

        private static bool JsonElementsEqual(JsonElement a, JsonElement b) =>
            a.ValueKind == b.ValueKind && a.GetRawText() == b.GetRawText();
    }
}