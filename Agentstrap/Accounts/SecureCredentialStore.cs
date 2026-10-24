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

        public bool Remove(long userId)
        {
            string path = GetPath(userId);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        public bool Exists(long userId) => File.Exists(GetPath(userId));
    }
}