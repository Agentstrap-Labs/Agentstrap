namespace Agentstrap.Integrations
{
    /// <summary>
    /// A single Bark message, following the documented API fields that this
    /// feature needs. Credentials are supplied by the caller, never carried here.
    /// </summary>
    public class BarkMessage
    {
        public const int MaxMessageLength = 4096;
        public const int MaxTitleLength = 250;

        public string Message { get; set; } = String.Empty;

        public string? Title { get; set; }

    }
}
