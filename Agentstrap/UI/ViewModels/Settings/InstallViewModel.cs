using System.IO;
using System.Windows;
using System.Windows.Input;
using Agentstrap.AppData;
using Agentstrap.Enums;
using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.UI.ViewModels.Settings
{
    public class InstallViewModel : NotifyPropertyChangedViewModel
    {
        private static RobloxPlayerData PlayerData => new();

        private static RobloxStudioData StudioData => new();

        private bool _isInstalling;
        private string _installStatus = "";

        public bool IsInstalling
        {
            get => _isInstalling;
            private set
            {
                if (_isInstalling == value)
                    return;
                _isInstalling = value;
                OnPropertyChanged(nameof(IsInstalling));
            }
        }

        public string InstallStatus
        {
            get => _installStatus;
            private set
            {
                if (_installStatus == value)
                    return;
                _installStatus = value;
                OnPropertyChanged(nameof(InstallStatus));
            }
        }

        public string RobloxStatus =>
            File.Exists(PlayerData.ExecutablePath) ? "Installed" : "Not installed";

        public string RobloxDetails =>
            File.Exists(PlayerData.ExecutablePath)
                ? "Roblox Player files are ready for Agentstrap."
                : "Roblox Player will be installed when you launch it.";

        public string StudioStatus =>
            File.Exists(StudioData.ExecutablePath) ? "Installed" : "Not installed";

        public string StudioDetails =>
            File.Exists(StudioData.ExecutablePath)
                ? "Roblox Studio files are ready for Agentstrap."
                : "Roblox Studio will be installed when you launch it.";

        public ICommand InstallRobloxCommand =>
            new RelayCommand(() => LaunchRoblox(LaunchMode.Player, false));

        public ICommand ReinstallRobloxCommand =>
            new RelayCommand(() => LaunchRoblox(LaunchMode.Player, true));

        public ICommand InstallStudioCommand =>
            new RelayCommand(() => LaunchRoblox(LaunchMode.Studio, false));

        public ICommand ReinstallStudioCommand =>
            new RelayCommand(() => LaunchRoblox(LaunchMode.Studio, true));

        private void LaunchRoblox(LaunchMode mode, bool forceReinstall)
        {
            IsInstalling = true;

            if (mode == LaunchMode.Player)
                InstallStatus = forceReinstall
                    ? "Reinstalling Roblox Player..."
                    : "Installing Roblox Player...";
            else
                InstallStatus = forceReinstall
                    ? "Reinstalling Roblox Studio..."
                    : "Installing Roblox Studio...";

            if (forceReinstall)
            {
                App.State.Prop.ForceReinstall = true;
                App.State.Save();
            }

            LaunchHandler.LaunchRoblox(mode);
        }
    }
}