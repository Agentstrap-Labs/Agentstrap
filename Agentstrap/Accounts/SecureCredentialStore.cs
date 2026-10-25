using System.Security.Cryptography;

namespace Agentstrap.Accounts
{
    public sealed class SecureCredentialStore
    {
        public string StoreDirectory { get; }

        public SecureCredentialStore(string storeDirectory)
        {
            StoreDirectory = storeDirectory;
            Directory.CreateDirectory(storeDirectory);
        }

        public string GetPath(long userId) => Path.Combine(StoreDirectory, userId + ".bin");

        /// <summary>
        /// Named-secret variant (numeric ids are supported alongside these, so the
        /// existing account credential files stay untouched).
        /// </summary>
        public string GetPath(string key) => Path.Combine(StoreDirectory, NormalizeKey(key) + ".bin");

        public void Save(long userId, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentException("Secret is required.", nameof(secret));
            byte[] plain = Encoding.UTF8.GetBytes(secret);
            byte[] protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(GetPath(userId), protectedBytes);
        }

        public string? Load(long userId)
        {
            string path = GetPath(userId);
            return Read(path);
        }

        public bool Remove(long userId)
        {
            string path = GetPath(userId);
            return RemoveFile(path);
        }

        public bool Exists(long userId) => File.Exists(GetPath(userId));

        public void Save(string key, string secret) => SaveFile(GetPath(key), secret);

        public string? Load(string key) => Read(GetPath(key));

        public bool Remove(string key) => RemoveFile(GetPath(key));

        public bool Exists(string key) => File.Exists(GetPath(key));

        /// <summary>
        /// Removes every secret stored in this directory (named and numeric alike).
        /// </summary>
        public void Clear()
        {
            foreach (string file in Directory.EnumerateFiles(StoreDirectory, "*.bin"))
                File.Delete(file);
        }

        private static void SaveFile(string path, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentException("Secret is required.", nameof(secret));
            byte[] plain = Encoding.UTF8.GetBytes(secret);
            byte[] protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, protectedBytes);
        }

        private static string? Read(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                byte[] protectedBytes = File.ReadAllBytes(path);
                byte[] plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return null;
            }
        }

        private static bool RemoveFile(string path)
        {
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        /// <summary>
        /// Secrets are identified by a filesystem-safe key. Anything that is not a
        /// simple safe name is swapped for a SHA-256 digest, so arbitrary input can
        /// never escape the store directory.
        /// </summary>
        private static string NormalizeKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));

            string trimmed = key.Trim();

            if (Regex.IsMatch(trimmed, @"^[A-Za-z0-9._-]+$"))
                return trimmed;

            byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(trimmed));

            return "key-" + Convert.ToHexString(digest).ToLowerInvariant();
        }
    }
}
