namespace Agentstrap.Diagnostics
{
    public sealed class DiagnosticResult
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Info;
        public string TechnicalInfo { get; init; } = "";
        public string SuggestedResolution { get; init; } = "";
        public Func<bool>? RepairAction { get; init; }
        public bool RepairAvailable => RepairAction is not null;

        public bool RunRepair() => RepairAction?.Invoke() ?? false;
    }
}