using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Agentstrap.AppData;

namespace Agentstrap.UI.ViewModels.Settings
{
    public enum DiagnosticSeverity
    {
        Healthy,
        Warning,
        Problem
    }

    /// <summary>
    /// A labelled set of checks. Membership only depends on the category each check
    /// declares at construction, so the page can bind straight to it.
    /// </summary>
    public class DiagnosticGroup
    {
        public DiagnosticGroup(string title, IEnumerable<DiagnosticResult> results)
        {
            Title = title;
            Results = results.ToArray();
        }

        public string Title { get; }

        public IReadOnlyList<DiagnosticResult> Results { get; }
    }

    /// <summary>
    /// One line item in the Diagnostics center. Mutating any property raises
    /// change notification so bound rows refresh as the scan progresses.
    /// </summary>
    public class DiagnosticResult : INotifyPropertyChanged
    {
        private DiagnosticSeverity _severity = DiagnosticSeverity.Healthy;
        private string _summary = "Not checked yet";
        private string _details = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public required string Id { get; init; }

        public required string Title { get; init; }

        public string Category { get; init; } = "General";

        public DiagnosticSeverity Severity
        {
            get => _severity;
            set
            {
                if (_severity == value)
                    return;

                _severity = value;

                Raise();
                Raise(nameof(SeverityLabel));
                Raise(nameof(SeveritySymbol));
            }
        }

        public string Summary
        {
            get => _summary;
            set
            {
                if (_summary == value)
                    return;

                _summary = value;

                Raise();
            }
        }

        public string Details
        {
            get => _details;
            set
            {
                if (_details == value)
                    return;

                _details = value;

                Raise();
            }
        }

        public bool HasDetails => !String.IsNullOrWhiteSpace(Details);

        public string SeverityLabel => Severity switch
        {
            DiagnosticSeverity.Healthy => "Healthy",
            DiagnosticSeverity.Warning => "Warning",
            _ => "Problem"
        };

        /// <summary>
        /// Name of the icon representing this severity. Kept as data so the page can map
        /// it onto whatever icon set it renders with.
        /// </summary>
        public Wpf.Ui.Common.SymbolRegular SeveritySymbol => Severity switch
        {
            DiagnosticSeverity.Healthy => Wpf.Ui.Common.SymbolRegular.CheckmarkCircle24,
            DiagnosticSeverity.Warning => Wpf.Ui.Common.SymbolRegular.Warning24,
            _ => Wpf.Ui.Common.SymbolRegular.ErrorCircle24
        };

        private void Raise([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Backs the Diagnostics center. Each check reports Healthy, Warning or Problem.
    /// </summary>
    public class DiagnosticsViewModel : NotifyPropertyChangedViewModel
    {
        private bool _isRunning;
        private string _headline = "Not checked yet";
        private DiagnosticSeverity _overall = DiagnosticSeverity.Healthy;

        public IReadOnlyList<DiagnosticResult> Results { get; } = new List<DiagnosticResult>
        {
            new()
            {
                Id = "roblox-install",
                Title = "Roblox installation",
                Category = "Roblox"
            },
            new()
            {
                Id = "roblox-version",
                Title = "Roblox version state",
                Category = "Roblox"
            },
            new()
            {
                Id = "config",
                Title = "Agentstrap configuration",
                Category = "Configuration"
            },
            new()
            {
                Id = "missing-files",
                Title = "Missing files",
                Category = "Filesystem"
            },
            new()
            {
                Id = "invalid-settings",
                Title = "Invalid settings",
                Category = "Configuration"
            },
            new()
            {
                Id = "mod-conflicts",
                Title = "Mod conflicts",
                Category = "Mods"
            },
            new()
            {
                Id = "permissions",
                Title = "Permissions",
                Category = "Filesystem"
            },
            new()
            {
                Id = "update-state",
                Title = "Update state",
                Category = "Updates"
            }
        };

        public IEnumerable<DiagnosticResult> InstallationChecks =>
            Results.Where(x => x.Category == "Roblox");

        public IEnumerable<DiagnosticResult> ConfigurationChecks =>
            Results.Where(x => x.Category == "Configuration");

        public IEnumerable<DiagnosticResult> FilesystemChecks =>
            Results.Where(x => x.Category == "Filesystem");

        public IEnumerable<DiagnosticResult> ModChecks =>
            Results.Where(x => x.Category == "Mods");

        public IEnumerable<DiagnosticResult> UpdateChecks =>
            Results.Where(x => x.Category == "Updates");

        /// <summary>
        /// The same checks, ordered and labelled the way the page presents them. Empty
        /// groups are dropped so a category with nothing in it never leaves a dangling
        /// heading behind.
        /// </summary>
        public IReadOnlyList<DiagnosticGroup> Groups => _groups ??= new[]
        {
            new DiagnosticGroup("Roblox", InstallationChecks),
            new DiagnosticGroup("Configuration", ConfigurationChecks),
            new DiagnosticGroup("Filesystem", FilesystemChecks),
            new DiagnosticGroup("Mods", ModChecks),
            new DiagnosticGroup("Updates", UpdateChecks)
        }
            .Where(group => group.Results.Count > 0)
            .ToArray();

        private IReadOnlyList<DiagnosticGroup>? _groups;

        /// <summary>
        /// The per-severity totals are computed from <see cref="Results"/>, so they have
        /// to be refreshed whenever one of the checks changes severity. Without this the
        /// headline updates but the counts beside it stay frozen on their initial value.
        /// </summary>
        public DiagnosticsViewModel()
        {
            foreach (var result in Results)
                result.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName != nameof(DiagnosticResult.Severity))
                        return;

                    OnPropertyChanged(nameof(ProblemCount));
                    OnPropertyChanged(nameof(WarningCount));
                    OnPropertyChanged(nameof(HealthyCount));
                };
        }

        public string Headline
        {
            get => _headline;
            private set => Set(ref _headline, value);
        }

        public DiagnosticSeverity Overall
        {
            get => _overall;
            private set
            {
                if (!Set(ref _overall, value))
                    return;

                OnPropertyChanged(nameof(OverallLabel));
            }
        }

        public string OverallLabel => Overall switch
        {
            DiagnosticSeverity.Healthy => "Healthy",
            DiagnosticSeverity.Warning => "Warnings found",
            _ => "Problems found"
        };

        public int ProblemCount =>
            Results.Count(x => x.Severity == DiagnosticSeverity.Problem);

        public int WarningCount =>
            Results.Count(x => x.Severity == DiagnosticSeverity.Warning);

        public int HealthyCount =>
            Results.Count(x => x.Severity == DiagnosticSeverity.Healthy);

        public bool IsRunning
        {
            get => _isRunning;
            private set => Set(ref _isRunning, value);
        }

        public ICommand RunDiagnosticsCommand => new AsyncRelayCommand(RunDiagnosticsAsync);

        public ICommand OpenLogsCommand =>
            new RelayCommand(() => UpdateViewModel.OpenFolder(Paths.Logs));

        public ICommand CopyReportCommand => new RelayCommand(CopyReport);

        private async Task RunDiagnosticsAsync()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            Headline = "Running checks...";

            foreach (var result in Results)
            {
                result.Severity = DiagnosticSeverity.Healthy;
                result.Summary = "Checking...";
                result.Details = "";
            }

            await Task.WhenAll(
                CheckRobloxInstallationAsync(),
                CheckRobloxVersionAsync(),
                CheckConfigurationAsync(),
                CheckMissingFilesAsync(),
                CheckInvalidSettingsAsync(),
                CheckModConflictsAsync(),
                CheckPermissionsAsync(),
                CheckUpdateStateAsync()
            );

            Overall = ProblemCount > 0
                ? DiagnosticSeverity.Problem
                : WarningCount > 0
                    ? DiagnosticSeverity.Warning
                    : DiagnosticSeverity.Healthy;

            Headline = Overall switch
            {
                DiagnosticSeverity.Healthy =>
                    $"All {Results.Count} checks passed.",
                DiagnosticSeverity.Warning =>
                    $"{WarningCount} warning(s), {ProblemCount} problem(s).",
                _ =>
                    $"{ProblemCount} problem(s), {WarningCount} warning(s)."
            };

            IsRunning = false;
        }

        private DiagnosticResult Get(string id) =>
            Results.First(x => x.Id == id);

        private Task CheckRobloxInstallationAsync()
        {
            var result = Get("roblox-install");

            try
            {
                var player = new RobloxPlayerData();
                string path = player.ExecutablePath;

                if (!File.Exists(path))
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = "Roblox not detected";
                    result.Details =
                        "Roblox Player will be installed the next time you launch it through Agentstrap.";
                    return Task.CompletedTask;
                }

                result.Severity = DiagnosticSeverity.Healthy;
                result.Summary = "Roblox Player is installed";
                result.Details = path;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckRobloxInstallation", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Could not read the Roblox installation";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckRobloxVersionAsync()
        {
            var result = Get("roblox-version");

            try
            {
                if (!App.IsPlayerInstalled)
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = "Version state unknown";
                    result.Details =
                        "Agentstrap has not recorded a Roblox version for this profile yet.";
                    return Task.CompletedTask;
                }

                var state = App.PlayerState.Prop;

                result.Severity = DiagnosticSeverity.Healthy;
                result.Summary = "Version state is valid";
                result.Details = $"Version GUID {state.VersionGuid}";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckRobloxVersion", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Version state is unreadable";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckConfigurationAsync()
        {
            var result = Get("config");

            try
            {
                if (!App.Settings.Loaded)
                {
                    result.Severity = DiagnosticSeverity.Problem;
                    result.Summary = "Settings did not load";
                    result.Details =
                        "Agentstrap fell back to defaults. Check that Settings.json is not corrupt.";
                    return Task.CompletedTask;
                }

                var settings = App.Settings.Prop;
                var issues = new List<string>();

                if (!settings.CheckForUpdates)
                    issues.Add("Automatic update checking is disabled.");

                if (!settings.EnableAnalytics)
                    issues.Add("Analytics are disabled.");

                if (settings.WebEnvironment != WebEnvironment.Production)
                    issues.Add($"Web environment is set to {settings.WebEnvironment}.");

                if (settings.DeveloperMode)
                    issues.Add("Developer mode is enabled.");

                if (issues.Count > 0)
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = "Configuration loaded with non-default values";
                    result.Details = string.Join("\n", issues.Select(x => "- " + x));
                }
                else
                {
                    result.Severity = DiagnosticSeverity.Healthy;
                    result.Summary = "Configuration is valid";
                    result.Details =
                        $"Settings.json loaded from {App.Settings.FileLocation}";
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckConfiguration", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Configuration check failed";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckMissingFilesAsync()
        {
            var result = Get("missing-files");

            try
            {
                var required = new (string Label, string Path, bool IsDirectory)[]
                {
                    ("Settings.json", App.Settings.FileLocation, false),
                    ("State.json", App.State.FileLocation, false),
                    ("Logs", Paths.Logs, true),
                    ("Versions", Paths.Versions, true),
                    ("Modifications", Paths.Modifications, true)
                };

                var missing = required
                    .Where(x => x.IsDirectory
                        ? !Directory.Exists(x.Path)
                        : !File.Exists(x.Path))
                    .Select(x => x.Label)
                    .ToList();

                if (missing.Count == 0)
                {
                    result.Severity = DiagnosticSeverity.Healthy;
                    result.Summary = "All expected files are present";
                    result.Details = "";
                    return Task.CompletedTask;
                }

                // Logs and Modifications are created on demand, so treat those as warnings.
                bool onlyRecreatable = missing.All(
                    x => x is "Logs" or "Modifications" or "Versions"
                );

                result.Severity = onlyRecreatable
                    ? DiagnosticSeverity.Warning
                    : DiagnosticSeverity.Problem;

                result.Summary = onlyRecreatable
                    ? "Some folders will be recreated on demand"
                    : "Required files are missing";

                result.Details = string.Join(
                    "\n",
                    missing.Select(x => "- " + x)
                );
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckMissingFiles", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Missing file check failed";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckInvalidSettingsAsync()
        {
            var result = Get("invalid-settings");

            try
            {
                var settings = App.Settings.Prop;
                var issues = new List<string>();

                if (!ConfigurationMigrationRunner
                    .KnownProcessPriorities
                    .Contains(settings.RobloxProcessPriority))
                {
                    issues.Add(
                        $"RobloxProcessPriority '{settings.RobloxProcessPriority}' is not a valid priority."
                    );
                }

                if (!Locale.SupportedLocales.ContainsKey(settings.Locale))
                    issues.Add($"Locale '{settings.Locale}' is not a supported locale.");

                if (settings.BootstrapperIconCustomLocation.Length > 0 &&
                    !File.Exists(settings.BootstrapperIconCustomLocation))
                {
                    issues.Add(
                        "The custom bootstrapper icon path does not exist on disk."
                    );
                }

                if (settings.SelectedCustomTheme is not null &&
                    !File.Exists(
                        Path.Combine(Paths.CustomThemes, settings.SelectedCustomTheme)
                    ))
                {
                    issues.Add($"Custom theme '{settings.SelectedCustomTheme}' was not found.");
                }

                if (settings.AgentstrapMatchmakerEnabled &&
                    String.IsNullOrWhiteSpace(settings.AgentstrapMatchmakerPlaceId))
                {
                    issues.Add("The matchmaker is enabled but no place id is set.");
                }

                if (settings.AgentstrapMatchmakerMinimumFreeSlots < 0)
                    issues.Add("The matchmaker minimum free slots value is negative.");

                if (issues.Count == 0)
                {
                    result.Severity = DiagnosticSeverity.Healthy;
                    result.Summary = "No invalid settings detected";
                    result.Details = "";
                    return Task.CompletedTask;
                }

                var migration = ConfigurationMigrationRunner.Inspect();

                result.Severity = migration.NeedsMigration
                    ? DiagnosticSeverity.Problem
                    : DiagnosticSeverity.Warning;

                result.Summary = $"{issues.Count} invalid setting(s) detected";
                result.Details = string.Join("\n", issues.Select(x => "- " + x));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckInvalidSettings", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Invalid setting check failed";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckModConflictsAsync()
        {
            var result = Get("mod-conflicts");

            try
            {
                var conflicts = new List<string>();
                var pending = App.PendingSettingTasks
                    .Where(x => x.Value.Changed)
                    .Select(x => x.Value.ToString())
                    .ToList();

                if (SkyboxImageConverter.HasCustomSkybox() &&
                    Directory.Exists(Paths.CustomSkybox) &&
                    !Directory.EnumerateFiles(Paths.CustomSkybox, "*.png").Any())
                {
                    conflicts.Add(
                        "A custom skybox is recorded but the sky folder contains no images."
                    );
                }

                if (File.Exists(Paths.CustomFont))
                {
                    try
                    {
                        using var stream = File.OpenRead(Paths.CustomFont);

                        if (stream.Length == 0)
                            conflicts.Add("The custom font file is empty.");
                    }
                    catch (Exception ex)
                    {
                        conflicts.Add($"The custom font file could not be read: {ex.Message}");
                    }
                }

                if (pending.Count > 0)
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = $"{pending.Count} mod change(s) not yet applied";
                    result.Details = string.Join(
                        "\n",
                        pending.Select(x => "- " + x)
                    );

                    return Task.CompletedTask;
                }

                if (conflicts.Count == 0)
                {
                    result.Severity = DiagnosticSeverity.Healthy;
                    result.Summary = "No mod conflicts detected";
                    result.Details = "";
                    return Task.CompletedTask;
                }

                result.Severity = DiagnosticSeverity.Warning;
                result.Summary = $"{conflicts.Count} potential mod conflict(s)";
                result.Details = string.Join("\n", conflicts.Select(x => "- " + x));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckModConflicts", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Mod conflict check failed";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private Task CheckPermissionsAsync()
        {
            var result = Get("permissions");

            try
            {
                var issues = new List<string>();

                if (!CanWrite(Paths.Base))
                    issues.Add($"No write access to {Paths.Base}.");

                if (!CanWrite(Path.Combine(Paths.LocalAppData, "Roblox")))
                    issues.Add("No write access to the Roblox local app data folder.");

                if (!CanWrite(Paths.Temp))
                    issues.Add($"No write access to the temp folder {Paths.Temp}.");

                if (issues.Count == 0)
                {
                    result.Severity = DiagnosticSeverity.Healthy;
                    result.Summary = "Permissions are sufficient";
                    result.Details = "";
                    return Task.CompletedTask;
                }

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Permission problems detected";
                result.Details = string.Join("\n", issues.Select(x => "- " + x));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckPermissions", ex);

                result.Severity = DiagnosticSeverity.Problem;
                result.Summary = "Permission check failed";
                result.Details = ex.Message;
            }

            return Task.CompletedTask;
        }

        private async Task CheckUpdateStateAsync()
        {
            var result = Get("update-state");

            try
            {
                if (ConfigurationMigrationRunner.Inspect().NeedsMigration)
                {
                    result.Severity = DiagnosticSeverity.Problem;
                    result.Summary = "Configuration migrations are pending";
                    result.Details =
                        "Open the Update tab and apply the pending migrations.";
                    return;
                }

                if (!App.Settings.Prop.CheckForUpdates)
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = "Update checking is disabled";
                    result.Details =
                        "Agentstrap will not report on new releases while this is off.";
                    return;
                }

                var updater = new UpdateViewModel();
                await updater.CheckAgentstrapAsync();

                if (updater.UpdateAvailable)
                {
                    result.Severity = DiagnosticSeverity.Warning;
                    result.Summary = $"Version {updater.LatestVersion} is available";
                    result.Details =
                        $"Installed version is {updater.InstalledVersion}.";
                    return;
                }

                result.Severity = DiagnosticSeverity.Healthy;
                result.Summary = $"Up to date on {updater.InstalledVersion}";
                result.Details = updater.ReleaseSummary;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("DiagnosticsViewModel::CheckUpdateState", ex);

                result.Severity = DiagnosticSeverity.Warning;
                result.Summary = "Update state could not be confirmed";
                result.Details = ex.Message;
            }
        }

        private static bool CanWrite(string path)
        {
            try
            {
                Directory.CreateDirectory(path);

                string probe = Path.Combine(
                    path,
                    $".agentstrap-write-probe-{Guid.NewGuid():N}.tmp"
                );

                File.WriteAllText(probe, "probe");
                File.Delete(probe);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CopyReport()
        {
            string report = string.Join(
                Environment.NewLine,
                Results.Select(x =>
                    $"[{x.SeverityLabel}] {x.Title}: {x.Summary}"
                    + (x.HasDetails ? Environment.NewLine + x.Details : "")
                )
            );

            try
            {
                Clipboard.SetText(report);

                Frontend.ShowMessageBox(
                    "The diagnostics report was copied to the clipboard.",
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "The report could not be copied.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName!);

            return true;
        }
    }
}