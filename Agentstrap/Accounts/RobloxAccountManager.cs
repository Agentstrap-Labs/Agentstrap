namespace Agentstrap.Accounts
{
    public sealed class RobloxAccountManager
    {
        public string DataDirectory { get; }
        public SecureCredentialStore Credentials { get; }
        private readonly string _metadataPath;

        public List<RobloxAccount> Accounts { get; private set; } = new();
        public long? ActiveAccountId { get; private set; }

        public RobloxAccountManager(string dataDirectory)
        {
            DataDirectory = dataDirectory;
            Directory.CreateDirectory(dataDirectory);
            Credentials = new SecureCredentialStore(Path.Combine(dataDirectory, "credentials"));
            _metadataPath = Path.Combine(dataDirectory, "accounts.json");
            Load();
        }

        public void Load()
        {
            Accounts = new List<RobloxAccount>();
            ActiveAccountId = null;
            if (!File.Exists(_metadataPath)) return;
            try
            {
                string json = File.ReadAllText(_metadataPath);
                var data = JsonSerializer.Deserialize<AccountManagerData>(json);
                if (data is null) return;
                Accounts = data.Accounts ?? new List<RobloxAccount>();
                ActiveAccountId = data.ActiveAccountId;
            }
            catch
            {
                // keep empty on corrupt
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(DataDirectory);
            var data = new AccountManagerData
            {
                Accounts = Accounts,
                ActiveAccountId = ActiveAccountId,
                LastUpdated = DateTime.UtcNow
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_metadataPath, json);
        }

        public async Task<RobloxAccount?> AddAccountAsync(string rawCookie, Func<string, Task<AuthenticatedRobloxUser?>> validator)
        {
            if (validator is null) throw new ArgumentNullException(nameof(validator));
            string token = NormalizeCookie(rawCookie);
            var user = await validator(token);
            if (user is null || user.Id <= 0) return null;

            var existing = Accounts.FirstOrDefault(a => a.UserId == user.Id);
            if (existing is not null)
            {
                Credentials.Save(existing.UserId, token);
                ActiveAccountId = existing.UserId;
                Save();
                return existing;
            }

            var account = new RobloxAccount
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            };
            Accounts.Add(account);
            Credentials.Save(account.UserId, token);
            if (ActiveAccountId is null) ActiveAccountId = account.UserId;
            Save();
            return account;
        }

        public bool RemoveAccount(long userId)
        {
            var account = Accounts.FirstOrDefault(a => a.UserId == userId);
            if (account is null) return false;
            Accounts.Remove(account);
            Credentials.Remove(userId);
            if (ActiveAccountId == userId)
                ActiveAccountId = Accounts.FirstOrDefault()?.UserId;
            Save();
            return true;
        }

        public bool SetActiveAccount(long userId)
        {
            if (!Accounts.Any(a => a.UserId == userId)) return false;
            ActiveAccountId = userId;
            Save();
            return true;
        }

        public RobloxAccount? GetActiveAccount() =>
            ActiveAccountId is null ? null : Accounts.FirstOrDefault(a => a.UserId == ActiveAccountId);

        public async Task LogoutActiveAccountAsync()
        {
            var active = GetActiveAccount();
            if (active is null) return;

            string? cookie = Credentials.Load(active.UserId);
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                try
                {
                    using var client = new HttpClient();
                    string csrf = await GetCsrfTokenStaticAsync(client);
                    using var req = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/logout");
                    req.Headers.Add("Cookie", ".ROBLOSECURITY=" + cookie);
                    if (!string.IsNullOrWhiteSpace(csrf))
                        req.Headers.Add("x-csrf-token", csrf);
                    await client.SendAsync(req);
                }
                catch
                {
                    // ignore network errors during logout
                }
            }

            ActiveAccountId = null;
            Save();
        }

        private static async Task<string> GetCsrfTokenStaticAsync(HttpClient client)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/logout");
                var resp = await client.SendAsync(req);
                if (resp.Headers.TryGetValues("x-csrf-token", out var vals))
                    return vals.FirstOrDefault() ?? "";
                return "";
            }
            catch
            {
                return "";
            }
        }

        public string? GetCredential(long userId) => Credentials.Load(userId);

        public async Task<RobloxAccount?> AddAccountByQuickSignInAsync(Action<string>? onCodeCreated, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            string csrf = await GetCsrfTokenAsync(client);

            // create token
            using var createReq = new HttpRequestMessage(HttpMethod.Post, "https://apis.roblox.com/auth-token-service/v1/login/create");
            createReq.Headers.Add("x-csrf-token", csrf);
            var createResp = await client.SendAsync(createReq, cancellationToken);
            if (!createResp.IsSuccessStatusCode) return null;

            string createJson = await createResp.Content.ReadAsStringAsync(cancellationToken);
            using var createDoc = JsonDocument.Parse(createJson);
            string code = createDoc.RootElement.GetProperty("code").GetString() ?? "";
            string privateKey = createDoc.RootElement.GetProperty("privateKey").GetString() ?? "";
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(privateKey)) return null;

            onCodeCreated?.Invoke(code);

            // poll status
            string status = "Created";
            while (!cancellationToken.IsCancellationRequested)
            {
                var statusPayload = new { code, privateKey };
                using var statusReq = new HttpRequestMessage(HttpMethod.Post, "https://apis.roblox.com/auth-token-service/v1/login/status")
                {
                    Content = new StringContent(JsonSerializer.Serialize(statusPayload), Encoding.UTF8, "application/json")
                };
                statusReq.Headers.Add("x-csrf-token", csrf);
                var statusResp = await client.SendAsync(statusReq, cancellationToken);

                if (statusResp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    if (statusResp.Headers.TryGetValues("x-csrf-token", out var vals))
                    {
                        csrf = vals.FirstOrDefault() ?? csrf;
                    }
                    continue;
                }

                if (!statusResp.IsSuccessStatusCode)
                {
                    // CodeInvalid etc.
                    return null;
                }

                string statusJson = await statusResp.Content.ReadAsStringAsync(cancellationToken);
                using var statusDoc = JsonDocument.Parse(statusJson);
                status = statusDoc.RootElement.GetProperty("status").GetString() ?? "Created";

                if (status == "Validated") break;
                if (status == "Cancelled") return null;

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) return null;

            // login
            string loginCsrf = await GetCsrfTokenAsync(client);
            var loginPayload = new { ctype = "AuthToken", cvalue = code, password = privateKey };
            using var loginReq = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/login")
            {
                Content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json")
            };
            loginReq.Headers.Add("x-csrf-token", loginCsrf);

            var loginResp = await client.SendAsync(loginReq, cancellationToken);
            if (!loginResp.IsSuccessStatusCode)
            {
                // retry with csrf from 403
                if (loginResp.Headers.TryGetValues("x-csrf-token", out var lvals))
                {
                    loginCsrf = lvals.FirstOrDefault() ?? loginCsrf;
                    using var retryReq = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/login")
                    {
                        Content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json")
                    };
                    retryReq.Headers.Add("x-csrf-token", loginCsrf);
                    loginResp = await client.SendAsync(retryReq, cancellationToken);
                }
            }

            if (!loginResp.IsSuccessStatusCode) return null;

            // extract cookie
            string? cookie = null;
            if (loginResp.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var c in cookies)
                {
                    if (c.Contains(".ROBLOSECURITY=", StringComparison.Ordinal))
                    {
                        cookie = c.Split(".ROBLOSECURITY=")[1].Split(';')[0].Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(cookie)) return null;

            // parse user info from response if present
            string loginJson = await loginResp.Content.ReadAsStringAsync(cancellationToken);
            long userId = 0;
            string username = "";
            string display = "";
            try
            {
                using var loginDoc = JsonDocument.Parse(loginJson);
                if (loginDoc.RootElement.TryGetProperty("user", out var u))
                {
                    userId = u.GetProperty("id").GetInt64();
                    username = u.GetProperty("name").GetString() ?? "";
                    display = u.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? username : username;
                }
            }
            catch { }

            if (userId <= 0)
            {
                // fall back to cookie validation
                var validated = await ValidateCookieAsync(client, cookie);
                if (validated is null) return null;
                userId = validated.Id;
                username = validated.Username;
                display = validated.DisplayName;
            }

            // store account
            var existing = Accounts.FirstOrDefault(a => a.UserId == userId);
            if (existing is not null)
            {
                Credentials.Save(existing.UserId, cookie);
                ActiveAccountId = existing.UserId;
                Save();
                return existing;
            }

            var account = new RobloxAccount
            {
                UserId = userId,
                Username = username,
                DisplayName = display
            };
            Accounts.Add(account);
            Credentials.Save(account.UserId, cookie);
            if (ActiveAccountId is null) ActiveAccountId = account.UserId;
            Save();
            return account;
        }

        private static async Task<AuthenticatedRobloxUser?> ValidateCookieAsync(HttpClient client, string cookie)
        {
            try
            {
                var container = new CookieContainer();
                using var handler = new HttpClientHandler { CookieContainer = container };
                using var c = new HttpClient(handler);
                container.Add(new Uri("https://roblox.com"), new Cookie(".ROBLOSECURITY", cookie, "/", ".roblox.com"));
                var resp = await c.GetAsync("https://users.roblox.com/v1/users/authenticated");
                if (!resp.IsSuccessStatusCode) return null;
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return new AuthenticatedRobloxUser
                {
                    Id = doc.RootElement.GetProperty("id").GetInt64(),
                    Username = doc.RootElement.GetProperty("name").GetString() ?? "",
                    DisplayName = doc.RootElement.TryGetProperty("displayName", out var d) ? d.GetString() ?? "" : ""
                };
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string> GetCsrfTokenAsync(HttpClient client)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/logout");
                var resp = await client.SendAsync(req);
                if (resp.Headers.TryGetValues("x-csrf-token", out var vals))
                {
                    return vals.FirstOrDefault() ?? "";
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public bool HasCredential(long userId) => Credentials.Exists(userId);

        private static string NormalizeCookie(string cookie)
        {
            string raw = cookie ?? "";
            if (raw.Contains(".ROBLOSECURITY=", StringComparison.Ordinal))
                raw = raw.Split(".ROBLOSECURITY=")[1].Split(';')[0].Trim();
            return raw.Trim();
        }
    }

    internal sealed class AccountManagerData
    {
        [JsonPropertyName("accounts")]
        public List<RobloxAccount> Accounts { get; set; } = new();

        [JsonPropertyName("activeAccountId")]
        public long? ActiveAccountId { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}