using System.Collections;
using System.Linq;
using ModTestRunner;
using UnityEngine;

namespace KarmaWarnings.Tests
{
    // The KarmaWarnings checks, as Harness extension methods so scenarios call them like the runner's own helpers.
    //
    // The banner is read only through KarmaWarningsController.Showing/Current (and its Mithrix preview through Banner and
    // MithrixPanel), and Karma state only through KarmaTriggerState.IsEarned and KarmaGate.TryGetKarma.
    internal static class KarmaWarningsChecks
    {
        // The mod re-reads the world once per frame, so a warning from the previous step can still be up for a frame or two
        // after the change that should clear it.
        private const float ClearGraceSeconds = 0.5f;

        public static KarmaWarningsController Controller =>
            KarmaWarningsController.Instances.FirstOrDefault(controller => controller);

        // ---- The banner ----

        private static string ShowingNow()
        {
            KarmaWarningsController controller = Controller;
            if (controller == null) return "no HUD controller";
            return controller.Showing
                ? $"\"{controller.Current.Text}\" (+{controller.Current.Amount}{(controller.Current.HideHeadline ? ", no headline" : "")})"
                : "nothing";
        }

        private static bool IsShowing(string text)
        {
            KarmaWarningsController controller = Controller;
            return controller != null && controller.Showing && controller.Current.Text == text;
        }

        // Waits for the banner to show this text with its "Increases Karma" headline. Pass the Karma it should add to check that.
        public static IEnumerator ExpectWarning(this Harness h, string text, int? amount = null)
        {
            yield return h.Until(() => IsShowing(text), () => $"the warning \"{text}\" (showing {ShowingNow()}) {h.Diagnosis()}");

            KarmaWarning warning = Controller.Current;
            if (warning.HideHeadline) throw h.Fail($"\"{text}\" shows without the Increases Karma headline");
            if (amount.HasValue && warning.Amount != amount.Value)
            {
                throw h.Fail($"\"{text}\" adds {warning.Amount} Karma, expected {amount.Value}");
            }
        }

        // The Halcyon Shrine's message: advice, shown without the headline.
        public static IEnumerator ExpectAdvice(this Harness h, string text)
        {
            yield return h.Until(() => IsShowing(text), () => $"the advice \"{text}\" (showing {ShowingNow()}) {h.Diagnosis()}");
            if (!Controller.Current.HideHeadline) throw h.Fail($"\"{text}\" shows with the headline, but it is advice");
        }

        // The banner stays empty for a while. Fails the moment something shows.
        public static IEnumerator ExpectNoWarning(this Harness h, float seconds = Harness.DefaultHoldSeconds)
        {
            if (Controller == null) throw h.Fail("no HUD controller to read the banner from");

            float graceEnd = Time.realtimeSinceStartup + ClearGraceSeconds;
            while (Controller.Showing && Time.realtimeSinceStartup < graceEnd)
            {
                yield return null;
            }

            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                if (Controller.Showing) throw h.Fail($"expected no warning, but the banner shows {ShowingNow()} {h.Diagnosis()}");
                yield return null;
            }
        }

        // ---- Karma ----

        // Sets Karma with DebugToolkit and waits for the client-visible value to follow.
        public static IEnumerator SetKarma(this Harness h, int karma)
        {
            h.Cmd($"{Commands.KarmaCommand} {karma}");
            yield return h.Until(() =>
            {
                KarmaGate.TryGetKarma(out int now);
                return now == karma;
            }, $"Karma to reach {karma}");
        }

        public static IEnumerator ExpectEarned(this Harness h, string id)
        {
            yield return h.Until(() => KarmaTriggerState.IsEarned(id), $"{id} to be earned");
        }

        // ---- The Mithrix preview ----

        // Changes the MithrixPreview setting for this scenario only.
        public static void SetMithrixPreview(this Harness h, MithrixPreviewMode mode)
        {
            h.SetConfig(KarmaWarningsPlugin.Settings.MithrixPreview, mode);
        }

        // The warning on the banner as of the last frame.
        public static KarmaWarning CurrentWarning(this Harness h)
        {
            return Controller != null ? Controller.Current : default;
        }

        // The Mithrix lines under the warning, or null when the banner leaves them out.
        public static string MithrixBannerText(this Harness h)
        {
            return Controller != null && Controller.Banner ? Controller.Banner.MithrixText : null;
        }

        // The sidebar, or null before it is first shown.
        public static MithrixPanel MithrixPanel(this Harness h)
        {
            return Controller != null && Controller.MithrixPanel ? Controller.MithrixPanel : null;
        }

        public static bool SidebarVisible(this Harness h)
        {
            MithrixPanel panel = h.MithrixPanel();
            return panel && panel.Visible;
        }

        public static string DescribePreview(this Harness h)
        {
            MithrixPanel panel = h.MithrixPanel();
            string sidebar = panel ? $"\"{panel.Header}\" / \"{panel.Body}\" ({(panel.Visible ? "visible" : "hidden")})" : "never made";
            return $"banner lines \"{h.MithrixBannerText() ?? "(none)"}\", sidebar {sidebar}, warning {ShowingNow()}";
        }
    }
}
