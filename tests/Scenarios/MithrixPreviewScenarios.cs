using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using ModTestRunner;
using RoR2.UI;
using UnityEngine;

namespace KarmaWarnings.Tests
{
    // The MithrixPreview setting: the line per Karma level under the warning, and the sidebar under the objectives. The
    // Null Portal adds 2 Karma (more with the Karma Modifier), so it shows more than one level. The text scenarios call
    // MithrixEffects directly and need no stage.
    internal static class MithrixPreviewScenarios
    {
        private const string Suite = "mithrix";
        private static readonly string[] Triggers = { KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena };
        // The Karma the portal scenarios start from: low enough for the portal's levels to fit under the maximum.
        private const int StartKarma = 3;
        // A jump of more than this many levels is summarized on the banner, and only these get sidebar details.
        private const int DetailedLevels = 3;
        private const string Accent = "#FFFFFF";
        private const string LookingGlassGUID = "droppod.lookingglass";
        // LookingGlass's stats panel, which it adds to the objective column.
        private const string LookingGlassPanelName = "PlayerStats";

        public static IEnumerable<Scenario> All()
        {
            yield return Text("banner_text_long_jump", BannerTextLongJump);
            yield return Text("sidebar_text_long_jump", SidebarTextLongJump);
            yield return Text("texts_avoid_cripple", TextsAvoidCripple);
            yield return Portal("off_shows_no_preview", OffShowsNoPreview);
            yield return Portal("banner_line_per_level", BannerLinePerLevel);
            yield return Portal("banner_stops_at_max", BannerStopsAtMax);
            yield return Portal("sidebar_shows_levels", SidebarShowsLevels);
            yield return Portal("sidebar_under_objectives", SidebarUnderObjectives);
            yield return Portal("sidebar_below_lookingglass", SidebarBelowLookingGlass);
            yield return Portal("sidebar_hides_with_warning", SidebarHidesWithWarning);
            yield return new KarmaScenario(2, Suite, "advice_has_no_preview", Stages.TitanicPlains,
                new[] { KarmaTriggers.CompleteHalcyonShrine }, AdviceHasNoPreview);
        }

        private static Scenario Text(string name, Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(1, Suite, name, null, null, run);
        }

        private static Scenario Portal(string name, Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, Triggers, run);
        }

        // ---- The texts ----

        private static IEnumerator BannerTextLongJump(Harness h)
        {
            Equal(h, MithrixEffects.BannerText(0, 8),
                $"Mithrix at Karma 1: {Texts.MithrixLines[1]}\nMithrix at Karma 2: {Texts.MithrixLines[2]}\nMithrix at Karma 3–8: 6 more levels of changes",
                "the banner from 0 to 8 Karma");
            Equal(h, MithrixEffects.BannerText(5, 8), ExpectedBanner(5, 8), "the banner for exactly 3 levels");
            Equal(h, MithrixEffects.BannerText(7, 12), $"Mithrix at Karma 8: {Texts.MithrixLines[8]}", "the banner past the maximum");
            Equal(h, MithrixEffects.BannerText(8, 8), "", "the banner with no level gained");
            yield break;
        }

        // Every level gets its title, in order; only the first DetailedLevels get bullets under it.
        private static IEnumerator SidebarTextLongJump(Harness h)
        {
            string text = MithrixEffects.SidebarText(0, 8, Accent);
            string[] lines = text.Split('\n');
            int level = 0;
            Dictionary<int, int> bullets = new Dictionary<int, int>();
            foreach (string line in lines)
            {
                if (line.StartsWith($"<color={Accent}>•</color>"))
                {
                    if (level == 0) throw h.Fail($"the sidebar starts with a bullet: \"{text}\"");
                    bullets[level] = bullets.TryGetValue(level, out int count) ? count + 1 : 1;
                    continue;
                }
                level++;
                Equal(h, line, $"<color={Accent}>{level}:</color> {Texts.MithrixTitles[level]}", $"the sidebar's title line for Karma {level}");
            }
            if (level != 8) throw h.Fail($"the sidebar from 0 to 8 Karma has {level} title lines: \"{text}\"");
            for (int i = 1; i <= 8; i++)
            {
                bool hasBullets = bullets.ContainsKey(i);
                bool wantsBullets = i <= DetailedLevels && MithrixEffects.Levels[i].Details.Length > 0;
                if (hasBullets != wantsBullets)
                {
                    throw h.Fail($"Karma {i} {(hasBullets ? "has" : "lacks")} details on the sidebar from 0 to 8: \"{text}\"");
                }
            }
            yield break;
        }

        private static IEnumerator TextsAvoidCripple(Harness h)
        {
            for (int from = 0; from < 8; from++)
            {
                for (int to = from + 1; to <= 8; to++)
                {
                    string text = MithrixEffects.BannerText(from, to) + "\n" + MithrixEffects.SidebarText(from, to, Accent);
                    if (text.IndexOf("crippl", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw h.Fail($"the preview from {from} to {to} Karma says Cripple: \"{text}\"");
                    }
                }
            }
            yield break;
        }

        // ---- The banner ----

        private static IEnumerator OffShowsNoPreview(Harness h)
        {
            h.SetMithrixPreview(MithrixPreviewMode.Off);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal);
            yield return h.Holds(() => h.MithrixBannerText() == null && !h.SidebarVisible(), () => $"no preview ({h.DescribePreview()})");
        }

        private static IEnumerator BannerLinePerLevel(Harness h)
        {
            h.SetMithrixPreview(MithrixPreviewMode.Banner);
            yield return h.SetKarma(StartKarma);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal);

            string expected = ExpectedBanner(StartKarma, StartKarma + h.CurrentWarning().Amount);
            yield return h.Until(() => h.MithrixBannerText() == expected, () => $"the banner lines \"{expected}\" ({h.DescribePreview()})");
            if (h.SidebarVisible()) throw h.Fail($"the sidebar shows in Banner mode ({h.DescribePreview()})");
        }

        // The portal adds 2, but only 1 is left below the maximum.
        private static IEnumerator BannerStopsAtMax(Harness h)
        {
            h.SetMithrixPreview(MithrixPreviewMode.Banner);
            yield return h.SetKarma(7);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal, 1);

            string expected = $"Mithrix at Karma 8: {Texts.MithrixLines[8]}";
            yield return h.Until(() => h.MithrixBannerText() == expected, () => $"the banner line \"{expected}\" ({h.DescribePreview()})");
        }

        // ---- The sidebar ----

        private static IEnumerator SidebarShowsLevels(Harness h)
        {
            yield return ShowSidebar(h, StartKarma);
            int to = Math.Min(StartKarma + h.CurrentWarning().Amount, 8);
            MithrixPanel panel = h.MithrixPanel();

            Equal(h, panel.Header, $"Mithrix at Karma {to}", "the sidebar header");
            for (int level = StartKarma + 1; level <= to; level++)
            {
                string title = $"{level}:</color> {Texts.MithrixTitles[level]}";
                if (!panel.Body.Contains(title)) throw h.Fail($"the sidebar lacks \"{title}\" ({h.DescribePreview()})");
            }
            if (panel.Body.Contains($"{StartKarma}:</color>")) throw h.Fail($"the sidebar shows the current level ({h.DescribePreview()})");
            if (h.MithrixBannerText() != null) throw h.Fail($"the banner shows Mithrix lines in Sidebar mode ({h.DescribePreview()})");
        }

        // It joins the objective panel's column, below the objectives, rather than the fallback spot.
        private static IEnumerator SidebarUnderObjectives(Harness h)
        {
            yield return ShowSidebar(h, StartKarma);
            Transform panel = h.MithrixPanel().transform;
            Transform column = panel.parent;
            ObjectivePanelController objectives = column.GetComponentsInChildren<ObjectivePanelController>(true)
                .FirstOrDefault(found => found.transform.parent == column);
            if (!objectives) throw h.Fail($"the sidebar is under {column.name}, not in the objective panel's column");
            if (objectives.transform.GetSiblingIndex() > panel.GetSiblingIndex())
            {
                throw h.Fail($"the sidebar is above the objectives in {column.name}");
            }
            if (panel.GetSiblingIndex() != column.childCount - 1) throw h.Fail($"the sidebar isn't last in {column.name}");
        }

        // LookingGlass moves its stats panel to the end of the column once, a second after making it. The scenario does the
        // same move, so it doesn't depend on when LookingGlass made its panel, and the sidebar must go back below it.
        private static IEnumerator SidebarBelowLookingGlass(Harness h)
        {
            if (!Chainloader.PluginInfos.ContainsKey(LookingGlassGUID)) throw h.Skip("LookingGlass isn't installed");

            yield return ShowSidebar(h, StartKarma);
            Transform panel = h.MithrixPanel().transform;
            Transform column = panel.parent;
            Transform stats = column.Find(LookingGlassPanelName);
            if (!stats) throw h.Skip($"LookingGlass's stats panel isn't in {column.name} (it's off or detached)");

            stats.SetAsLastSibling();
            yield return h.Until(() => panel.GetSiblingIndex() == column.childCount - 1,
                () => $"the sidebar to go back below LookingGlass's stats (sidebar at {panel.GetSiblingIndex()}, stats at {stats.GetSiblingIndex()})");
            if (stats.GetSiblingIndex() > panel.GetSiblingIndex()) throw h.Fail("LookingGlass's stats are below the sidebar");
        }

        // It fades with the warning and then leaves the column, so it holds no empty gap under the objectives.
        private static IEnumerator SidebarHidesWithWarning(Harness h)
        {
            yield return ShowSidebar(h, StartKarma);
            yield return h.Until(() => h.MithrixPanel().TakesSpace, () => $"the sidebar to take its place in the column ({h.DescribePreview()})");
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
            yield return h.Until(() => !h.MithrixPanel().Visible && !h.MithrixPanel().TakesSpace,
                () => $"the sidebar to hide and leave the column ({h.DescribePreview()})");
        }

        // The Halcyon Shrine's advice tells you how to avoid the Karma, so neither mode adds a preview to it.
        private static IEnumerator AdviceHasNoPreview(Harness h)
        {
            h.SetMithrixPreview(MithrixPreviewMode.Sidebar);
            yield return ShrineScenarios.HalcyonDraining(h);
            yield return h.Holds(() => !h.SidebarVisible(), () => $"no sidebar with the advice ({h.DescribePreview()})");

            h.SetMithrixPreview(MithrixPreviewMode.Banner);
            yield return h.Holds(() => h.CurrentWarning().HideHeadline && h.MithrixBannerText() == null,
                () => $"the advice without Mithrix lines ({h.DescribePreview()})");
        }

        // ---- Helpers ----

        // In Sidebar mode at this Karma, faces the Null Portal and waits for the sidebar to show.
        private static IEnumerator ShowSidebar(Harness h, int karma)
        {
            h.SetMithrixPreview(MithrixPreviewMode.Sidebar);
            yield return h.SetKarma(karma);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal);
            yield return h.Until(() => h.SidebarVisible(), () => $"the sidebar to show ({h.DescribePreview()})");
        }

        // One line per level gained, capped at 8; a jump of more than DetailedLevels ends in a summary line.
        private static string ExpectedBanner(int from, int to)
        {
            to = Math.Min(to, 8);
            List<string> lines = new List<string>();
            for (int level = from + 1; level <= to; level++)
            {
                if (to - from > DetailedLevels && level - from == DetailedLevels)
                {
                    lines.Add($"Mithrix at Karma {level}–{to}: {to - level + 1} more levels of changes");
                    break;
                }
                lines.Add($"Mithrix at Karma {level}: {Texts.MithrixLines[level]}");
            }
            return string.Join("\n", lines);
        }

        private static void Equal(Harness h, string actual, string expected, string what)
        {
            if (actual != expected) throw h.Fail($"{what} is \"{actual}\", expected \"{expected}\"");
        }
    }
}
