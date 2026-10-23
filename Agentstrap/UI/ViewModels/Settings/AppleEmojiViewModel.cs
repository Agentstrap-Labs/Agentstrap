using System.IO;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Agentstrap.Enums;
using Agentstrap.Utility;

namespace Agentstrap.UI.ViewModels.Settings
{
    public class AppleEmojiViewModel : NotifyPropertyChangedViewModel
    {
        private readonly AppleEmojiModeService _service = AppleEmojiMode.Default;

        private AppleEmojiModeStatus _status = AppleEmojiModeStatus.Disabled;
        private string _statusMessage = "Apple Emoji Mode is disabled.";
        private string _selectedFontPath = "";
        private string _appliedHash = "";
        private bool _robloxDetected;
        private string _diagnosticReport = "";

        public AppleEmojiViewModel()
        {
            _selectedFontPath = App.Settings.Prop.RobloxAppleEmojiModeFontPath ?? "";
            _appliedHash = App.Settings.Prop.RobloxAppleEmojiModeAppliedHash ?? "";

            Refresh();
        }

        public AppleEmojiModeStatus Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                    return;

                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage == value)
                    return;

                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string SelectedFontPath
        {
            get => _selectedFontPath;
            set
            {
                if (_selectedFontPath == value)
                    return;

                _selectedFontPath = value ?? "";
                OnPropertyChanged(nameof(SelectedFontPath));
                OnPropertyChanged(nameof(SelectedFontName));
                Refresh();
            }
        }

        public string SelectedFontName =>
            String.IsNullOrWhiteSpace(SelectedFontPath)
                ? "No font selected"
                : Path.GetFileName(SelectedFontPath);

        public string AppliedHash
        {
            get => _appliedHash;
            private set
            {
                if (_appliedHash == value)
                    return;

                _appliedHash = value ?? "";
                OnPropertyChanged(nameof(AppliedHash));
            }
        }

        public bool RobloxDetected
        {
            get => _robloxDetected;
            private set
            {
                if (_robloxDetected == value)
                    return;

                _robloxDetected = value;
                OnPropertyChanged(nameof(RobloxDetected));
            }
        }

        public bool IsEnabled => Status == AppleEmojiModeStatus.EnabledAndApplied;

        public string StatusLabel => Status switch
        {
            AppleEmojiModeStatus.Disabled => "Disabled",
            AppleEmojiModeStatus.EnabledAndApplied => "Enabled and applied",
            AppleEmojiModeStatus.SetupRequired => "Setup required",
            AppleEmojiModeStatus.RobloxNotDetected => "Roblox not detected",
            AppleEmojiModeStatus.AssetsUnavailable => "Required assets unavailable",
            AppleEmojiModeStatus.RenderingMethodUnsupported => "Rendering method unsupported",
            AppleEmojiModeStatus.RenderingUnverified => "Rendering unverified",
            AppleEmojiModeStatus.RenderingVerified => "Rendering verified",
            AppleEmojiModeStatus.Failed => "Operation failed",
            _ => "Unknown"
        };

        public string DiagnosticReport
        {
            get => _diagnosticReport;
            private set
            {
                if (_diagnosticReport == value)
                    return;

                _diagnosticReport = value;
                OnPropertyChanged(nameof(DiagnosticReport));
            }
        }

        public string SimulatedPreview =>
            "Simulated preview only: 😀 😃 😄 😁 😆 😅 😂 🤣 🙂 😉 😊 😇 🥰 😍 🤩 😘";

        public ICommand SelectFontCommand => new RelayCommand(SelectFont);

        public ICommand EnableCommand => new RelayCommand(Enable);

        public ICommand DisableCommand => new RelayCommand(Disable);

        public ICommand RestoreDefaultsCommand => new RelayCommand(RestoreDefaults);

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public ICommand CopyDiagnosticCommand => new RelayCommand(CopyDiagnostic);

        private void SelectFont()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Emoji font files|*.ttf;*.otf;*.ttc|All files|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            var validation = _service.ValidateFontFile(dialog.FileName);

            if (!validation.IsValid)
            {
                Frontend.ShowMessageBox(
                    validation.Error ?? "The selected font is not valid.",
                    MessageBoxImage.Error
                );

                return;
            }

            SelectedFontPath = dialog.FileName;
        }

        private void Enable()
        {
            if (String.IsNullOrWhiteSpace(SelectedFontPath))
            {
                SelectFont();

                if (String.IsNullOrWhiteSpace(SelectedFontPath))
                    return;
            }

            var result = _service.Enable(SelectedFontPath);

            Status = result.Status;
            StatusMessage = result.Message;

            if (result.Status == AppleEmojiModeStatus.EnabledAndApplied)
            {
                AppliedHash = result.Hash ?? AppliedHash;

                App.Settings.Prop.RobloxAppleEmojiModeEnabled = true;
                App.Settings.Prop.RobloxAppleEmojiModeFontPath = SelectedFontPath;
                App.Settings.Prop.RobloxAppleEmojiModeAppliedHash = AppliedHash;

                App.Settings.Save();
            }

            Refresh();
        }

        private void Disable()
        {
            var result = _service.Disable(AppliedHash);

            Status = result.Status;
            StatusMessage = result.Message;

            if (result.Status == AppleEmojiModeStatus.Disabled)
            {
                App.Settings.Prop.RobloxAppleEmojiModeEnabled = false;
                App.Settings.Prop.RobloxAppleEmojiModeAppliedHash = "";
                AppliedHash = "";

                App.Settings.Save();
            }

            Refresh();
        }

        private void RestoreDefaults()
        {
            Disable();

            SelectedFontPath = "";
            App.Settings.Prop.RobloxAppleEmojiModeFontPath = "";
            App.Settings.Save();

            Status = AppleEmojiModeStatus.Disabled;
            StatusMessage = "Apple Emoji Mode was restored to its default, disabled state.";
            Refresh();
        }

        private void Refresh()
        {
            RobloxDetected = _service.IsRobloxInstalled();

            Status = _service.GetStatus(
                App.Settings.Prop.RobloxAppleEmojiModeEnabled,
                SelectedFontPath,
                AppliedHash
            );

            StatusMessage = Status switch
            {
                AppleEmojiModeStatus.Disabled => "Apple Emoji Mode is disabled.",
                AppleEmojiModeStatus.EnabledAndApplied => "Apple Emoji Mode is applied. Roblox rendering remains unverified.",
                AppleEmojiModeStatus.SetupRequired => "Setup is required before Apple Emoji Mode can run.",
                AppleEmojiModeStatus.RobloxNotDetected => "Roblox Player was not detected. No files were changed.",
                AppleEmojiModeStatus.AssetsUnavailable => "The selected Apple style emoji font is unavailable or invalid.",
                AppleEmojiModeStatus.RenderingMethodUnsupported => "This Roblox client does not support the required emoji font replacement method.",
                AppleEmojiModeStatus.RenderingUnverified => "Apple Emoji Mode is applied, but rendering inside Roblox is unverified.",
                AppleEmojiModeStatus.RenderingVerified => "Apple Emoji Mode rendering was verified.",
                AppleEmojiModeStatus.Failed => "The last Apple Emoji Mode operation failed.",
                _ => "Apple Emoji Mode status is unknown."
            };

            DiagnosticReport = _service.CreateDiagnosticReport(
                App.Settings.Prop.RobloxAppleEmojiModeEnabled,
                SelectedFontPath,
                AppliedHash,
                Status
            );
        }

        private void CopyDiagnostic()
        {
            try
            {
                Clipboard.SetText(DiagnosticReport);

                Frontend.ShowMessageBox(
                    "The Apple Emoji Mode diagnostic report was copied to the clipboard.",
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "The diagnostic report could not be copied.\n\n" + ex.Message,
                    MessageBoxImage.Error
                );
            }
        }
    }
}