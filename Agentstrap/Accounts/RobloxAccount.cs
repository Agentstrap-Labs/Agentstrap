namespace Agentstrap.Accounts
{
    public sealed class RobloxAccount
    {
        public long UserId { get; set; }
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public DateTime AddedUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class AuthenticatedRobloxUser
    {
        public long Id { get; set; }
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}