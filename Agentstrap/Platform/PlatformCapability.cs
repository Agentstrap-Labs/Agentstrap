namespace Agentstrap.Platform
{
    public sealed class PlatformCapability
    {
        public string Name { get; init; } = "";
        public CapabilityStatus Status { get; init; } = CapabilityStatus.Unsupported;
        public string Description { get; init; } = "";
        public string Resolution { get; init; } = "";

        public bool IsAvailable => Status == CapabilityStatus.Supported || Status == CapabilityStatus.Experimental;
    }
}