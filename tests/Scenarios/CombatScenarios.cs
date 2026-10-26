using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModTestRunner;
using RoR2;
using UnityEngine;

namespace KarmaWarnings.Tests
{
    // Fights where the Karma comes from how they end.
    internal static class CombatScenarios
    {
        private const string Suite = "combat";
        // The boss is warned about once it is at half health.
        private const float BossLowHealth = 0.3f;
        // The teleporter's charge is held back this far, so the 10% threshold can't be passed while the boss is set up.
        private const float FrozenChargeSeconds = 1000000f;
        // A teleporter boss spawns some seconds after the teleporter starts, which is longer than any other wait.
        private const float BossSpawnTimeout = 30f;
        // The Newt kicks the player out at 99% health and SPEX at 90%.
        private const float NewtWarnHealth = 0.995f;
        private const float NewtKickedHealth = 0.95f;
        private const float SpexWarnHealth = 0.95f;
        private const float SpexKickedHealth = 0.85f;
        // The Newt can take a few seconds to appear after the player walks into the shop.
        private const float NewtTimeout = 20f;
        private const float LemurianTimeout = 30f;
        // Close enough to the trigger zone's middle to be inside it.
        private const float NewtTriggerStandOff = 0.5f;

        public static IEnumerable<Scenario> All()
        {
            // Stage 1 ends with the scenario that starts the teleporter.
            yield return new KarmaScenario(3, Suite, "teleporter_boss_before_10_percent", Stages.TitanicPlains,
                new[] { KarmaTriggers.KillBossQuickly }, TeleporterBoss);
            yield return new KarmaScenario(3, Suite, "newt_kick_out", Stages.Bazaar,
                new[] { KarmaTriggers.KickedOutOfBazaar }, Newt);
            yield return new KarmaScenario(3, Suite, "spex_kick_out", Stages.ComputationalExchange,
                new[] { KarmaTriggers.KickedOutOfCompExchange }, Spex);
            yield return new KarmaScenario(3, Suite, "elemental_lemurians_alive", Stages.AbandonedAqueduct,
                new[] { KarmaTriggers.KilledElementalLemurians }, Lemurians);
            yield return new KarmaScenario(3, Suite, "vulture_egg_aimed", Stages.SirensCall,
                new[] { KarmaTriggers.SuperRoboBallBossBeaten }, VultureEgg);
            yield return new KarmaScenario(3, Suite, "alloy_worship_unit_alive", Stages.SirensCall,
                new[] { KarmaTriggers.SuperRoboBallBossBeaten }, AlloyWorshipUnit);
            // After vulture_egg_aimed, which shows the egg warning on this stage before any Alloy Worship Unit.
            yield return new KarmaScenario(3, Suite, "vulture_egg_silent_after_alloy_worship_unit", Stages.SirensCall,
                new[] { KarmaTriggers.SuperRoboBallBossBeaten }, VultureEggAfterBoss);
        }

        private static IEnumerator TeleporterBoss(Harness h)
        {
            TeleporterInteraction teleporter = TeleporterScenarios.Teleporter(h);
            yield return h.Interact(teleporter.gameObject);
            teleporter.holdoutZoneController.baseChargeDuration = FrozenChargeSeconds;

            yield return h.Until(() => teleporter.bossGroup && teleporter.bossGroup.combatSquad
                && teleporter.bossGroup.combatSquad.memberCount > 0 && teleporter.bossGroup.totalMaxObservedMaxHealth > 0f,
                "the teleporter boss to spawn", BossSpawnTimeout);

            yield return h.ExpectNoWarning();
            foreach (CharacterMaster master in teleporter.bossGroup.combatSquad.readOnlyMembersList.ToList())
            {
                CharacterBody boss = master ? master.GetBody() : null;
                if (boss)
                {
                    h.SetHealthFraction(boss, BossLowHealth);
                }
            }
            yield return h.ExpectWarning(Texts.QuickBoss);
        }

        // Aimed at, away (still silent at full health), hurt to the last point where it can be avoided, then too late.
        private static IEnumerator Newt(Harness h)
        {
            // The Newt's master is placed in the Bazaar switched off. A trigger zone at the shop, SpawnShopkeeperTrigger,
            // calls its SpawnBodyHere when a player walks in.
            GameObject trigger = GameObject.Find(GameNames.NewtSpawnTrigger);
            if (!trigger) throw h.Skip($"no {GameNames.NewtSpawnTrigger} in this stage");
            Collider zone = trigger.GetComponent<Collider>();
            yield return h.StandNear(zone ? zone.bounds.center : trigger.transform.position, "the Newt's spawn trigger", NewtTriggerStandOff);

            CharacterBody newt = null;
            yield return h.Until(() => newt = h.FindBody(GameNames.NewtBody), "the Newt to appear", NewtTimeout);

            yield return h.StandBefore(newt);
            yield return h.ExpectWarning(Texts.Newt);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
            h.SetHealthFraction(newt, NewtWarnHealth);
            yield return h.ExpectWarning(Texts.Newt);
            h.SetHealthFraction(newt, NewtKickedHealth);
            yield return h.ExpectNoWarning();
            h.SetHealthFraction(newt, 1f);
        }

        private static IEnumerator Spex(Harness h)
        {
            ComputationalExchangeController exchange = ComputationalExchangeController.instance;
            if (!exchange || !exchange.solusVendorBody) throw h.Skip("no SPEX in this stage");
            // SPEX starts asleep; waking it is the setup, as talking to it would be.
            if (!exchange.isAwake) exchange.WakeUp();
            yield return h.Until(() => exchange.isAwake, "SPEX to wake up");

            CharacterBody spex = exchange.solusVendorBody;
            yield return h.StandBefore(spex);
            yield return h.ExpectWarning(Texts.Spex);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
            h.SetHealthFraction(spex, SpexWarnHealth);
            yield return h.ExpectWarning(Texts.Spex);
            h.SetHealthFraction(spex, SpexKickedHealth);
            yield return h.ExpectNoWarning();
            h.SetHealthFraction(spex, 1f);
        }

        // The warning shows only while the player aims at an elemental Elder Lemurian. They appear in the hidden chamber
        // several seconds after the stage loads.
        private static IEnumerator Lemurians(Harness h)
        {
            // In a run they spawn once the hidden chamber's pressure doors open, from master prefabs the mod tells apart by
            // name. The runner spawns one of those masters directly, braindead so it stays put.
            CharacterBody lemurian = FixedPlaceScenarios.ElementalLemurian();
            if (!lemurian)
            {
                h.Cmd($"{Commands.SpawnAi} {GameNames.FireLemurianMaster} 1 None 1");
                yield return h.Until(() => lemurian = FixedPlaceScenarios.ElementalLemurian(), "an elemental Elder Lemurian to spawn", LemurianTimeout);
            }

            yield return h.StandBefore(lemurian);
            yield return h.ExpectWarning(Texts.Lemurians);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
        }

        private static IEnumerator VultureEgg(Harness h)
        {
            CharacterBody egg = null;
            yield return h.Spawned<CharacterBody>($"{Commands.SpawnBody} {GameNames.VultureEggBody}", found => egg = found);
            yield return h.StandBefore(egg);
            yield return h.ExpectWarning(Texts.EggsSummon);
        }

        // Braindead, so it stays where the camera points. The warning shows only while the player aims at it.
        private static IEnumerator AlloyWorshipUnit(Harness h)
        {
            CharacterBody awu = null;
            yield return h.Spawned<CharacterBody>($"{Commands.SpawnAi} {GameNames.AlloyWorshipUnitMaster} 1 None 1", found => awu = found);
            yield return h.StandBefore(awu);
            yield return h.ExpectWarning(Texts.AlloyWorshipUnit);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
        }

        // Once the boss has appeared, the eggs left on the stage stop warning, also after it dies.
        private static IEnumerator VultureEggAfterBoss(Harness h)
        {
            CharacterBody awu = null;
            yield return h.Spawned<CharacterBody>($"{Commands.SpawnAi} {GameNames.AlloyWorshipUnitMaster} 1 None 1", found => awu = found);
            yield return h.StandBefore(awu);
            yield return h.ExpectWarning(Texts.AlloyWorshipUnit);

            h.Cmd(Commands.KillAll);
            yield return h.Until(() => !awu || !awu.healthComponent.alive, "the Alloy Worship Unit to die");

            CharacterBody egg = null;
            yield return h.Spawned<CharacterBody>($"{Commands.SpawnBody} {GameNames.VultureEggBody}", found => egg = found);
            yield return h.StandBefore(egg);
            yield return h.ExpectNoWarning();
        }
    }
}
