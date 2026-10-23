using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Agentstrap.Enums.FlagPresets;

namespace Agentstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private Dictionary<string, object>? _preResetFlags;

        public event EventHandler? RequestPageReloadEvent;
        
        public event EventHandler? OpenFlagEditorEvent;

        private void OpenFastFlagEditor() => OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        private FastFlagPreset? _selectedPreset;

        public IReadOnlyList<FastFlagPreset> FastFlagPresets => FastFlagManager.Presets;

        public FastFlagPreset? SelectedPreset
        {
            get => _selectedPreset;

            set
            {
                _selectedPreset = value;
                OnPropertyChanged(nameof(SelectedPreset));
                OnPropertyChanged(nameof(SelectedPresetDescription));
            }
        }

        public string SelectedPresetDescription => _selectedPreset?.Description ?? "";

        public ICommand ApplyPresetCommand => new RelayCommand(ApplyPreset);

        private void ApplyPreset()
        {
            if (_selectedPreset is null)
                return;

            App.FastFlags.ApplyPreset(_selectedPreset);

            RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
        }

        private bool _hasValidated;
        private string _validationSummary = "";

        public string ValidationSummary
        {
            get => _validationSummary;

            private set
            {
                _validationSummary = value;
                OnPropertyChanged(nameof(ValidationSummary));
            }
        }

        public Visibility ValidationVisibility => _hasValidated ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ValidateFlagsCommand => new RelayCommand(ValidateFlags);

        private void ValidateFlags()
        {
            var invalid = App.FastFlags.Validate();

            _hasValidated = true;

            if (invalid.Count == 0)
            {
                ValidationSummary = $"All {App.FastFlags.Prop.Count} fast flags are valid.";
            }
            else
            {
                var lines = invalid.Take(15)
                    .Select(x => $"  {x.Name} = {x.Value} ({x.Error})");

                string suffix = invalid.Count > 15 ? $"\n  ...and {invalid.Count - 15} more." : "";

                ValidationSummary =
                    $"{invalid.Count} of {App.FastFlags.Prop.Count} fast flags look invalid:\n" +
                    string.Join("\n", lines) + suffix;
            }

            OnPropertyChanged(nameof(ValidationSummary));
            OnPropertyChanged(nameof(ValidationVisibility));
        }

        public Visibility CanShowFastFlagEditor => App.IsStudioInstalled ? Visibility.Visible : Visibility.Collapsed;

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
        }

        public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

        public TextureQuality SelectedTextureQuality
        {
            get => TextureQualities.Where(x => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
            set
            {
                if (value == TextureQuality.Default)
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality", null);
                }
                else
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
                }
            }
        }
        public bool ResetConfiguration
        {
            get => _preResetFlags is not null;

            set
            {
                if (value)
                {
                    _preResetFlags = new(App.FastFlags.Prop);
                    App.FastFlags.Prop.Clear();
                }
                else
                {
                    App.FastFlags.Prop = _preResetFlags!;
                    _preResetFlags = null;
                }

                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

