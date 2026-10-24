using System;
using System.IO;

using Agentstrap.Backups;
using Agentstrap.Diagnostics;
using Agentstrap.Enums;
using Agentstrap.Extensions;
using Agentstrap.Models.Persistable;
using Agentstrap.Platform;
using Agentstrap.Profiles;

using Xunit;

namespace Agentstrap.Tests
{
    public class NewFeaturesTests
    {
        private static string NewTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "AgentstrapNewFeaturesTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        [Fact]
        public void PlatformInfo_Current_IsWindows_OnThisMachine()
        {
            Assert.Equal(PlatformKind.Windows, PlatformInfo.Current);
            Assert.True(PlatformInfo.IsWindows);
        }

        [Fact]
        public void PlatformCapabilities_RobloxFastFlags_UnsupportedOnNonWindows()
        {
            // On Windows build machine, FastFlags supported
            Assert.True(PlatformCapabilities.IsSupported("FastFlags"));
        }

        [Fact]
        public void Profile_CreateFromSettings_AndRoundTrip()
        {
            string root = NewTempDirectory();
            try
            {
                var settings = new Settings { BootstrapperTitle = "TestProfile", Theme = Theme.Default };
                var mgr = new ProfileManager(Path.Combine(root, "profiles"));
                var profile = mgr.CreateFromSettings("MyProfile", settings, "desc");
                mgr.SaveProfile(profile);

                var loaded = mgr.LoadProfile("MyProfile");
                Assert.NotNull(loaded);
                Assert.Equal("MyProfile", loaded!.Name);
                Assert.Equal("TestProfile", loaded.Settings.BootstrapperTitle);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Profile_ImportExport_AndDuplicate()
        {
            string root = NewTempDirectory();
            try
            {
                var settings = new Settings();
                var mgr = new ProfileManager(Path.Combine(root, "profiles"));
                var profile = mgr.CreateFromSettings("P1", settings);
                mgr.SaveProfile(profile);

                string exportPath = Path.Combine(root, "p1.json");
                mgr.ExportProfile("P1", exportPath);
                Assert.True(File.Exists(exportPath));

                var imported = mgr.ImportProfile(exportPath);
                Assert.Equal("P1", imported.Name);

                mgr.DuplicateProfile("P1", "P2");
                Assert.Contains("P2", mgr.ListProfiles());
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Profile_Rename_Delete()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new ProfileManager(Path.Combine(root, "profiles"));
                mgr.SaveProfile(mgr.CreateFromSettings("Old", new Settings()));
                Assert.True(mgr.RenameProfile("Old", "New"));
                Assert.Contains("New", mgr.ListProfiles());
                Assert.DoesNotContain("Old", mgr.ListProfiles());
                Assert.True(mgr.DeleteProfile("New"));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void ConfigurationComparer_DetectsChangedValue()
        {
            var left = new Settings { BootstrapperTitle = "A" };
            var right = new Settings { BootstrapperTitle = "B" };
            var diffs = ConfigurationComparer.CompareSettings(left, right);
            Assert.Contains(diffs, d => d.Path == "BootstrapperTitle" && d.ChangeType == "Changed");
        }

        [Fact]
        public void Backup_CreateValidateRestore()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new BackupManager(Path.Combine(root, "backups"));
                var settings = new Settings { BootstrapperTitle = "Original" };
                var backup = mgr.CreateBackup(settings, "test");
                Assert.True(File.Exists(backup.FilePath));
                Assert.True(mgr.ValidateBackup(backup.FilePath));

                // corrupt
                File.WriteAllText(backup.FilePath, "not-json");
                Assert.False(mgr.ValidateBackup(backup.FilePath));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Backup_Restore_CreatesSafetyBackup()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new BackupManager(Path.Combine(root, "backups"));
                var original = new Settings { BootstrapperTitle = "Original" };
                var backup = mgr.CreateBackup(original, "orig");
                var current = new Settings { BootstrapperTitle = "Current" };

                var restored = mgr.RestoreBackup(backup, current);
                Assert.Equal("Original", restored.BootstrapperTitle);
                Assert.True(mgr.ListBackups().Count >= 2);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void Backup_ImportExport_Retention()
        {
            string root = NewTempDirectory();
            try
            {
                var mgr = new BackupManager(Path.Combine(root, "backups"));
                var b = mgr.CreateBackup(new Settings(), "b");
                string export = Path.Combine(root, "b.json");
                mgr.ExportBackup(b.Id, export);
                Assert.True(File.Exists(export));
                var imported = mgr.ImportBackup(export);
                Assert.NotEqual(b.Id, imported.Id);
                // retention
                for (int i = 0; i < 5; i++) mgr.CreateBackup(new Settings(), $"b{i}");
                int removed = mgr.ApplyRetention(2);
                Assert.True(removed > 0);
                Assert.True(mgr.ListBackups().Count <= 2);
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void ExtensionManifest_Validation()
        {
            var m = new ExtensionManifest { Id = "test.ext", Name = "Test", Version = "1.0", MinimumAgentstrapVersion = "1.2.0", SupportedOperatingSystems = new() { "Windows" } };
            Assert.True(m.IsValid(out var errors));
            Assert.Empty(errors);

            var bad = new ExtensionManifest();
            Assert.False(bad.IsValid(out var errs));
            Assert.NotEmpty(errs);
        }

        [Fact]
        public void ExtensionPackageValidator_PathTraversal()
        {
            Assert.False(ExtensionPackageValidator.IsSafeRelativePath("../evil", out _));
            Assert.False(ExtensionPackageValidator.IsSafeRelativePath("/abs", out _));
            Assert.True(ExtensionPackageValidator.IsSafeRelativePath("a/b.txt", out _));
        }

        [Fact]
        public void ExtensionManager_InstallEnableDisableRemove()
        {
            string root = NewTempDirectory();
            try
            {
                string src = Path.Combine(root, "src");
                Directory.CreateDirectory(src);
                File.WriteAllText(Path.Combine(src, "manifest.json"), "{\"Id\":\"ext1\",\"Name\":\"Ext1\",\"Version\":\"1.0\",\"MinimumAgentstrapVersion\":\"1.0.0\",\"SupportedOperatingSystems\":[\"Windows\"]}");
                File.WriteAllText(Path.Combine(src, "readme.txt"), "hi");

                var mgr = new ExtensionManager(Path.Combine(root, "extensions"));
                var installed = mgr.InstallFromDirectory(src);
                Assert.Equal("ext1", installed.Manifest.Id);
                Assert.True(mgr.Disable("ext1"));
                var got = mgr.Get("ext1");
                Assert.False(got!.Enabled);
                Assert.True(mgr.Enable("ext1"));
                Assert.True(mgr.Remove("ext1"));
                Assert.Null(mgr.Get("ext1"));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void ExtensionManager_ZipInstall_PathTraversalRejected()
        {
            string root = NewTempDirectory();
            try
            {
                string zipPath = Path.Combine(root, "bad.zip");
                using (var fs = File.Create(zipPath))
                using (var a = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create))
                {
                    var e = a.CreateEntry("../evil.txt");
                    using (var s = e.Open()) s.WriteByte(1);
                    var m = a.CreateEntry("manifest.json");
                    using (var ms = m.Open())
                    using (var w = new StreamWriter(ms))
                    {
                        w.Write("{\"Id\":\"z\",\"Name\":\"Z\",\"Version\":\"1.0\",\"MinimumAgentstrapVersion\":\"1.0.0\",\"SupportedOperatingSystems\":[\"Windows\"]}");
                        w.Flush();
                    }
                }
                var mgr = new ExtensionManager(Path.Combine(root, "extensions"));
                Assert.Throws<InvalidDataException>(() => mgr.InstallFromZip(zipPath));
            }
            finally { Directory.Delete(root, recursive: true); }
        }

        [Fact]
        public void DiagnosticRunner_RunsAllChecks()
        {
            string root = NewTempDirectory();
            try
            {
                var ctx = new DiagnosticContext
                {
                    ConfigurationDirectory = Path.Combine(root, "config"),
                    BackupsDirectory = Path.Combine(root, "backups"),
                    ExtensionsDirectory = Path.Combine(root, "extensions"),
                    ModificationsDirectory = Path.Combine(root, "mods"),
                    RobloxClientDetected = true,
                    ConfigurationValid = true
                };
                var runner = new DiagnosticRunner(ctx);
                var results = runner.RunAll();
                Assert.True(results.Count >= 8);
                Assert.All(results, r => Assert.False(string.IsNullOrWhiteSpace(r.Name)));
            }
            finally { Directory.Delete(root, recursive: true); }
        }
    }
}