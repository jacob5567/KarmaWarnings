using System.IO;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace KarmaWarnings
{
    // Every RiskOfOptions type is touched only inside NoInlining methods, so the mod loads fine without it.
    internal static class RiskOfOptionsCompat
    {
        public const string GUID = "com.rune580.riskofoptions";
        private const string IconResource = "KarmaWarnings.icon.png";

        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(GUID);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Register(PluginConfig settings)
        {
            ModSettingsManager.SetModDescription(
                "Warns you before you do something that raises your Karma.");
            Sprite icon = LoadIcon();
            if (icon)
            {
                ModSettingsManager.SetModIcon(icon);
            }

            ModSettingsManager.AddOption(new CheckBoxOption(settings.Enabled));
            ModSettingsManager.AddOption(new ChoiceOption(settings.MithrixPreview));

            // The options menu groups these under a "Debug" header, named after their config section.
            ModSettingsManager.AddOption(new CheckBoxOption(settings.DebugLogging));
            ModSettingsManager.AddOption(new CheckBoxOption(settings.DebugAlwaysShow));
            ModSettingsManager.AddOption(new KeyBindOption(settings.DumpKey));
        }

        // The Thunderstore icon, embedded in the DLL by the csproj.
        private static Sprite LoadIcon()
        {
            using (Stream stream = typeof(RiskOfOptionsCompat).Assembly.GetManifestResourceStream(IconResource))
            {
                if (stream == null) return null;
                using (MemoryStream bytes = new MemoryStream())
                {
                    stream.CopyTo(bytes);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(bytes.ToArray())) return null;
                    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}
