using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Backs the Quality of Life page: update management plus custom window
    /// attributes for the Roblox player window.
    /// </summary>
    public class QoLViewModel : NotifyPropertyChangedViewModel
    {
        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;

            set
            {
                App.Settings.Prop.CheckForUpdates = value;
                OnPropertyChanged(nameof(UpdateCheckingEnabled));
            }
        }

        public bool BackgroundUpdatesEnabled
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;

            set
            {
                App.Settings.Prop.BackgroundUpdatesEnabled = value;
                OnPropertyChanged(nameof(BackgroundUpdatesEnabled));
            }
        }

        public bool CustomWindowAttributesEnabled
        {
            get => App.Settings.Prop.CustomWindowAttributesEnabled;

            set
            {
                App.Settings.Prop.CustomWindowAttributesEnabled = value;
                OnPropertyChanged(nameof(CustomWindowAttributesEnabled));
            }
        }

        public string WindowWidth
        {
            get => App.Settings.Prop.WindowWidth?.ToString() ?? "";

            set
            {
                App.Settings.Prop.WindowWidth = ParsePositive(value);
                OnPropertyChanged(nameof(WindowWidth));
            }
        }

        public string WindowHeight
        {
            get => App.Settings.Prop.WindowHeight?.ToString() ?? "";

            set
            {
                App.Settings.Prop.WindowHeight = ParsePositive(value);
                OnPropertyChanged(nameof(WindowHeight));
            }
        }

        public string WindowPositionX
        {
            get => App.Settings.Prop.WindowPositionX?.ToString() ?? "";

            set
            {
                App.Settings.Prop.WindowPositionX = ParseOptional(value);
                OnPropertyChanged(nameof(WindowPositionX));
            }
        }

        public string WindowPositionY
        {
            get => App.Settings.Prop.WindowPositionY?.ToString() ?? "";

            set
            {
                App.Settings.Prop.WindowPositionY = ParseOptional(value);
                OnPropertyChanged(nameof(WindowPositionY));
            }
        }

        public string WindowTitle
        {
            get => App.Settings.Prop.WindowTitle;

            set
            {
                App.Settings.Prop.WindowTitle = value ?? "";
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public ICommand ResetWindowAttributesCommand => new RelayCommand(ResetWindowAttributes);

        private void ResetWindowAttributes()
        {
            App.Settings.Prop.WindowWidth = null;
            App.Settings.Prop.WindowHeight = null;
            App.Settings.Prop.WindowPositionX = null;
            App.Settings.Prop.WindowPositionY = null;
            App.Settings.Prop.WindowTitle = "";

            OnPropertyChanged(nameof(WindowWidth));
            OnPropertyChanged(nameof(WindowHeight));
            OnPropertyChanged(nameof(WindowPositionX));
            OnPropertyChanged(nameof(WindowPositionY));
            OnPropertyChanged(nameof(WindowTitle));
        }

        private static int? ParseOptional(string? value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            return Int32.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
                ? result
                : null;
        }

        private static int? ParsePositive(string? value)
        {
            int? result = ParseOptional(value);

            return result is > 0 ? result : null;
        }
    }
}
