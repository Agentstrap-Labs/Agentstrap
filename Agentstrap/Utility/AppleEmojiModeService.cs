using System.Security.Cryptography;
using System.Text;

using Agentstrap.Enums;
using Agentstrap.Models.Persistable;

namespace Agentstrap.Utility
{
    /// <summary>
    /// Result of an Apple Emoji Mode operation. The status is the authoritative
    /// state that callers should surface to the user.
    /// </summary>
    public sealed class AppleEmojiModeResult
    {
        public AppleEmojiModeResult(AppleEmojiModeStatus status, string message, string? hash = null)
        {
            Status = status;
            Message = message;
            Hash = hash;
        }

        public AppleEmojiModeStatus Status { get; }

        public string Message { get; }

        public string? Hash { get; }
    }

    /// <summary>
    /// Safe, reversible manager for Roblox Apple Emoji Mode. It never touches
    /// Windows system fonts or Roblox binaries: it only copies a user supplied,
    /// compatible emoji font into Agentstrap's Modifications overlay, which is
    /// the same mechanism already used for other Roblox font mods.
    /// </summary>
    public sealed class AppleEmojiModeService
    {
        public const string EmojiFontRelativePath = @"content\fonts\TwemojiMozilla.ttf";

        private static readonly Dictionary<string, byte[]> FontHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            { "ttf", new byte[] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[] { 0x74, 0x74, 0x63, 0x66 } }
        };

        public AppleEmojiModeService(string modificationsRoot, string backupRoot, Func<bool> isRobloxInstalled)
        {
            ModificationsRoot = modificationsRoot ?? throw new ArgumentNullException(nameof(modificationsRoot));
            BackupRoot = backupRoot ?? throw new ArgumentNullException(nameof(backupRoot));
            IsRobloxInstalled = isRobloxInstalled ?? throw new ArgumentNullException(nameof(isRobloxInstalled));
        }

        public string ModificationsRoot { get; }

        public string BackupRoot { get; }

        public Func<bool> IsRobloxInstalled { get; }

        public string TargetPath => Path.Combine(ModificationsRoot, EmojiFontRelativePath);

        public string BackupPath => Path.Combine(BackupRoot, "TwemojiMozilla.original.ttf");

        public AppleEmojiModeResult Enable(string sourcePath)
        {
            if (!IsRobloxInstalled())
            {
                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.RobloxNotDetected,
                    "Roblox Player was not detected. No files were changed."
                );
            }

            var validation = ValidateFontFile(sourcePath);

            if (!validation.IsValid)
            {
                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.AssetsUnavailable,
                    validation.Error ?? "The selected emoji font is not valid."
                );
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TargetPath)!);
                Directory.CreateDirectory(BackupRoot);

                // Preserve the first pre-existing modification so repeated enables
                // never overwrite the user's original overlay file.
                if (File.Exists(TargetPath) && !File.Exists(BackupPath))
                {
                    File.Copy(TargetPath, BackupPath, overwrite: false);
                }

                Filesystem.AssertReadOnly(TargetPath);
                File.Copy(sourcePath, TargetPath, overwrite: true);

                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.EnabledAndApplied,
                    "Apple Emoji Mode was applied to the Roblox emoji font slot. Rendering inside Roblox is unverified.",
                    validation.Hash
                );
            }
            catch (Exception ex)
            {
                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.Failed,
                    $"Apple Emoji Mode could not be applied: {ex.Message}"
                );
            }
        }

        public AppleEmojiModeResult Disable(string? appliedHash)
        {
            try
            {
                if (File.Exists(BackupPath))
                {
                    string targetHash = File.Exists(TargetPath) ? HashFile(TargetPath) : "";

                    if (String.IsNullOrEmpty(appliedHash) ||
                        String.Equals(targetHash, appliedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(TargetPath)!);
                        Filesystem.AssertReadOnly(TargetPath);
                        File.Copy(BackupPath, TargetPath, overwrite: true);
                    }

                    File.Delete(BackupPath);

                    return new AppleEmojiModeResult(
                        AppleEmojiModeStatus.Disabled,
                        "Apple Emoji Mode was disabled and the previous Roblox emoji font was restored."
                    );
                }

                if (File.Exists(TargetPath))
                {
                    string targetHash = HashFile(TargetPath);

                    // Only remove a modification that this feature owns. An unrelated
                    // file left in the emoji slot is preserved.
                    if (String.IsNullOrEmpty(appliedHash) || String.Equals(targetHash, appliedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Filesystem.AssertReadOnly(TargetPath);
                        File.Delete(TargetPath);
                    }
                }

                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.Disabled,
                    "Apple Emoji Mode was disabled."
                );
            }
            catch (Exception ex)
            {
                return new AppleEmojiModeResult(
                    AppleEmojiModeStatus.Failed,
                    $"Apple Emoji Mode could not be disabled: {ex.Message}"
                );
            }
        }

        public AppleEmojiModeStatus GetStatus(bool enabled, string? sourcePath, string? appliedHash)
        {
            if (!enabled)
                return AppleEmojiModeStatus.Disabled;

            if (!IsRobloxInstalled())
                return AppleEmojiModeStatus.RobloxNotDetected;

            if (String.IsNullOrWhiteSpace(sourcePath))
                return AppleEmojiModeStatus.SetupRequired;

            var validation = ValidateFontFile(sourcePath);

            if (!validation.IsValid)
                return AppleEmojiModeStatus.AssetsUnavailable;

            if (!File.Exists(TargetPath))
                return AppleEmojiModeStatus.SetupRequired;

            string targetHash = HashFile(TargetPath);

            if (!String.IsNullOrEmpty(appliedHash) &&
                !String.Equals(targetHash, appliedHash, StringComparison.OrdinalIgnoreCase))
            {
                return AppleEmojiModeStatus.SetupRequired;
            }

            if (!String.Equals(targetHash, validation.Hash, StringComparison.OrdinalIgnoreCase))
                return AppleEmojiModeStatus.SetupRequired;

            // Agentstrap cannot observe Roblox's renderer, so a successful file
            // write is never upgraded to a verified rendering result.
            return AppleEmojiModeStatus.RenderingUnverified;
        }

        public (bool IsValid, string? Error, string Hash) ValidateFontFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return (false, "No font file was selected.", "");

            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                return (false, $"The selected path is invalid: {ex.Message}", "");
            }

            if (!File.Exists(fullPath))
                return (false, "The selected font file does not exist.", "");

            try
            {
                var info = new FileInfo(fullPath);

                if (info.Length == 0)
                    return (false, "The selected font file is empty.", "");

                // No size limit is enforced for user-supplied emoji fonts.

                string extension = Path.GetExtension(fullPath).TrimStart('.').ToLowerInvariant();

                if (!FontHeaders.ContainsKey(extension))
                    return (false, "Only .ttf, .otf and .ttc font files are supported.", "");

                byte[] header = new byte[4];

                using (var stream = File.OpenRead(fullPath))
                {
                    if (stream.Read(header, 0, header.Length) != header.Length ||
                        !header.SequenceEqual(FontHeaders[extension]))
                    {
                        return (false, "The selected file is not a valid font file.", "");
                    }
                }

                return (true, null, HashFile(fullPath));
            }
            catch (Exception ex)
            {
                return (false, $"The selected font could not be read: {ex.Message}", "");
            }
        }

        public string CreateDiagnosticReport(bool enabled, string? sourcePath, string? appliedHash, AppleEmojiModeStatus status)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Roblox Apple Emoji Mode diagnostics");
            builder.AppendLine($"Status: {status}");
            builder.AppendLine($"Enabled in settings: {enabled}");
            builder.AppendLine($"Roblox detected: {IsRobloxInstalled()}");
            builder.AppendLine($"Modifications folder: {ModificationsRoot}");
            builder.AppendLine($"Emoji font target: {TargetPath}");
            builder.AppendLine($"Target exists: {File.Exists(TargetPath)}");
            builder.AppendLine($"Backup exists: {File.Exists(BackupPath)}");

            if (!String.IsNullOrWhiteSpace(sourcePath))
            {
                var validation = ValidateFontFile(sourcePath);

                builder.AppendLine($"Selected font: {sourcePath}");
                builder.AppendLine($"Selected font valid: {validation.IsValid}");

                if (validation.IsValid)
                    builder.AppendLine($"Selected font hash: {validation.Hash}");
                else
                    builder.AppendLine($"Selected font error: {validation.Error}");
            }
            else
            {
                builder.AppendLine("Selected font: none");
            }

            if (!String.IsNullOrWhiteSpace(appliedHash))
                builder.AppendLine($"Applied hash: {appliedHash}");

            if (File.Exists(TargetPath))
                builder.AppendLine($"Target hash: {HashFile(TargetPath)}");

            builder.AppendLine();
            builder.AppendLine("Compatibility notes:");
            builder.AppendLine("- Roblox renders its emoji through content\\fonts\\TwemojiMozilla.ttf.");
            builder.AppendLine("- Agentstrap applies the replacement only through its Modifications overlay.");
            builder.AppendLine("- No Windows system font, Roblox executable or anti-cheat component is modified.");
            builder.AppendLine("- Apple's original emoji designs are proprietary; the user must supply a legally usable compatible font.");
            builder.AppendLine("- Agentstrap cannot observe Roblox rendering, so successful application remains unverified.");

            return builder.ToString();
        }

        private static string HashFile(string path)
        {
            using var stream = File.OpenRead(path);
            byte[] hash = App.MD5Provider.ComputeHash(stream);
            return MD5Hash.Stringify(hash);
        }
    }

    /// <summary>
    /// Convenience accessor for the application's default Apple Emoji Mode service.
    /// Tests should construct <see cref="AppleEmojiModeService"/> directly with
    /// temporary directories.
    /// </summary>
    public static class AppleEmojiMode
    {
        public static AppleEmojiModeService Default =>
            new(
                Paths.Modifications,
                Path.Combine(Paths.LocalAppData, App.ProjectName, "AppleEmojiMode"),
                () =>
                {
                    try
                    {
                        return File.Exists(new AppData.RobloxPlayerData().ExecutablePath);
                    }
                    catch
                    {
                        return false;
                    }
                }
            );
    }
}
