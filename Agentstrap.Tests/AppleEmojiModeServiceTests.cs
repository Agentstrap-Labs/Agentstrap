using System;
using System.IO;

using Agentstrap.Enums;
using Agentstrap.Utility;

using Xunit;

namespace Agentstrap.Tests
{
    public class AppleEmojiModeServiceTests
    {
        private static string NewTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "AgentstrapAppleEmojiTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static string CreateFontFile(string directory, string name, string content)
        {
            string path = Path.Combine(directory, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // A valid TrueType header followed by arbitrary payload is enough for
            // the service's lightweight validation.
            byte[] header = { 0x00, 0x01, 0x00, 0x00 };
            File.WriteAllBytes(path, header);
            File.AppendAllText(path, content);

            return path;
        }

        [Fact]
        public void Enable_AppliesFontAndPreservesOriginal()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string originalPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
                File.WriteAllText(originalPath, "ORIGINAL");

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");

                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                var result = service.Enable(sourcePath);

                Assert.Equal(AppleEmojiModeStatus.EnabledAndApplied, result.Status);
                Assert.True(File.Exists(originalPath));
                Assert.Equal(File.ReadAllText(sourcePath), File.ReadAllText(originalPath));
                Assert.True(File.Exists(service.BackupPath));
                Assert.Equal("ORIGINAL", File.ReadAllText(service.BackupPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Disable_RestoresOriginalFile()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.WriteAllText(targetPath, "ORIGINAL");

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);

                var enableResult = service.Enable(sourcePath);
                var disableResult = service.Disable(enableResult.Hash);

                Assert.Equal(AppleEmojiModeStatus.Disabled, disableResult.Status);
                Assert.True(File.Exists(targetPath));
                Assert.Equal("ORIGINAL", File.ReadAllText(targetPath));
                Assert.False(File.Exists(service.BackupPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void RepeatedEnable_DoesNotOverwriteBackup()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.WriteAllText(targetPath, "ORIGINAL");

                string firstSource = CreateFontFile(root, "first.ttf", "FIRST");
                string secondSource = CreateFontFile(root, "second.ttf", "SECOND");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);

                service.Enable(firstSource);
                service.Enable(secondSource);

                Assert.Equal("ORIGINAL", File.ReadAllText(service.BackupPath));
                Assert.Equal(File.ReadAllText(secondSource), File.ReadAllText(targetPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Disable_WhenTargetChangedAfterActivation_PreservesChangedFile()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.WriteAllText(targetPath, "ORIGINAL");

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                var enableResult = service.Enable(sourcePath);

                File.WriteAllText(targetPath, "USER-CHANGED");
                service.Disable(enableResult.Hash);

                Assert.Equal("USER-CHANGED", File.ReadAllText(targetPath));
                Assert.False(File.Exists(service.BackupPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Disable_WhenNoBackupAndHashMatches_RemovesTarget()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                File.Copy(sourcePath, targetPath, overwrite: true);

                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                var hash = service.ValidateFontFile(sourcePath).Hash;

                var result = service.Disable(hash);

                Assert.Equal(AppleEmojiModeStatus.Disabled, result.Status);
                Assert.False(File.Exists(targetPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Disable_WhenNoBackupAndHashDiffers_PreservesTarget()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.WriteAllText(targetPath, "UNRELATED");

                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                service.Disable("different-hash");

                Assert.True(File.Exists(targetPath));
                Assert.Equal("UNRELATED", File.ReadAllText(targetPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Enable_WhenRobloxNotDetected_DoesNotChangeFiles()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.WriteAllText(targetPath, "ORIGINAL");

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => false);

                var result = service.Enable(sourcePath);

                Assert.Equal(AppleEmojiModeStatus.RobloxNotDetected, result.Status);
                Assert.Equal("ORIGINAL", File.ReadAllText(targetPath));
                Assert.False(File.Exists(service.BackupPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Enable_WhenSourceMissing_ReturnsAssetsUnavailable()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);

                var result = service.Enable(Path.Combine(root, "missing.ttf"));

                Assert.Equal(AppleEmojiModeStatus.AssetsUnavailable, result.Status);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void Enable_WhenSourceInvalid_ReturnsAssetsUnavailable()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string sourcePath = Path.Combine(root, "bad.ttf");
                File.WriteAllText(sourcePath, "not a font");

                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                var result = service.Enable(sourcePath);

                Assert.Equal(AppleEmojiModeStatus.AssetsUnavailable, result.Status);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void GetStatus_ReturnsUnverifiedWhenApplied()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string targetPath = Path.Combine(modifications, AppleEmojiModeService.EmojiFontRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);
                var enableResult = service.Enable(sourcePath);

                var status = service.GetStatus(true, sourcePath, enableResult.Hash);

                Assert.Equal(AppleEmojiModeStatus.RenderingUnverified, status);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void GetStatus_ReturnsRobloxNotDetected()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => false);

                var status = service.GetStatus(true, sourcePath, "hash");

                Assert.Equal(AppleEmojiModeStatus.RobloxNotDetected, status);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void ValidateFontFile_RejectsEmptyFile()
        {
            string root = NewTempDirectory();

            try
            {
                string path = Path.Combine(root, "empty.ttf");
                File.WriteAllText(path, "");

                var service = new AppleEmojiModeService(root, Path.Combine(root, "backup"), () => true);
                var result = service.ValidateFontFile(path);

                Assert.False(result.IsValid);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void CreateDiagnosticReport_ContainsKeyInformation()
        {
            string root = NewTempDirectory();

            try
            {
                string modifications = Path.Combine(root, "Modifications");
                string backupRoot = Path.Combine(root, "Backup");
                string sourcePath = CreateFontFile(root, "source.ttf", "APPLE");
                var service = new AppleEmojiModeService(modifications, backupRoot, () => true);

                string report = service.CreateDiagnosticReport(true, sourcePath, null, AppleEmojiModeStatus.RenderingUnverified);

                Assert.Contains("Roblox Apple Emoji Mode diagnostics", report);
                Assert.Contains(sourcePath, report);
                Assert.Contains("Roblox detected: True", report);
                Assert.Contains("unverified", report, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}