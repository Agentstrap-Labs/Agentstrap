using Agentstrap.Enums.FlagPresets;

namespace Agentstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        private Dictionary<string, object> OriginalProp = new();

        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string FileName => "ClientAppSettings.json";

        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings", FileName);

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },

            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        /// <summary>
        /// Curated bundles of the preset flags above. Applying one sets (or, for
        /// null values, clears) every flag it owns so the rendering presets can be
        /// configured in one click.
        /// </summary>
        public static IReadOnlyList<FastFlagPreset> Presets { get; } = new List<FastFlagPreset>
        {
            new()
            {
                Name = "Default",
                Description = "Clears the rendering preset flags and returns them to Roblox's own defaults.",
                Flags = new Dictionary<string, string?>
                {
                    { "FFlagHandleAltEnterFullscreenManually", null },
                    { "DFFlagDisableDPIScale", null },
                    { "FIntDebugForceMSAASamples", null },
                    { "DFFlagTextureQualityOverrideEnabled", null },
                    { "DFIntTextureQualityOverride", null },
                }
            },
            new()
            {
                Name = "Performance",
                Description = "Favours frame rate: disables display scaling, forces 1x MSAA and the lowest texture quality.",
                Flags = new Dictionary<string, string?>
                {
                    { "DFFlagDisableDPIScale", "True" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntTextureQualityOverride", "0" },
                }
            },
            new()
            {
                Name = "Balanced",
                Description = "A middle ground: 2x MSAA and one step below the highest texture quality.",
                Flags = new Dictionary<string, string?>
                {
                    { "DFFlagDisableDPIScale", null },
                    { "FIntDebugForceMSAASamples", "2" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntTextureQualityOverride", "1" },
                }
            },
            new()
            {
                Name = "Quality",
                Description = "Favours visuals: 4x MSAA and the highest texture quality.",
                Flags = new Dictionary<string, string?>
                {
                    { "DFFlagDisableDPIScale", null },
                    { "FIntDebugForceMSAASamples", "4" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntTextureQualityOverride", "3" },
                }
            }
        };

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(PresetFlags[name]);
        }

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;

                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }

            return mapping.First().Key;
        }

        /// <summary>
        /// Applies every flag in a preset bundle.
        /// </summary>
        public void ApplyPreset(FastFlagPreset preset)
        {
            foreach (var pair in preset.Flags)
                SetValue(pair.Key, pair.Value);
        }

        /// <summary>
        /// Checks every current flag and returns the ones that do not match
        /// Roblox's expected name/value formats.
        /// </summary>
        public IReadOnlyList<FastFlagValidationEntry> Validate()
        {
            var invalid = new List<FastFlagValidationEntry>();

            foreach (var pair in Prop)
            {
                string value = pair.Value?.ToString() ?? "";
                var error = FastFlagValidator.Validate(pair.Key, value);

                if (error != FastFlagValidationError.None)
                    invalid.Add(new FastFlagValidationEntry(pair.Key, value, error));
            }

            return invalid;
        }

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override bool Load(bool alertFailure = true)
        {
            bool result = base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");

            return result;
        }
    }
}

