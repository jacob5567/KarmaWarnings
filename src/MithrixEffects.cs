using System.Text;

namespace KarmaWarnings
{
    // What each Karma level adds to Mithrix, from the wiki's Karma page. Levels stack, and every level also adds +1%
    // damage. Shown by the MithrixPreview setting: Line on the banner, Title and Details on the sidebar.
    internal static class MithrixEffects
    {
        internal sealed class Level
        {
            // Finishes "Mithrix at Karma N: ".
            public string Line;
            public string Title;
            public string[] Details;
        }

        // Indexed by Karma level; index 0 is unused. Details stay short: the sidebar is narrow.
        public static readonly Level[] Levels =
        {
            null,
            new Level
            {
                Line = "shields himself after every 10% of health lost",
                Title = "Damage shell",
                Details = new[]
                {
                    "5 s shell after every 10% of health lost",
                    "Hits on the shell deal 1 damage",
                },
            },
            new Level
            {
                Line = "+30% speed; leap bombs, gravity pull, dash spikes",
                Title = "+30% speed",
                Details = new[]
                {
                    "Leap: shock wave bombs and a gravity well",
                    "Dash leaves Lunar Ruin spikes",
                    "Phase 2: the throne gives monsters items",
                },
            },
            new Level
            {
                Line = "hammer and needle attacks leave Lunar Ruin spikes",
                Title = "Lunar Ruin spikes",
                Details = new[]
                {
                    "Hammer attacks add spikes and waves",
                    "Needles are faster and leave spikes",
                    "Phase 2: dying monsters erupt in fire",
                },
            },
            new Level
            {
                Line = "landings call a meteor storm",
                Title = "Meteor storm",
                Details = new[]
                {
                    "Leap landings call meteors",
                    "Dash: 5 s knockback immunity",
                    "Phase 4: orbs spiral spike trails",
                },
            },
            new Level
            {
                Line = "summons Karmic Spires that slow and shoot you",
                Title = "Karmic Spires",
                Details = new[]
                {
                    "Up to 6 spires in phases 1 and 3",
                    "They slow nearby players and shoot",
                    "Phase 2: the throne blasts a player",
                },
            },
            new Level
            {
                Line = "halved cooldowns, elite monsters, new phase 4 attacks",
                Title = "Halved cooldowns",
                Details = new[]
                {
                    "Dash fires a homing bomb",
                    "Phase 3: monsters get an elite aspect",
                    "Phase 4 under 50%: mortars, golems, wisps",
                },
            },
            new Level
            {
                Line = "+1% damage, nothing new",
                Title = "Damage only",
                Details = new string[0],
            },
            new Level
            {
                Line = "+50% max health",
                Title = "+50% max health",
                Details = new[] { "New sky and spawn lines" },
            },
        };

        // A jump of more than this many levels lists the rest without their lines or details, so it stays short.
        private const int MaxDetailedLevels = 3;

        // BannerText by [fromKarma, toKarma], since the banner asks every frame.
        private static readonly string[,] bannerTexts = new string[Levels.Length, Levels.Length];

        // One line per level gained, for the banner; a longer jump ends in one line for the rest. Empty when no level is
        // gained.
        public static string BannerText(int fromKarma, int toKarma)
        {
            fromKarma = Clamp(fromKarma);
            toKarma = Clamp(toKarma);
            return bannerTexts[fromKarma, toKarma] ?? (bannerTexts[fromKarma, toKarma] = BuildBannerText(fromKarma, toKarma));
        }

        private static string BuildBannerText(int fromKarma, int toKarma)
        {
            StringBuilder text = new StringBuilder();
            int last = Clamp(toKarma);
            bool tooMany = last - fromKarma > MaxDetailedLevels;
            for (int level = fromKarma + 1; level <= last; level++)
            {
                if (text.Length > 0) text.Append('\n');
                if (tooMany && level - fromKarma == MaxDetailedLevels)
                {
                    text.Append($"Mithrix at Karma {level}–{last}: {last - level + 1} more levels of changes");
                    break;
                }
                text.Append($"Mithrix at Karma {level}: {Levels[level].Line}");
            }
            return text.ToString();
        }

        // A title and bullets per level gained, for the sidebar. Past MaxDetailedLevels, only the titles.
        public static string SidebarText(int fromKarma, int toKarma, string accentColor)
        {
            StringBuilder text = new StringBuilder();
            int last = Clamp(toKarma);
            for (int level = fromKarma + 1; level <= last; level++)
            {
                Level info = Levels[level];
                if (text.Length > 0) text.Append('\n');
                text.Append($"<color={accentColor}>{level}:</color> {info.Title}");
                if (level - fromKarma > MaxDetailedLevels) continue;
                foreach (string detail in info.Details)
                {
                    // The indent tag also indents the wrapped lines, so each bullet hangs.
                    text.Append($"\n<color={accentColor}>•</color><indent=1em>{detail}</indent>");
                }
            }
            return text.ToString();
        }

        private static int Clamp(int karma)
        {
            return karma < 0 ? 0 : karma < Levels.Length - 1 ? karma : Levels.Length - 1;
        }
    }
}
