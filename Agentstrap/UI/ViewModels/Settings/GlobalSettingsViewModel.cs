using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Backs the "Global Settings" page: edits Roblox's own global user
    /// settings (graphics, volume, etc.) through Agentstrap's
    /// <see cref="App.GlobalSettings"/> manager.
    /// </summary>
    public class GlobalSettingsViewModel : NotifyPropertyChangedViewModel
    {
        public int GraphicsQualityLevel
        {
            get => App.GlobalSettings.Prop.SavedQualityLevel;

            set
            {
                App.GlobalSettings.Prop.GraphicsQualityLevel = value;
                App.GlobalSettings.Prop.SavedQualityLevel = value;
                if (value <= 0)
                    App.GlobalSettings.Prop.MaxQualityEnabled = true;
                else
                    App.GlobalSettings.Prop.MaxQualityEnabled = false;

                OnPropertyChanged(nameof(GraphicsQualityLevel));
                OnPropertyChanged(nameof(SavedQualityLevel));
                OnPropertyChanged(nameof(MaxQualityEnabled));
            }
        }

        public int SavedQualityLevel
        {
            get => App.GlobalSettings.Prop.SavedQualityLevel;

            set
            {
                App.GlobalSettings.Prop.GraphicsQualityLevel = value;
                App.GlobalSettings.Prop.SavedQualityLevel = value;
                if (value <= 0)
                    App.GlobalSettings.Prop.MaxQualityEnabled = true;
                else
                    App.GlobalSettings.Prop.MaxQualityEnabled = false;

                OnPropertyChanged(nameof(GraphicsQualityLevel));
                OnPropertyChanged(nameof(SavedQualityLevel));
                OnPropertyChanged(nameof(MaxQualityEnabled));
            }
        }

        public bool MaxQualityEnabled
        {
            get => App.GlobalSettings.Prop.MaxQualityEnabled;

            set
            {
                App.GlobalSettings.Prop.MaxQualityEnabled = value;

                if (value)
                {
                    App.GlobalSettings.Prop.GraphicsQualityLevel = 0;
                    App.GlobalSettings.Prop.SavedQualityLevel = 0;
                }
                else if (App.GlobalSettings.Prop.SavedQualityLevel <= 0)
                {
                    App.GlobalSettings.Prop.GraphicsQualityLevel = 5;
                    App.GlobalSettings.Prop.SavedQualityLevel = 5;
                }

                OnPropertyChanged(nameof(MaxQualityEnabled));
                OnPropertyChanged(nameof(GraphicsQualityLevel));
                OnPropertyChanged(nameof(SavedQualityLevel));
            }
        }

        public double MasterVolume
        {
            get => App.GlobalSettings.Prop.MasterVolume;

            set
            {
                App.GlobalSettings.Prop.MasterVolume = (float)value;
                OnPropertyChanged(nameof(MasterVolume));
            }
        }

        public int FramerateCap
        {
            get => App.GlobalSettings.Prop.FramerateCap;

            set
            {
                App.GlobalSettings.Prop.FramerateCap = value;
                OnPropertyChanged(nameof(FramerateCap));
            }
        }

        public bool Fullscreen
        {
            get => App.GlobalSettings.Prop.Fullscreen;

            set
            {
                App.GlobalSettings.Prop.Fullscreen = value;
                OnPropertyChanged(nameof(Fullscreen));
            }
        }

        public double MouseSensitivity
        {
            get => App.GlobalSettings.Prop.MouseSensitivity;

            set
            {
                App.GlobalSettings.Prop.MouseSensitivity = (float)value;
                OnPropertyChanged(nameof(MouseSensitivity));
            }
        }

        public double MasterVolumeStudio
        {
            get => App.GlobalSettings.Prop.MasterVolumeStudio;

            set
            {
                App.GlobalSettings.Prop.MasterVolumeStudio = (float)value;
                OnPropertyChanged(nameof(MasterVolumeStudio));
            }
        }

        public ICommand ResetGlobalSettingsCommand => new RelayCommand(ResetGlobalSettings);

        private void ResetGlobalSettings()
        {
            App.GlobalSettings.Prop.GraphicsQualityLevel = 0;
            App.GlobalSettings.Prop.SavedQualityLevel = 8;
            App.GlobalSettings.Prop.MaxQualityEnabled = true;
            App.GlobalSettings.Prop.MasterVolume = 0.7f;
            App.GlobalSettings.Prop.FramerateCap = 9999;
            App.GlobalSettings.Prop.Fullscreen = true;
            App.GlobalSettings.Prop.MouseSensitivity = 1f;
            App.GlobalSettings.Prop.MasterVolumeStudio = 1f;

            OnPropertyChanged(nameof(GraphicsQualityLevel));
            OnPropertyChanged(nameof(SavedQualityLevel));
            OnPropertyChanged(nameof(MaxQualityEnabled));
            OnPropertyChanged(nameof(MasterVolume));
            OnPropertyChanged(nameof(FramerateCap));
            OnPropertyChanged(nameof(Fullscreen));
            OnPropertyChanged(nameof(MouseSensitivity));
            OnPropertyChanged(nameof(MasterVolumeStudio));
        }
    }
}
