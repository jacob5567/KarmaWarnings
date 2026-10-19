using BepInEx.Configuration;
using UnityEngine;

namespace KarmaWarnings
{
    // All config entries, bound from the plugin's config file.
    internal sealed class PluginConfig
    {
        public readonly ConfigEntry<bool> Enabled;

        public readonly ConfigEntry<bool> DebugLogging;
        public readonly ConfigEntry<bool> DebugAlwaysShow;
        public readonly ConfigEntry<KeyboardShortcut> DumpKey;

        public PluginConfig(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Show a warning above the crosshair before you do something that raises your Karma.");

            DebugLogging = config.Bind("Debug", "DebugLogging", false,
                "Log extra information about what the mod detects, and enable the dump key.");
            DebugAlwaysShow = config.Bind("Debug", "DebugAlwaysShow", false,
                "Show a warning even when nothing would raise your Karma, for testing.");
            DumpKey = config.Bind("Debug", "DumpKey", new KeyboardShortcut(KeyCode.F8),
                "With DebugLogging on, press to log the current Karma state.");
        }
    }
}
