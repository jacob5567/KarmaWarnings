using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModTestRunner;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KarmaWarnings.Tests
{
    // Things only the game's own stages hold: the preplaced Legendary Chests and the Vulture Eggs. Each stage picks
    // their places at random from a set, so the scenarios look for them at runtime and walk up to whichever is there.
    internal static class FixedPlaceScenarios
    {
        private const string Suite = "places";
        // Scene objects can switch on a few seconds after the stage loads.
        private const float FindTimeout = 20f;
        private const string GoldChestName = "GOLDCHEST_NAME";

        public static IEnumerable<Scenario> All()
        {
            yield return Chest("abyssal_depths_chest", Stages.AbyssalDepths);
            yield return Chest("sundered_grove_chest", Stages.SunderedGrove);
            yield return Chest("repurposed_crater_chest", Stages.RepurposedCrater);
            yield return new KarmaScenario(4, Suite, "vulture_eggs", Stages.SirensCall,
                new[] { KarmaTriggers.SuperRoboBallBossBeaten }, VultureEggsScenario);
        }

        private static Scenario Chest(string name, string scene)
        {
            return new KarmaScenario(4, Suite, name, scene, new[] { KarmaTriggers.OpenHiddenGoldChest }, ChestScenario);
        }

        private static IEnumerator ChestScenario(Harness h)
        {
            PurchaseInteraction chest = null;
            yield return h.Until(() => chest = PreplacedChest(), "the stage's Legendary Chest to appear", FindTimeout);
            yield return h.StandBefore(chest.gameObject);
            yield return h.ExpectWarning(Texts.LegendaryChest);
        }

        // The real eggs are placed in the scene, unlike the one combat.vulture_egg_aimed spawns.
        private static IEnumerator VultureEggsScenario(Harness h)
        {
            CharacterBody egg = null;
            yield return h.Until(() => egg = VultureEgg(), "a Vulture Egg to appear", FindTimeout);
            yield return h.StandBefore(egg);
            yield return h.ExpectWarning(Texts.EggsSummon);
        }

        // The stage's own Legendary Chest: part of the scene file (a scene id), still for sale. A chest the director
        // spawns has no scene id.
        public static PurchaseInteraction PreplacedChest()
        {
            return Object.FindObjectsOfType<PurchaseInteraction>().FirstOrDefault(purchase =>
                purchase.displayNameToken == GoldChestName && purchase.available
                && purchase.GetComponent<NetworkIdentity>() && purchase.GetComponent<NetworkIdentity>().sceneId.Value != 0);
        }

        public static CharacterBody VultureEgg()
        {
            return CharacterBody.readOnlyInstancesList.FirstOrDefault(body => body && body.healthComponent
                && body.healthComponent.alive && body.name.Contains("VultureEgg"));
        }

        // The hidden chamber's Fire and Ice Elder Lemurians are told apart by their masters' names.
        public static CharacterBody ElementalLemurian()
        {
            return CharacterBody.readOnlyInstancesList.FirstOrDefault(body => body && body.master
                && (body.master.name.StartsWith("LemurianBruiserMasterFire") || body.master.name.StartsWith("LemurianBruiserMasterIce")));
        }
    }
}
