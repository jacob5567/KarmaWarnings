using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // The stage's teleporter in each of its states. On stage 5 leaving starts the next loop, but only while the teleporter
    // is idle or fully charged: while it charges the player is already committed.
    //
    // The three Sky Meadow scenarios share one teleporter, so they run in this order and the later ones start from the
    // state the earlier ones left.
    internal static class TeleporterScenarios
    {
        private const string Suite = "teleporter";

        public static IEnumerable<Scenario> All()
        {
            yield return new KarmaScenario(2, Suite, "idle_on_stage_1_silent", Stages.TitanicPlains, null, IdleOnStage1);
            yield return Loop("idle_on_stage_5_warns_moon", IdleOnStage5);
            yield return Loop("aligned_to_next_stage_warns_new_loop", AlignedToNextStage);
            yield return Loop("charging_on_stage_5_silent", ChargingOnStage5);
            yield return Loop("charged_on_stage_5_warns_new_loop", ChargedOnStage5);
        }

        private static Scenario Loop(string name, System.Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(2, Suite, name, Stages.SkyMeadow, new[] { KarmaTriggers.LoopAtLeastOnce }, run);
        }

        public static TeleporterInteraction Teleporter(Harness h)
        {
            TeleporterInteraction teleporter = TeleporterInteraction.instance;
            if (!teleporter) throw h.Skip("this stage has no teleporter");
            return teleporter;
        }

        private static IEnumerator IdleOnStage1(Harness h)
        {
            TeleporterInteraction teleporter = Teleporter(h);
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectNoWarning();
        }

        // A Primordial Teleporter starts aligned with the moon: its prongs' state machine starts in IdleToActive and
        // settles in Active.
        private static IEnumerator IdleOnStage5(Harness h)
        {
            TeleporterInteraction teleporter = Teleporter(h);
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectWarning(Texts.GoToMoon);
        }

        // Using the prongs aligns it with the next stage (their Idle state). The later Sky Meadow scenarios keep that.
        private static IEnumerator AlignedToNextStage(Harness h)
        {
            TeleporterInteraction teleporter = Teleporter(h);
            EntityStateMachine prongs = teleporter.GetComponentsInChildren<EntityStateMachine>()
                .FirstOrDefault(machine => machine.state is EntityStates.LunarTeleporter.LunarTeleporterBaseState);
            if (!prongs) throw h.Skip("this teleporter has no prongs (not a Primordial Teleporter)");

            if (!(prongs.state is EntityStates.LunarTeleporter.Idle))
            {
                yield return h.Until(() => prongs.state is EntityStates.LunarTeleporter.Active, "the prongs to settle on the moon");
                yield return h.Interact(prongs.gameObject);
                yield return h.Until(() => prongs.state is EntityStates.LunarTeleporter.Idle, "the prongs to align with the next stage");
            }
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectWarning(Texts.NewLoop);
        }

        private static IEnumerator ChargingOnStage5(Harness h)
        {
            TeleporterInteraction teleporter = Teleporter(h);
            yield return h.Interact(teleporter.gameObject);
            yield return h.Until(() => teleporter.isCharging, "the teleporter to start charging");
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectNoWarning();

            // Cleanup kills every monster, the teleporter boss included. Under 10% charge that would earn KillBossQuickly
            // and make the quick-kill scenario skip, so the charge is pushed past it first.
            teleporter.holdoutZoneController.FullyChargeHoldoutZone();
            yield return h.Until(() => teleporter.chargeFraction > 0.1f, "the teleporter to pass 10% charge");
        }

        // The previous scenario started the charge; the zone is filled and then the boss killed, so the teleporter finishes.
        // Killing the boss first, under 10% charge, would earn KillBossQuickly and make the quick-kill scenario skip.
        private static IEnumerator ChargedOnStage5(Harness h)
        {
            TeleporterInteraction teleporter = Teleporter(h);
            if (!teleporter.isCharging && !teleporter.isCharged)
            {
                throw h.Skip("the teleporter was not charging: run teleporter.charging_on_stage_5_silent first");
            }

            teleporter.holdoutZoneController.FullyChargeHoldoutZone();
            yield return h.Until(() => teleporter.chargeFraction > 0.1f, "the teleporter to pass 10% charge");
            h.Cmd(Commands.KillAll);
            yield return h.Until(() => teleporter.isCharged, "the teleporter to finish charging");
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectWarning(Texts.NewLoop);
        }
    }
}
