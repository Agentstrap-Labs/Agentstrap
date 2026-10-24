namespace Agentstrap.Backups
{
    public sealed class ConfigurationBackup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Checksum { get; set; } = "";
        public int SchemaVersion { get; set; } = 1;
        public Settings Settings { get; set; } = new();
    }
}