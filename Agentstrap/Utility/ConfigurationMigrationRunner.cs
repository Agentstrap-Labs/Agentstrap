namespace Agentstrap.Utility
{
    /// <summary>
    /// A single configuration migration that should run against the stored settings.
    /// </summary>
    public class ConfigurationMigration
    {
        /// <summary>
        /// Incremented when a new migration is introduced. Migrations run in order.
        /// </summary>
        public int Revision { get; init; }

        public string Name { get; init; } = "";

        public string Description { get; init; } = "";

        public Action? Apply { get; init; }

        public Func<bool>? IsPending { get; init; }
    }

    public class MigrationReport
    {
        /// <summary>
        /// The schema revision the stored configuration currently satisfies.
        /// </summary>
        public int Revision { get; init; }

        public List<string> Pending { get; init; } = new();

        public List<string> Repaired { get; init; } = new();

        public bool NeedsMigration => Pending.Count > 0;
    }

    /// <summary>
    /// Validates and upgrades the persisted Agentstrap configuration.
    /// </summary>
    /// <remarks>
    /// Migrations are declared as an ordered list. <see cref="Inspect"/> reports
    /// what would change without writing anything, so the Update tab can display
    /// pending work before the user commits to it.
    /// </remarks>
    internal static class ConfigurationMigrationRunner
    {
        private const int CurrentRevision = 2;

        internal static int Revision => CurrentRevision;

        internal static MigrationReport Inspect()
        {
            var report = new MigrationReport { Revision = CurrentRevision };

            foreach (var migration in Migrations)
            {
                bool pending;

                try
                {
                    pending = migration.IsPending?.Invoke() ?? false;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(
                        $"ConfigurationMigrationRunner::Inspect({migration.Name})",
                        ex
                    );

                    continue;
                }

                if (pending)
                    report.Pending.Add($"{migration.Name}: {migration.Description}");
            }

            return report;
        }

        internal static int Apply()
        {
            int applied = 0;

            foreach (var migration in Migrations)
            {
                try
                {
                    if (migration.IsPending?.Invoke() != true)
                        continue;

                    migration.Apply?.Invoke();

                    applied++;

                    App.Logger.WriteLine(
                        "ConfigurationMigrationRunner::Apply",
                        $"Applied migration '{migration.Name}'"
                    );
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(
                        $"ConfigurationMigrationRunner::Apply({migration.Name})",
                        ex
                    );
                }
            }

            return applied;
        }

        /// <summary>
        /// Ordered migration list. Append new migrations; do not reorder existing ones.
        /// </summary>
        private static List<ConfigurationMigration> Migrations => new()
        {
            new()
            {
                Revision = 1,
                Name = "DropVoidstrapMatchmakerPlaceId",
                Description = "Matchmaker place id is empty and the matchmaker is enabled",
                IsPending = () =>
                    App.Settings.Prop.AgentstrapMatchmakerEnabled &&
                    String.IsNullOrWhiteSpace(App.Settings.Prop.AgentstrapMatchmakerPlaceId),
                Apply = () =>
                {
                    App.Settings.Prop.AgentstrapMatchmakerEnabled = false;
                }
            },
            new()
            {
                Revision = 2,
                Name = "NormalizeRobloxProcessPriority",
                Description = "Process priority is not a value Windows accepts",
                IsPending = () => !ValidPriorities.Contains(
                    App.Settings.Prop.RobloxProcessPriority
                ),
                Apply = () =>
                {
                    App.Settings.Prop.RobloxProcessPriority = "Normal";
                }
            }
        };

        /// <summary>
        /// Priority strings accepted by <c>Bootstrapper::GetRobloxProcessPriority</c>.
        /// </summary>
        private static readonly HashSet<string> ValidPriorities =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Realtime",
                "High",
                "Above normal",
                "Normal",
                "Below normal"
            };

        internal static IReadOnlyCollection<string> KnownProcessPriorities =>
            ValidPriorities;
    }
}