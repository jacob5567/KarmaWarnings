using System.Collections;
using System.Collections.Generic;
using ModTestRunner;

namespace KarmaWarnings.Tests
{
    // The conditions every warning shares: how much Karma there is, and whether the player is aiming at the thing. The Null
    // Portal stands in for all of them.
    internal static class KarmaGateScenarios
    {
        private const string Suite = "gates";
        private static readonly string[] Triggers = { KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena };

        public static IEnumerable<Scenario> All()
        {
            yield return Make("low_karma_warns", LowKarmaWarns);
            yield return Make("amount_capped_to_what_is_left", AmountCapped);
            yield return Make("max_karma_silent", MaxKarmaSilent);
            yield return Make("not_aiming_silent", NotAimingSilent);
            yield return Make("banner_follows_aim", BannerFollowsAim);
        }

        private static Scenario Make(string name, System.Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, Triggers, run);
        }

        private static IEnumerator LowKarmaWarns(Harness h)
        {
            yield return h.SetKarma(0);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal);
        }

        // At 7 of 8 Karma only 1 more can be added, whatever the portal's reward and the Karma Modifier are.
        private static IEnumerator AmountCapped(Harness h)
        {
            yield return h.SetKarma(7);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectWarning(Texts.NullPortal, 1);
        }

        private static IEnumerator MaxKarmaSilent(Harness h)
        {
            yield return h.SetKarma(8);
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.ExpectNoWarning();
        }

        private static IEnumerator NotAimingSilent(Harness h)
        {
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
        }

        private static IEnumerator BannerFollowsAim(Harness h)
        {
            UnityEngine.GameObject portal = null;
            yield return PortalScenarios.SpawnAndFace(h, GameNames.NullPortal, spawned => portal = spawned);
            yield return h.ExpectWarning(Texts.NullPortal);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
            yield return h.AimAt(portal);
            yield return h.ExpectWarning(Texts.NullPortal);
        }
    }
}
