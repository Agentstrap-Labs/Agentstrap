using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Backs the Update tab. Reports on Agentstrap releases, installed mod
    /// updates, pending configuration migrations and NEXUS channel state.
    /// </summary>
    public class UpdateViewModel : NotifyPropertyChangedViewModel
    {
        private string _agentstrapStatus = "Not checked yet";
        private string _installedVersion = App.Version;
        private string _latestVersion = "-";
        private string _releaseSummary = "Run a check to compare against the latest published release.";
        private bool _updateAvailable;
        private bool _isChecking;

        private string _modsStatus = "Not checked yet";
        private string _modsSummary = "Run a check to scan installed mods for newer versions.";
        private int _modsOutdated;

        private string _migrationStatus = "Not checked yet";
        private string _migrationSummary = "Run a check to validate stored configuration.";

        private string _nexusStatus = "Not checked yet";
        private string _nexusSummary = "Run a check to validate NEXUS state.";

        public string AgentstrapStatus
        {
            get => _agentstrapStatus;
            private set => Set(ref _agentstrapStatus, value);
        }

        public string InstalledVersion
        {
            get => _installedVersion;
            private set => Set(ref _installedVersion, value);
        }

        public string LatestVersion
        {
            get => _latestVersion;
            private set => Set(ref _latestVersion, value);
        }

        public string ReleaseSummary
        {
            get => _releaseSummary;
            private set => Set(ref _releaseSummary, value);
        }

        public bool UpdateAvailable
        {
            get => _updateAvailable;
            private set => Set(ref _updateAvailable, value);
        }

        public bool IsChecking
        {
            get => _isChecking;
            private set => Set(ref _isChecking, value);
        }

        public string ModsStatus
        {
            get => _modsStatus;
            private set => Set(ref _modsStatus, value);
        }

        public string ModsSummary
        {
            get => _modsSummary;
            private set => Set(ref _modsSummary, value);
        }

        public int ModsOutdated
        {
            get => _modsOutdated;
            private set => Set(ref _modsOutdated, value);
        }

        public string MigrationStatus
        {
            get => _migrationStatus;
            private set => Set(ref _migrationStatus, value);
        }

        public string MigrationSummary
        {
            get => _migrationSummary;
            private set => Set(ref _migrationSummary, value);
        }

        public string NexusStatus
        {
            get => _nexusStatus;
            private set => Set(ref _nexusStatus, value);
        }

        public string NexusSummary
        {
            get => _nexusSummary;
            private set => Set(ref _nexusSummary, value);
        }

        public ICommand CheckUpdatesCommand =>
            new AsyncRelayCommand(CheckAllAsync);

        public ICommand CheckAgentstrapCommand =>
            new AsyncRelayCommand(CheckAgentstrapAsync);

        public ICommand CheckModsCommand =>
            new AsyncRelayCommand(CheckModsAsync);

        public ICommand CheckMigrationsCommand =>
            new AsyncRelayCommand(CheckMigrationsAsync);

        public ICommand CheckNexusCommand =>
            new AsyncRelayCommand(CheckNexusAsync);

        public ICommand OpenReleasesCommand =>
            new RelayCommand(OpenReleases);

        public ICommand OpenModsFolderCommand =>
            new RelayCommand(() => OpenFolder(Paths.Modifications));

        public UpdateViewModel()
        {
            // fire and forget: the page binds to state as each check completes
            _ = CheckAllAsync();
        }

        /// <summary>
        /// Runs every update check. Safe to call repeatedly and concurrently.
        /// </summary>
        public async Task CheckAllAsync()
        {
            if (IsChecking)
                return;

            IsChecking = true;

            try
            {
                await Task.WhenAll(
                    CheckAgentstrapAsync(),
                    CheckModsAsync(),
                    CheckMigrationsAsync(),
                    CheckNexusAsync()
                );
            }
            finally
            {
                IsChecking = false;
                OnPropertyChanged(nameof(IsChecking));
            }
        }

        public async Task CheckAgentstrapAsync()
        {
            const string LOG_IDENT = "UpdateViewModel::CheckAgentstrapAsync";

            AgentstrapStatus = "Checking for updates...";
            OnPropertyChanged(nameof(AgentstrapStatus));

            if (!App.Settings.Prop.CheckForUpdates)
            {
                AgentstrapStatus = "Update checking is disabled";
                ReleaseSummary =
                    "Enable automatic update checking in Agentstrap settings to query GitHub.";
                UpdateAvailable = false;
                return;
            }

            try
            {
                GithubRelease? release = await App.GetLatestRelease();

                if (release is null || String.IsNullOrEmpty(release.TagName))
                {
                    AgentstrapStatus = "Could not reach the release feed";
                    ReleaseSummary =
                        "GitHub did not return usable release data. Check your connection and try again.";
                    return;
                }

                string latest = NormalizeVersion(release.TagName);

                LatestVersion = latest;
                InstalledVersion = App.Version;

                if (IsNewer(latest, App.Version))
                {
                    UpdateAvailable = true;
                    AgentstrapStatus = "Update available";
                    ReleaseSummary =
                        $"{release.Name} ({release.CreatedAt}) is newer than the installed build.";
                }
                else
                {
                    UpdateAvailable = false;
                    AgentstrapStatus = "Agentstrap is up to date";
                    ReleaseSummary =
                        $"Installed build {App.Version} matches the latest published release.";
                }

                OnPropertyChanged(nameof(LatestVersion));
                OnPropertyChanged(nameof(InstalledVersion));
                OnPropertyChanged(nameof(UpdateAvailable));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                AgentstrapStatus = "Update check failed";
                ReleaseSummary = ex.Message;
            }
        }

        public async Task CheckModsAsync()
        {
            const string LOG_IDENT = "UpdateViewModel::CheckModsAsync";

            ModsStatus = "Scanning installed mods...";
            OnPropertyChanged(nameof(ModsStatus));

            try
            {
                if (!ModUpdateScanner.ManifestExists)
                {
                    ModsOutdated = 0;
                    ModsStatus = "No mods to check";
                    ModsSummary =
                        $"No mods manifest was found at {ModUpdateScanner.ManifestLocation}.";
                    return;
                }

                var outdated = await ModUpdateScanner.GetOutdatedModsAsync();

                ModsOutdated = outdated.Count;
                OnPropertyChanged(nameof(ModsOutdated));

                if (outdated.Count == 0)
                {
                    ModsStatus = "All mods are up to date";
                    ModsSummary =
                        "No installed mod reported a newer version than the one on disk.";
                }
                else
                {
                    ModsStatus = $"{outdated.Count} mod update(s) available";
                    ModsSummary = string.Join(
                        "\n",
                        outdated.Select(x => $"{x.Name}: {x.InstalledVersion} -> {x.LatestVersion}")
                    );
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                ModsStatus = "Mod scan failed";
                ModsSummary = ex.Message;
            }
        }

        public Task CheckMigrationsAsync()
        {
            const string LOG_IDENT = "UpdateViewModel::CheckMigrationsAsync";

            MigrationStatus = "Validating configuration...";
            OnPropertyChanged(nameof(MigrationStatus));

            try
            {
                var report = ConfigurationMigrationRunner.Inspect();

                if (report.Pending.Count == 0 && report.Repaired.Count == 0)
                {
                    MigrationStatus = "Configuration is up to date";
                    MigrationSummary =
                        $"Schema revision {report.Revision}. No migrations were required.";
                }
                else
                {
                    MigrationStatus = report.Pending.Count > 0
                        ? $"{report.Pending.Count} migration(s) pending"
                        : "Migrations repaired";

                    MigrationSummary = string.Join(
                        "\n",
                        report.Pending.Select(x => "Pending: " + x)
                            .Concat(report.Repaired.Select(x => "Repaired: " + x))
                    );
                }

                OnPropertyChanged(nameof(HasPendingMigrations));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                MigrationStatus = "Migration check failed";
                MigrationSummary = ex.Message;
            }

            return Task.CompletedTask;
        }

        public Task CheckNexusAsync()
        {
            try
            {
                if (App.Settings.Prop.NexusModeEnabled)
                {
                    NexusStatus = "NEXUS mode is active";
                    NexusSummary =
                        $"Channel '{App.Settings.Prop.NexusChannel}'. " +
                        "NEXUS may ship experimental behaviour that is not production ready.";
                }
                else
                {
                    NexusStatus = "NEXUS mode is disabled";
                    NexusSummary =
                        "Enable NEXUS under Agentstrap > Settings > Advanced > NEXUS to opt in.";
                }

                OnPropertyChanged(nameof(NexusStatus));
                OnPropertyChanged(nameof(NexusSummary));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("UpdateViewModel::CheckNexusAsync", ex);

                NexusStatus = "NEXUS state unavailable";
                NexusSummary = ex.Message;
            }

            return Task.CompletedTask;
        }

        public bool HasPendingMigrations
        {
            get
            {
                try
                {
                    return ConfigurationMigrationRunner.Inspect().Pending.Count > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public ICommand ApplyMigrationsCommand =>
            new AsyncRelayCommand(ApplyMigrationsAsync);

        public async Task ApplyMigrationsAsync()
        {
            const string LOG_IDENT = "UpdateViewModel::ApplyMigrationsAsync";

            MigrationStatus = "Applying migrations...";
            OnPropertyChanged(nameof(MigrationStatus));

            try
            {
                int applied = ConfigurationMigrationRunner.Apply();

                if (applied > 0)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Applied {applied} migration(s)");
                    App.Settings.Save();
                    App.State.Save();
                    App.FastFlags.Save();
                }

                MigrationStatus = applied > 0
                    ? $"Applied {applied} migration(s)"
                    : "Nothing to migrate";

                MigrationSummary = applied > 0
                    ? "Configuration was migrated and saved."
                    : "Configuration already matched the current schema.";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                MigrationStatus = "Migration failed";
                MigrationSummary = ex.Message;
            }

            OnPropertyChanged(nameof(MigrationStatus));
            OnPropertyChanged(nameof(MigrationSummary));
            OnPropertyChanged(nameof(HasPendingMigrations));
        }

        private static void OpenReleases()
        {
            OpenUrl(App.ProjectDownloadLink);
        }

        internal static void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "The folder could not be opened.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
        }

        internal static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "The link could not be opened.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Strips a leading "v" and any pre-release suffix from a tag name.
        /// </summary>
        private static string NormalizeVersion(string tag)
        {
            string value = tag.Trim();

            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                value = value[1..];

            int cut = value.IndexOfAny(new[] { '-', '+' });

            if (cut >= 0)
                value = value[..cut];

            return value;
        }

        private static bool IsNewer(string candidate, string current)
        {
            if (Version.TryParse(candidate, out var left) &&
                Version.TryParse(current, out var right))
            {
                return left > right;
            }

            return String.Compare(candidate, current, StringComparison.OrdinalIgnoreCase) > 0;
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propertyName!);
        }
    }
}