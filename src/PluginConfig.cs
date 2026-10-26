using BepInEx.Configuration;
using UnityEngine;

namespace KarmaWarnings
{
    internal enum MithrixPreviewMode
    {
        Off,
        Banner,
        Sidebar,
    }

    // All config entries, bound from the plugin's config file.
    internal sealed class PluginConfig
    {
        public readonly ConfigEntry<bool> Enabled;
        public readonly ConfigEntry<MithrixPreviewMode> MithrixPreview;

        public readonly ConfigEntry<bool> DebugLogging;
        public readonly ConfigEntry<bool> DebugAlwaysShow;
        public readonly ConfigEntry<KeyboardShortcut> DumpKey;

        public PluginConfig(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Show a warning above the crosshair before you do something that raises your Karma.");
            MithrixPreview = config.Bind("General", "MithrixPreview", MithrixPreviewMode.Off,
                "Also show what the Karma increase does to Mithrix. Banner: a short line per Karma level under the warning. Sidebar: a panel with more detail under the objectives.");

            DebugLogging = config.Bind("Debug", "DebugLogging", false,
                "Enable the dump key, which logs the Karma state and what the mod sees around you.");
            DebugAlwaysShow = config.Bind("Debug", "DebugAlwaysShow", false,
                "Show a warning even when nothing would raise your Karma, for testing.");
            DumpKey = config.Bind("Debug", "DumpKey", new KeyboardShortcut(KeyCode.F8),
                "With DebugLogging on, press to log the Karma state, what the warning banner shows, the exit you're aiming at, and what the fight checks for this stage see.");
        }
    }
}
