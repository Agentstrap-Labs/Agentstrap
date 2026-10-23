namespace Agentstrap.Enums
{
    /// <summary>
    /// Status values reported by Roblox Apple Emoji Mode. These intentionally
    /// separate detection, configuration and rendering verification so that a
    /// successful file write is never reported as proof that Roblox renders
    /// Apple style emojis.
    /// </summary>
    public enum AppleEmojiModeStatus
    {
        Disabled,
        EnabledAndApplied,
        SetupRequired,
        RobloxNotDetected,
        AssetsUnavailable,
        RenderingMethodUnsupported,
        RenderingUnverified,
        RenderingVerified,
        Failed
    }
}