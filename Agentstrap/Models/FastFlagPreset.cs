namespace Agentstrap.Models
{
    /// <summary>
    /// A curated bundle of fast flags that can be applied in one click. A null
    /// value removes the flag so the bundle can also reset settings it owns.
    /// </summary>
    public class FastFlagPreset
    {
        public string Name { get; init; } = "";

        public string Description { get; init; } = "";

        public IReadOnlyDictionary<string, string?> Flags { get; init; } = new Dictionary<string, string?>();
    }
}
