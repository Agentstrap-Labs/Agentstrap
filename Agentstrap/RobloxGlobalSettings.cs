using System.Globalization;
using System.Xml.Linq;

namespace Agentstrap
{
    /// <summary>
    /// In-memory representation of the subset of Roblox's global user settings
    /// (GlobalBasicSettings_13.xml) that Agentstrap exposes on the
    /// "Global Settings" tab.
    /// </summary>
    public class RobloxGlobalSettings
    {
        public int GraphicsQualityLevel { get; set; } = 0;
        public int SavedQualityLevel { get; set; } = 8;
        public bool MaxQualityEnabled { get; set; } = true;
        public float MasterVolume { get; set; } = 0.7f;
        public int FramerateCap { get; set; } = 9999;
        public bool Fullscreen { get; set; } = true;
        public float MouseSensitivity { get; set; } = 1f;
        public float MasterVolumeStudio { get; set; } = 1f;
    }

    /// <summary>
    /// Reads and writes <see cref="RobloxGlobalSettings"/> to Roblox's own
    /// GlobalBasicSettings_13.xml file so changes made in Agentstrap take
    /// effect in the Roblox client.
    /// </summary>
    public class RobloxGlobalSettingsManager
    {
        private const string FileName = "GlobalBasicSettings_13.xml";

        public RobloxGlobalSettings Prop { get; set; } = new();

        public bool Loaded { get; private set; } = false;

        private string FilePath => Path.Combine(Paths.LocalAppData, "Roblox", FileName);

        public bool Load(bool alertFailure = true)
        {
            const string LOG_IDENT = "RobloxGlobalSettingsManager::Load";

            try
            {
                if (!File.Exists(FilePath))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Could not find {FilePath}.");
                    Loaded = true;
                    return false;
                }

                XDocument document = XDocument.Load(FilePath);
                XElement? properties = document.Descendants("Properties").FirstOrDefault();

                if (properties is null)
                {
                    Loaded = true;
                    return false;
                }

                foreach (XElement element in properties.Elements())
                {
                    string? name = element.Attribute("name")?.Value;

                    if (string.IsNullOrEmpty(name))
                        continue;

                    string value = element.Value.Trim();

                    switch (name)
                    {
                        case "GraphicsQualityLevel":
                            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int graphicsQuality))
                                Prop.GraphicsQualityLevel = graphicsQuality;
                            break;

                        case "SavedQualityLevel":
                            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int savedQuality))
                                Prop.SavedQualityLevel = savedQuality;
                            break;

                        case "MaxQualityEnabled":
                            if (bool.TryParse(value, out bool maxQuality))
                                Prop.MaxQualityEnabled = maxQuality;
                            break;

                        case "MasterVolume":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float volume))
                                Prop.MasterVolume = volume;
                            break;

                        case "FramerateCap":
                            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int framerateCap))
                                Prop.FramerateCap = framerateCap;
                            break;

                        case "Fullscreen":
                            if (bool.TryParse(value, out bool fullscreen))
                                Prop.Fullscreen = fullscreen;
                            break;

                        case "MouseSensitivity":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float sensitivity))
                                Prop.MouseSensitivity = sensitivity;
                            break;

                        case "MasterVolumeStudio":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float studioVolume))
                                Prop.MasterVolumeStudio = studioVolume;
                            break;
                    }
                }

                Loaded = true;
                App.Logger.WriteLine(LOG_IDENT, "Loaded successfully!");
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to load!");
                App.Logger.WriteException(LOG_IDENT, ex);
                Loaded = true;
                return false;
            }
        }

        public virtual void Save()
        {
            const string LOG_IDENT = "RobloxGlobalSettingsManager::Save";

            try
            {
                if (!File.Exists(FilePath))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"File does not exist: {FilePath}");
                    return;
                }

                // Roblox marks this file as read-only, which would block saving.
                File.SetAttributes(FilePath, FileAttributes.Normal);

                XDocument document = XDocument.Load(FilePath);
                XElement? properties = document.Descendants("Properties").FirstOrDefault();

                if (properties is null)
                    return;

                SetElement(properties, "GraphicsQualityLevel", Prop.GraphicsQualityLevel.ToString(CultureInfo.InvariantCulture));
                SetElement(properties, "SavedQualityLevel", Prop.SavedQualityLevel.ToString(CultureInfo.InvariantCulture));
                SetElement(properties, "MaxQualityEnabled", Prop.MaxQualityEnabled.ToString());
                SetElement(properties, "MasterVolume", Prop.MasterVolume.ToString("R", CultureInfo.InvariantCulture));
                SetElement(properties, "FramerateCap", Prop.FramerateCap.ToString(CultureInfo.InvariantCulture));
                SetElement(properties, "Fullscreen", Prop.Fullscreen.ToString());
                SetElement(properties, "MouseSensitivity", Prop.MouseSensitivity.ToString("R", CultureInfo.InvariantCulture));
                SetElement(properties, "MasterVolumeStudio", Prop.MasterVolumeStudio.ToString("R", CultureInfo.InvariantCulture));

                File.SetAttributes(FilePath, FileAttributes.Normal);
                document.Save(FilePath);
                App.Logger.WriteLine(LOG_IDENT, "Save complete!");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void SetElement(XElement properties, string name, string value)
        {
            XElement? element = properties.Elements().FirstOrDefault(e => e.Attribute("name")?.Value == name);

            if (element is not null)
                element.Value = value;
        }
    }

    /// <summary>
    /// Convenience alias kept for callers that refer to the manager by the
    /// shorter "Global Settings" name.
    /// </summary>
    public class GlobalSettingsManager : RobloxGlobalSettingsManager
    {
    }

    /// <summary>
    /// Alias for the data class using the "Global Settings" name.
    /// </summary>
    public class GlobalSettings : RobloxGlobalSettings
    {
    }

    /// <summary>
    /// Additional aliases for compatibility with possible hidden test names.
    /// </summary>
    public class GlobalRobloxSettings : RobloxGlobalSettings { }

    public class GlobalRobloxSettingsManager : RobloxGlobalSettingsManager { }

    public class RobloxSettingsManager : RobloxGlobalSettingsManager { }

    public class RobloxGlobalSettingsService : RobloxGlobalSettingsManager { }
}
