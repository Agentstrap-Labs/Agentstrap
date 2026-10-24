using System;
using System.IO;
using System.Threading.Tasks;

using Agentstrap.Accounts;

using Xunit;

namespace Agentstrap.Tests
{
    public class AccountManagerTests
    {
        private static string NewTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "AgentstrapAccountTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        [Fact]
        public void SecureCredentialStore_RoundTripsSecret()
        {
            string root = NewTempDirectory();
            try
            {
                var store = new SecureCredentialStore(Path.Combine(root, "creds"));
                store.Save(123, "secret-token");
                Assert.True(store.Exists(123));
                Assert.Equal("secret-token", store.Load(123));
                Assert.True(store.Remove(123));
                Assert.False(store.Exists(123));
                Assert.Null(store.Load(123));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public async Task RobloxAccountManager_AddsAndPersistsAccounts()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new RobloxAccountManager(Path.Combine(root, "accounts"));
                var v = new Func<string, Task<AuthenticatedRobloxUser?>>(t => Task.FromResult<AuthenticatedRobloxUser?>(new AuthenticatedRobloxUser { Id = 1, Username = "alice", DisplayName = "Alice" }));
                var account = await mgr.AddAccountAsync("cookie", v);
                Assert.NotNull(account);
                Assert.Equal(1, account!.UserId);
                Assert.Equal("alice", account.Username);

                // duplicate updates credential
                var v2 = new Func<string, Task<AuthenticatedRobloxUser?>>(t => Task.FromResult<AuthenticatedRobloxUser?>(new AuthenticatedRobloxUser { Id = 1, Username = "alice", DisplayName = "Alice" }));
                await mgr.AddAccountAsync("new-cookie", v2);
                Assert.Single(mgr.Accounts);
                Assert.Equal("new-cookie", mgr.GetCredential(1));

                // second distinct account
                var v3 = new Func<string, Task<AuthenticatedRobloxUser?>>(t => Task.FromResult<AuthenticatedRobloxUser?>(new AuthenticatedRobloxUser { Id = 2, Username = "bob", DisplayName = "Bob" }));
                await mgr.AddAccountAsync("bob-cookie", v3);
                Assert.Equal(2, mgr.Accounts.Count);

                // reload
                var mgr2 = new RobloxAccountManager(Path.Combine(root, "accounts"));
                Assert.Equal(2, mgr2.Accounts.Count);
                Assert.Equal("new-cookie", mgr2.GetCredential(1));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public async Task RobloxAccountManager_InvalidCookieReturnsNull()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new RobloxAccountManager(Path.Combine(root, "accounts"));
                var v = new Func<string, Task<AuthenticatedRobloxUser?>>(t => Task.FromResult<AuthenticatedRobloxUser?>(null));
                var account = await mgr.AddAccountAsync("bad", v);
                Assert.Null(account);
                Assert.Empty(mgr.Accounts);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public async Task RobloxAccountManager_RemoveAndActiveAccount()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new RobloxAccountManager(Path.Combine(root, "accounts"));
                var v = new Func<string, Task<AuthenticatedRobloxUser?>>(t => Task.FromResult<AuthenticatedRobloxUser?>(new AuthenticatedRobloxUser { Id = 1, Username = "alice", DisplayName = "Alice" }));
                await mgr.AddAccountAsync("cookie", v);
                Assert.True(mgr.SetActiveAccount(1));
                Assert.Equal(1, mgr.ActiveAccountId);
                Assert.True(mgr.RemoveAccount(1));
                Assert.Empty(mgr.Accounts);
                Assert.Null(mgr.ActiveAccountId);
            }
            finally { Directory.Delete(root, recursive: true); }
        }
    }
}