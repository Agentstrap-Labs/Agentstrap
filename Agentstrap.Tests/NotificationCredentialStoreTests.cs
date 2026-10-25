using System;
using System.IO;
using System.Linq;
using System.Text;

using Agentstrap.Integrations;

using Xunit;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Covers secure notification credential persistence: DPAPI protected storage is
    /// round trippable, clearable, and - the important part - the store on disk never
    /// contains the secrets in plaintext.
    /// </summary>
    public class NotificationCredentialStoreTests
    {
        private static string NewTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "AgentstrapNotificationCredentialTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        [Fact]
        public void CredentialsRoundTripThroughDpapi()
        {
            string root = NewTempDirectory();
            try
            {
                var store = new ServerConnectionNotificationCredentialStore(root);

                store.IpInfoToken = "ipinfo-token-value";
                store.BarkPushKey = "bark-push-key-value";

                var loaded = store.Load();

                Assert.True(loaded.HasIpInfoToken);
                Assert.True(loaded.HasBarkCredentials);
                Assert.Equal("ipinfo-token-value", loaded.IpInfoToken);
                Assert.Equal("bark-push-key-value", loaded.BarkPushKey);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void SettingNullOrBlankRemovesTheSecret()
        {
            string root = NewTempDirectory();
            try
            {
                var store = new ServerConnectionNotificationCredentialStore(root);
                store.BarkPushKey = "value";

                store.BarkPushKey = null;
                Assert.Null(store.BarkPushKey);

                store.BarkPushKey = "value";
                store.BarkPushKey = "   ";
                Assert.Null(store.BarkPushKey);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void StoreFiles_AreNotReadablePlaintext()
        {
            string root = NewTempDirectory();
            try
            {
                string ipinfoToken = "ipinfo-token-must-not-leak";
                string pushKey = "bark-push-key-must-not-leak";

                var store = new ServerConnectionNotificationCredentialStore(root);
                store.IpInfoToken = ipinfoToken;
                store.BarkPushKey = pushKey;

                // every stored file must be encrypted: no plaintext copy anywhere in the store
                foreach (string file in Directory.EnumerateFiles(root, "*.bin"))
                {
                    string raw = File.ReadAllText(file, Encoding.Latin1);

                    Assert.DoesNotContain(ipinfoToken, raw);
                    Assert.DoesNotContain(pushKey, raw);
                }
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Save_PersistsAcrossStoreInstances()
        {
            string root = NewTempDirectory();
            try
            {
                var first = new ServerConnectionNotificationCredentialStore(root);
                first.IpInfoToken = "token-across-instances";
                first.BarkPushKey = "push-key";

                var second = new ServerConnectionNotificationCredentialStore(root);

                Assert.Equal("token-across-instances", second.IpInfoToken);
                Assert.Equal("push-key", second.BarkPushKey);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Clear_RemovesEverySecret()
        {
            string root = NewTempDirectory();
            try
            {
                var store = new ServerConnectionNotificationCredentialStore(root);
                store.IpInfoToken = "ipinfo-token-value";
                store.BarkPushKey = "push-key";

                store.Clear();

                Assert.Null(store.IpInfoToken);
                Assert.Null(store.BarkPushKey);

                var credentials = store.Load();
                Assert.False(credentials.HasIpInfoToken);
                Assert.False(credentials.HasBarkCredentials);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void DangerousKeys_NeverEscapeTheStoreDirectory()
        {
            string root = NewTempDirectory();
            try
            {
                var store = new ServerConnectionNotificationCredentialStore(root);

                store.Credentials.Save("bad:../escape!", "value");

                // the secret must have been stored under a hashed filename inside the store root
                var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToList();

                Assert.Single(files);
                Assert.DoesNotMatch(@"[\:*?""<>|]", Path.GetFileName(files[0]));
                Assert.Equal("value", store.Credentials.Load("bad:../escape!"));
            }
            finally { Directory.Delete(root, recursive: true); }
        }
    }
}
