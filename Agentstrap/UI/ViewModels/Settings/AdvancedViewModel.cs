using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Backs Agentstrap &gt; Settings &gt; Advanced. Hosts the NEXUS section and
    /// the experimental developer tools.
    /// </summary>
    public class AdvancedViewModel : NotifyPropertyChangedViewModel
    {
        private string _nexusStatus = "Disabled";
        private string _nexusDetails = "NEXUS is not enabled.";
        private string _developerStatus = "Locked";
        private string _developerDetails =
            "Developer tools require developer mode or NEXUS mode to be enabled.";
        private bool _isBusy;
        private string _logTail = "";

        /// <summary>
        /// NEXUS channels the user can pick between.
        /// </summary>
        public IReadOnlyList<string> NexusChannels { get; } = new[]
        {
            "stable",
            "preview",
            "nightly"
        };

        public ObservableCollection<DeveloperToolEntry> DeveloperTools { get; } = new();

        public bool NexusModeEnabled
        {
            get => App.Settings.Prop.NexusModeEnabled;

            set
            {
                if (value && !NexusModeEnabled)
                {
                    var result = Frontend.ShowMessageBox(
                        "NEXUS mode enables experimental behaviour that has not been " +
                        "fully tested. It can change how Agentstrap behaves during a launch.\n\n" +
                        "Enable it?",
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNo
                    );

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                App.Settings.Prop.NexusModeEnabled = value;

                RefreshNexus();
                RefreshDeveloperTools();

                OnPropertyChanged(nameof(NexusModeEnabled));
            }
        }

        public string NexusChannel
        {
            get => App.Settings.Prop.NexusChannel;

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    return;

                App.Settings.Prop.NexusChannel = value;

                RefreshNexus();

                OnPropertyChanged(nameof(NexusChannel));
            }
        }

        public bool NexusVerboseLoggingEnabled
        {
            get => App.Settings.Prop.NexusVerboseLogging;

            set
            {
                App.Settings.Prop.NexusVerboseLogging = value;
                OnPropertyChanged(nameof(NexusVerboseLoggingEnabled));
            }
        }

        public bool NexusAllowModOverrides
        {
            get => App.Settings.Prop.NexusAllowModOverrides;

            set
            {
                App.Settings.Prop.NexusAllowModOverrides = value;
                OnPropertyChanged(nameof(NexusAllowModOverrides));
            }
        }

        /// <summary>
        /// Developer tools are gated behind developer mode or NEXUS.
        /// </summary>
        public bool DeveloperToolsUnlocked =>
            App.Settings.Prop.DeveloperMode || App.Settings.Prop.NexusModeEnabled;

        public string NexusStatus
        {
            get => _nexusStatus;
            private set => Set(ref _nexusStatus, value);
        }

        public string NexusDetails
        {
            get => _nexusDetails;
            private set => Set(ref _nexusDetails, value);
        }

        public string DeveloperStatus
        {
            get => _developerStatus;
            private set => Set(ref _developerStatus, value);
        }

        public string DeveloperDetails
        {
            get => _developerDetails;
            private set => Set(ref _developerDetails, value);
        }

        public string LogTail
        {
            get => _logTail;
            private set => Set(ref _logTail, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(ref _isBusy, value);
        }

        public ICommand RefreshNexusCommand => new RelayCommand(RefreshNexus);

        public ICommand RefreshDeveloperToolsCommand =>
            new AsyncRelayCommand(RefreshDeveloperToolsAsync);

        public ICommand ResetNexusCommand =>
            new AsyncRelayCommand(ResetNexusAsync);

        public ICommand DumpConfigurationCommand =>
            new AsyncRelayCommand(DumpConfigurationAsync);

        public ICommand OpenLogsCommand =>
            new RelayCommand(() => UpdateViewModel.OpenFolder(Paths.Logs));

        public ICommand ClearLogsCommand =>
            new AsyncRelayCommand(ClearLogsAsync);

        public ICommand RestartAppCommand =>
            new RelayCommand(RestartApp);

        public ICommand CopyEnvironmentCommand =>
            new RelayCommand(CopyEnvironment);

        public AdvancedViewModel()
        {
            BuildDeveloperTools();

            RefreshNexus();
            RefreshDeveloperTools();
        }

        private void BuildDeveloperTools()
        {
            DeveloperTools.Clear();

            DeveloperTools.Add(new DeveloperToolEntry(
                "Dump runtime environment",
                "Write OS, runtime and install details to the log.",
                DumpConfigurationAsync
            ));

            DeveloperTools.Add(new DeveloperToolEntry(
                "Open log directory",
                "Open the folder containing Agentstrap logs.",
                () =>
                {
                    UpdateViewModel.OpenFolder(Paths.Logs);
                    return Task.CompletedTask;
                }
            ));

            DeveloperTools.Add(new DeveloperToolEntry(
                "Clear logs",
                "Delete every log file Agentstrap has written.",
                ClearLogsAsync
            ));

            DeveloperTools.Add(new DeveloperToolEntry(
                "Copy environment summary",
                "Copy an environment summary to the clipboard.",
                () =>
                {
                    CopyEnvironment();
                    return Task.CompletedTask;
                }
            ));

            DeveloperTools.Add(new DeveloperToolEntry(
                "Restart Agentstrap",
                "Relaunch Agentstrap using the current settings.",
                () =>
                {
                    RestartApp();
                    return Task.CompletedTask;
                }
            ));
        }

        private void RefreshNexus()
        {
            try
            {
                if (!NexusModeEnabled)
                {
                    NexusStatus = "Disabled";
                    NexusDetails =
                        "NEXUS is not enabled. Turn it on to opt in to experimental behaviour.";
                    OnPropertyChanged(nameof(NexusStatus));
                    OnPropertyChanged(nameof(NexusDetails));
                    return;
                }

                NexusStatus = $"Enabled on the {NexusChannel} channel";

                var notes = new List<string>
                {
                    "NEXUS may ship changes that are not production ready."
                };

                if (NexusVerboseLoggingEnabled)
                    notes.Add("Verbose logging is on, so logs will be more detailed.");

                if (NexusAllowModOverrides)
                    notes.Add("Mods are allowed to override NEXUS defaults.");

                NexusDetails = string.Join(" ", notes);

                OnPropertyChanged(nameof(NexusStatus));
                OnPropertyChanged(nameof(NexusDetails));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AdvancedViewModel::RefreshNexus", ex);

                NexusStatus = "Unavailable";
                NexusDetails = ex.Message;

                OnPropertyChanged(nameof(NexusStatus));
                OnPropertyChanged(nameof(NexusDetails));
            }
        }

        /// <summary>
        /// Developer tools are only usable once developer mode or NEXUS is on.
        /// </summary>
        private void RefreshDeveloperTools()
        {
            if (!DeveloperToolsUnlocked)
            {
                DeveloperStatus = "Locked";
                DeveloperDetails =
                    "Enable developer mode or NEXUS mode to use these tools.";
            }
            else
            {
                DeveloperStatus = "Unlocked";
                DeveloperDetails =
                    "These tools can modify configuration and delete files. Use them carefully.";
            }

            OnPropertyChanged(nameof(DeveloperStatus));
            OnPropertyChanged(nameof(DeveloperDetails));
            OnPropertyChanged(nameof(DeveloperToolsUnlocked));
        }

        private Task RefreshDeveloperToolsAsync()
        {
            RefreshDeveloperTools();

            return Task.CompletedTask;
        }

        private async Task ResetNexusAsync()
        {
            const string LOG_IDENT = "AdvancedViewModel::ResetNexus";

            var result = Frontend.ShowMessageBox(
                "This clears all NEXUS settings and returns them to their defaults.\n\n" +
                "Continue?",
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            IsBusy = true;

            try
            {
                App.Settings.Prop.NexusModeEnabled = false;
                App.Settings.Prop.NexusChannel = "stable";
                App.Settings.Prop.NexusVerboseLogging = false;
                App.Settings.Prop.NexusAllowModOverrides = false;

                App.Settings.Save();

                App.Logger.WriteLine(LOG_IDENT, "NEXUS settings reset");

                OnPropertyChanged(nameof(NexusModeEnabled));
                OnPropertyChanged(nameof(NexusChannel));
                OnPropertyChanged(nameof(NexusVerboseLoggingEnabled));
                OnPropertyChanged(nameof(NexusAllowModOverrides));

                RefreshNexus();
                await RefreshDeveloperToolsAsync();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    "NEXUS settings could not be reset.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DumpConfigurationAsync()
        {
            const string LOG_IDENT = "AdvancedViewModel::DumpConfiguration";

            IsBusy = true;

            try
            {
                var lines = new List<string>
                {
                    $"{App.ProjectName} v{App.Version}",
                    $"OS: {Environment.OSVersion}",
                    $"Process: {Paths.Process}",
                    $"Base: {Paths.Base}",
                    $"Temp: {Paths.Temp}",
                    $"Developer mode: {App.Settings.Prop.DeveloperMode}",
                    $"NEXUS: {App.Settings.Prop.NexusModeEnabled} ({App.Settings.Prop.NexusChannel})",
                    $"Web environment: {App.Settings.Prop.WebEnvironment}",
                    $"Player installed: {App.IsPlayerInstalled}",
                    $"Studio installed: {App.IsStudioInstalled}",
                    $"Settings loaded: {App.Settings.Loaded}"
                };

                foreach (string line in lines)
                    App.Logger.WriteLine(LOG_IDENT, line);

                LogTail = string.Join(Environment.NewLine, lines);
                OnPropertyChanged(nameof(LogTail));

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    "The environment could not be dumped.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClearLogsAsync()
        {
            const string LOG_IDENT = "AdvancedViewModel::ClearLogs";

            var result = Frontend.ShowMessageBox(
                "Every Agentstrap log file will be deleted. This cannot be undone.\n\n" +
                "Continue?",
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            IsBusy = true;

            try
            {
                if (!Directory.Exists(Paths.Logs))
                {
                    Frontend.ShowMessageBox(
                        "There is no log folder to clear.",
                        MessageBoxImage.Information
                    );

                    return;
                }

                int deleted = 0;

                foreach (string file in Directory.EnumerateFiles(Paths.Logs))
                {
                    try
                    {
                        File.Delete(file);
                        deleted++;
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException($"{LOG_IDENT}({file})", ex);
                    }
                }

                App.Logger.WriteLine(LOG_IDENT, $"Deleted {deleted} log file(s)");

                Frontend.ShowMessageBox(
                    $"Deleted {deleted} log file(s).",
                    MessageBoxImage.Information
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    "The logs could not be cleared.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void RestartApp()
        {
            const string LOG_IDENT = "AdvancedViewModel::RestartApp";

            try
            {
                App.Settings.Save();
                App.State.Save();
                App.FastFlags.Save();

                App.Logger.WriteLine(LOG_IDENT, "Restart requested");

                Process.Start(new ProcessStartInfo
                {
                    FileName = Paths.Application,
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    "Agentstrap could not restart.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
        }

        private void CopyEnvironment()
        {
            string summary =
                $"{App.ProjectName} v{App.Version}{Environment.NewLine}" +
                $"OS: {Environment.OSVersion}{Environment.NewLine}" +
                $"Install: {Paths.Base}{Environment.NewLine}" +
                $"Developer mode: {App.Settings.Prop.DeveloperMode}{Environment.NewLine}" +
                $"NEXUS: {App.Settings.Prop.NexusModeEnabled} ({App.Settings.Prop.NexusChannel})";

            try
            {
                Clipboard.SetText(summary);

                Frontend.ShowMessageBox(
                    "The environment summary was copied to the clipboard.",
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "The summary could not be copied.\n\n" + ex.Message,
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

    /// <summary>
    /// A single experimental developer tool shown in the Advanced page.
    /// </summary>
    public class DeveloperToolEntry
    {
        public string Name { get; }

        public string Description { get; }

        /// <summary>
        /// Wraps the tool so it can be bound directly to a control command.
        /// </summary>
        public ICommand Command { get; }

        public DeveloperToolEntry(
            string name,
            string description,
            Func<Task> action
        )
        {
            Name = name;
            Description = description;
            Command = new AsyncRelayCommand(action);
        }
    }
}