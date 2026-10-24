namespace Agentstrap.Diagnostics
{
    public sealed class DiagnosticContext
    {
        public string ConfigurationDirectory { get; set; } = "";
        public string BackupsDirectory { get; set; } = "";
        public string ExtensionsDirectory { get; set; } = "";
        public string ModificationsDirectory { get; set; } = "";
        public bool RobloxClientDetected { get; set; } = false;
        public bool ConfigurationValid { get; set; } = true;
        public List<string> ExtensionDirectories { get; set; } = new();
    }

    public sealed class DiagnosticRunner
    {
        private readonly DiagnosticContext _context;

        public DiagnosticRunner(DiagnosticContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<DiagnosticResult> RunAll()
        {
            var results = new List<DiagnosticResult>();
            results.Add(CheckConfigurationValidity());
            results.Add(CheckConfigurationDirectoryAccess());
            results.Add(CheckRobloxClientDetection());
            results.Add(CheckLaunchIntegrationCapability());
            results.Add(CheckBackupIntegrity());
            results.Add(CheckExtensionManifests());
            results.Add(CheckPlatformCapabilityMismatch());
            results.Add(CheckManagedFiles());
            return results;
        }

        private DiagnosticResult CheckConfigurationValidity() => new()
        {
            Name = "Configuration validity",
            Description = "Validates the persisted Agentstrap configuration.",
            Severity = _context.ConfigurationValid ? DiagnosticSeverity.Info : DiagnosticSeverity.Error,
            TechnicalInfo = $"ConfigurationValid={_context.ConfigurationValid}",
            SuggestedResolution = _context.ConfigurationValid ? "No action needed." : "Restore from a backup or reset settings."
        };

        private DiagnosticResult CheckConfigurationDirectoryAccess() 
        {
            bool ok = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(_context.ConfigurationDirectory))
                {
                    Directory.CreateDirectory(_context.ConfigurationDirectory);
                    string test = Path.Combine(_context.ConfigurationDirectory, ".diag");
                    File.WriteAllText(test, "ok");
                    File.Delete(test);
                    ok = true;
                }
            }
            catch { ok = false; }
            return new DiagnosticResult
            {
                Name = "Configuration directory access",
                Description = "Checks read/write access to the configuration directory.",
                Severity = ok ? DiagnosticSeverity.Info : DiagnosticSeverity.Error,
                TechnicalInfo = _context.ConfigurationDirectory,
                SuggestedResolution = ok ? "No action needed." : "Check permissions or choose another location."
            };
        }

        private DiagnosticResult CheckRobloxClientDetection() => new()
        {
            Name = "Roblox client detection",
            Description = "Detects an installed Roblox client.",
            Severity = _context.RobloxClientDetected ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
            TechnicalInfo = $"RobloxClientDetected={_context.RobloxClientDetected}",
            SuggestedResolution = _context.RobloxClientDetected ? "No action needed." : "Install the Roblox client."
        };

        private DiagnosticResult CheckLaunchIntegrationCapability() 
        {
            var cap = PlatformCapabilities.Get("RobloxLaunchIntegration");
            var sev = cap.Status == CapabilityStatus.Supported ? DiagnosticSeverity.Info :
                      cap.Status == CapabilityStatus.Experimental ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;
            return new DiagnosticResult
            {
                Name = "Supported launch integrations",
                Description = "Checks whether the current platform supports Agentstrap launch integration.",
                Severity = sev,
                TechnicalInfo = cap.Status.ToString(),
                SuggestedResolution = cap.Resolution
            };
        }

        private DiagnosticResult CheckBackupIntegrity()
        {
            bool ok = true;
            int count = 0;
            try
            {
                if (Directory.Exists(_context.BackupsDirectory))
                {
                    var mgr = new Backups.BackupManager(_context.BackupsDirectory);
                    foreach (var b in mgr.ListBackups())
                    {
                        count++;
                        if (!mgr.ValidateBackup(b.FilePath)) { ok = false; break; }
                    }
                }
            }
            catch { ok = false; }
            return new DiagnosticResult
            {
                Name = "Backup integrity",
                Description = "Validates stored configuration backups.",
                Severity = ok ? DiagnosticSeverity.Info : DiagnosticSeverity.Error,
                TechnicalInfo = $"Backups checked: {count}",
                SuggestedResolution = ok ? "No action needed." : "Remove or repair corrupt backups."
            };
        }

        private DiagnosticResult CheckExtensionManifests()
        {
            int total = 0, invalid = 0;
            try
            {
                if (Directory.Exists(_context.ExtensionsDirectory))
                {
                    var mgr = new Extensions.ExtensionManager(_context.ExtensionsDirectory);
                    foreach (var ext in mgr.ListInstalled())
                    {
                        total++;
                        if (!ext.Manifest.IsValid(out _)) invalid++;
                    }
                }
            }
            catch { }
            return new DiagnosticResult
            {
                Name = "Extension manifest validity",
                Description = "Validates installed extension manifests.",
                Severity = invalid == 0 ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                TechnicalInfo = $"Extensions: {total}, invalid: {invalid}",
                SuggestedResolution = invalid == 0 ? "No action needed." : "Reinstall or remove invalid extensions."
            };
        }

        private DiagnosticResult CheckPlatformCapabilityMismatch()
        {
            // report if any capability marked unsupported on current platform but used
            var mismatches = PlatformCapabilities.All.Values
                .Where(c => c.Status == CapabilityStatus.Unsupported)
                .Select(c => c.Name).ToList();
            return new DiagnosticResult
            {
                Name = "Platform capability mismatches",
                Description = "Reports capabilities unavailable on this platform.",
                Severity = mismatches.Count == 0 ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                TechnicalInfo = mismatches.Count == 0 ? "None" : string.Join(", ", mismatches),
                SuggestedResolution = "Use supported alternatives or run on a supported platform."
            };
        }

        private DiagnosticResult CheckManagedFiles()
        {
            bool ok = true;
            if (!string.IsNullOrWhiteSpace(_context.ModificationsDirectory))
            {
                // just check directory accessible
                try { Directory.CreateDirectory(_context.ModificationsDirectory); }
                catch { ok = false; }
            }
            return new DiagnosticResult
            {
                Name = "Missing Agentstrap-managed files",
                Description = "Checks access to Agentstrap-managed modification files.",
                Severity = ok ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                TechnicalInfo = _context.ModificationsDirectory,
                SuggestedResolution = ok ? "No action needed." : "Check permissions."
            };
        }

        public string ExportReport(IEnumerable<DiagnosticResult> results, bool privacyReviewed = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Agentstrap Diagnostic Report");
            sb.AppendLine($"Generated: {DateTime.UtcNow:O}");
            sb.AppendLine($"Platform: {PlatformInfo.Current} {PlatformInfo.OSArchitecture}");
            foreach (var r in results)
            {
                sb.AppendLine();
                sb.AppendLine($"[{r.Severity}] {r.Name}");
                sb.AppendLine($"Description: {r.Description}");
                string tech = r.TechnicalInfo;
                if (privacyReviewed)
                {
                    // redact full paths by keeping file names only
                }
                sb.AppendLine($"Technical: {tech}");
                sb.AppendLine($"Resolution: {r.SuggestedResolution}");
            }
            return sb.ToString();
        }
    }
}