using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Agentstrap.UI.Elements.Settings.Pages
{
    public partial class AccountsPage : Wpf.Ui.Controls.UiPage
    {
        private readonly Agentstrap.Accounts.RobloxAccountManager _manager;

        public AccountsPage()
        {
            InitializeComponent();
            string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Agentstrap", "Accounts");
            _manager = new Agentstrap.Accounts.RobloxAccountManager(dataDir);
            RefreshList();
        }

        private void RefreshList()
        {
            AccountList.Items.Clear();
            foreach (var acc in _manager.Accounts)
            {
                string active = _manager.ActiveAccountId == acc.UserId ? " (active)" : "";
                AccountList.Items.Add($"{acc.Username} ({acc.UserId}){active}");
            }
            StatusText.Text = _manager.Accounts.Count == 0 ? "No accounts saved." : $"{_manager.Accounts.Count} account(s).";
            _ = UpdateAvatarAsync();
        }

        private async void QuickSign_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Creating quick sign-in code...";
            var cts = new CancellationTokenSource();
            try
            {
                var account = await _manager.AddAccountByQuickSignInAsync(code =>
                {
                    Dispatcher.Invoke(() => { StatusText.Text = $"Enter this code on your logged-in Roblox device: {code}"; QuickCodeBox.Text = code; });
                }, cts.Token);

                if (account is null)
                {
                    StatusText.Text = "Quick sign-in failed or cancelled.";
                    return;
                }

                RefreshList();
                StatusText.Text = $"Added {account.Username}.";
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Quick sign-in cancelled.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Quick sign-in error: " + ex.Message;
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _manager.LogoutActiveAccountAsync();
            RefreshList();
            await UpdateAvatarAsync();
            StatusText.Text = "Logged out.";
        }

        private void CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(QuickCodeBox.Text))
            {
                Clipboard.SetText(QuickCodeBox.Text);
                StatusText.Text = "Code copied.";
            }
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            int idx = AccountList.SelectedIndex;
            if (idx < 0 || idx >= _manager.Accounts.Count)
            {
                StatusText.Text = "Select an account.";
                return;
            }
            var acc = _manager.Accounts[idx];
            if (_manager.SetActiveAccount(acc.UserId))
            {
                RefreshList();
                StatusText.Text = $"Switched to {acc.Username}. Roblox will use this account when launched.";
            }
        }

        private async Task UpdateAvatarAsync()
        {
            try
            {
                var active = _manager.GetActiveAccount();
                if (active is null)
                {
                    AvatarImage.Source = null;
                    return;
                }
                using var client = new HttpClient();
                string url = $"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={active.UserId}&size=150x150&format=Png&isCircular=true";
                string json = await client.GetStringAsync(url);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                if (data.GetArrayLength() == 0) return;
                string imageUrl = data[0].GetProperty("imageUrl").GetString() ?? "";
                if (string.IsNullOrWhiteSpace(imageUrl)) return;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(imageUrl);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                AvatarImage.Source = bmp;
            }
            catch
            {
                AvatarImage.Source = null;
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            int idx = AccountList.SelectedIndex;
            if (idx < 0 || idx >= _manager.Accounts.Count)
            {
                StatusText.Text = "Select an account.";
                return;
            }
            var acc = _manager.Accounts[idx];
            if (MessageBox.Show($"Remove {acc.Username}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _manager.RemoveAccount(acc.UserId);
                RefreshList();
                StatusText.Text = $"Removed {acc.Username}.";
            }
        }
    }
}