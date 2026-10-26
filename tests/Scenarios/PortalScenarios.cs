using System;
using System.Collections;
using System.Collections.Generic;
using ModTestRunner;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KarmaWarnings.Tests
{
    // Aiming at each portal that commits the player to a Karma milestone, and at the ones that don't.
    internal static class PortalScenarios
    {
        private const string Suite = "portals";

        public static IEnumerable<Scenario> All()
        {
            yield return Command("null_portal", GameNames.NullPortal, Texts.NullPortal,
                KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena);
            yield return Command("gold_portal", GameNames.GoldPortal, Texts.GoldPortal, KarmaTriggers.DefeatAurelionite);
            yield return Command("artifact_portal", GameNames.ArtifactPortal, Texts.ArtifactPortal, KarmaTriggers.ArtifactWorldComplete);
            // The green (Colossus) portal sends the player to Prime Meridian from Treeborn Colony (and its looped variant,
            // Golden Dieback).
            yield return new KarmaScenario(1, Suite, "green_portal_on_treeborn_colony", Stages.TreebornColony,
                new[] { KarmaTriggers.DefeatFalseSon }, h => AimAtCommandPortal(h, GameNames.GreenPortal, Texts.GreenPortal));
            // In a hidden realm the green portal leads where it would from the stage the player came from: an alternate
            // stage after stage 1, Prime Meridian after stage 3.
            yield return new KarmaScenario(1, Suite, "green_portal_on_gilded_coast_after_stage_3", Stages.TreebornColony,
                new[] { KarmaTriggers.DefeatFalseSon }, h => GreenPortalOnGildedCoast(h, true));
            yield return new KarmaScenario(1, Suite, "green_portal_silent_on_gilded_coast_after_stage_1", Stages.DistantRoost,
                new[] { KarmaTriggers.DefeatFalseSon }, h => GreenPortalOnGildedCoast(h, false));
            yield return Prefab("ritual_portal", GameNames.RitualPortalPrefab, Texts.RitualPortal, KarmaTriggers.BeatChampRushReaper);
            yield return Prefab("veil_portal", GameNames.VeilPortalPrefab, Texts.VeilPortal, KarmaTriggers.KillProvidence);
            yield return Prefab("decrypted_portal", GameNames.DecryptedPortalPrefab, Texts.DecryptedPortal, KarmaTriggers.DefeatSolusHeart);
            yield return new KarmaScenario(1, Suite, "solus_wing_portal", Stages.TitanicPlains, new[] { KarmaTriggers.DefeatSolusWing },
                h => AimAtCardPortal(h, GameNames.SolusWingPortalCard, Texts.SolusWingPortal));

            // Portals that only take the player somewhere ordinary.
            yield return new KarmaScenario(1, Suite, "blue_portal_silent_on_stage_1", Stages.TitanicPlains, null,
                h => Silent(h, GameNames.BluePortal));
            yield return new KarmaScenario(1, Suite, "celestial_portal_silent", Stages.TitanicPlains, null,
                h => Silent(h, GameNames.CelestialPortal));

            // Stage 5 is the last of a loop, so any exit starts the next one. A phase 2 scenario, to sit with the other
            // Sky Meadow ones.
            yield return new KarmaScenario(2, Suite, "new_loop_on_stage_5", Stages.SkyMeadow, new[] { KarmaTriggers.LoopAtLeastOnce },
                NewLoop);
        }

        private static Scenario Command(string name, string portal, string text, params string[] triggers)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, triggers, h => AimAtCommandPortal(h, portal, text));
        }

        private static Scenario Prefab(string name, string address, string text, params string[] triggers)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, triggers, h => AimAtPrefabPortal(h, address, text));
        }

        private static IEnumerator AimAtCommandPortal(Harness h, string portal, string text)
        {
            yield return SpawnAndFace(h, portal);
            yield return h.ExpectWarning(text);
        }

        private static IEnumerator AimAtCardPortal(Harness h, string card, string text)
        {
            SceneExitController exit = null;
            yield return h.Spawned<SceneExitController>($"{Commands.SpawnInteractable} {card}", found => exit = found);
            yield return h.StandBefore(exit.gameObject);
            yield return h.ExpectWarning(text);
        }

        private static IEnumerator AimAtPrefabPortal(Harness h, string address, string text)
        {
            NetworkIdentity portal = h.SpawnPrefab<NetworkIdentity>(address);
            yield return h.StandBefore(portal.gameObject);
            yield return h.ExpectWarning(text);
        }

        private static IEnumerator Silent(Harness h, string portal)
        {
            yield return SpawnAndFace(h, portal);
            yield return h.ExpectNoWarning();
        }

        // Starts on the scenario's stage so the mod sees it, then goes to Gilded Coast.
        private static IEnumerator GreenPortalOnGildedCoast(Harness h, bool leadsToMeridian)
        {
            yield return h.GoTo(Stages.GildedCoast);
            yield return SpawnAndFace(h, GameNames.GreenPortal);
            yield return leadsToMeridian ? h.ExpectWarning(Texts.GreenPortal) : h.ExpectNoWarning();
        }

        private static IEnumerator NewLoop(Harness h)
        {
            yield return SpawnAndFace(h, GameNames.BluePortal);
            yield return h.ExpectWarning(Texts.NewLoop);
        }

        // Spawns a portal with spawn_portal, which puts it in front of the player, then stands before it and looks at it.
        public static IEnumerator SpawnAndFace(Harness h, string portal, Action<GameObject> spawned = null)
        {
            SceneExitController exit = null;
            yield return h.Spawned<SceneExitController>($"{Commands.SpawnPortal} {portal}", found => exit = found);
            yield return h.StandBefore(exit.gameObject);
            spawned?.Invoke(exit.gameObject);
        }
    }
}
