using System.Collections;
using System.Collections.Generic;
using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // Interactables that commit the player to a milestone: the Shrine of Descent, the Halcyon Shrine and the Legendary Chest.
    internal static class ShrineScenarios
    {
        private const string Suite = "shrines";
        // Enough for any shrine's price.
        private const int Money = 100000;
        // The shrine takes a moment to open before its second press counts.
        private const float ShrineOpeningSeconds = 3f;
        private const int InteractionsWhenDraining = 2;

        public static IEnumerable<Scenario> All()
        {
            yield return new KarmaScenario(2, Suite, "descent_unused_warns", Stages.TitanicPlains,
                new[] { KarmaTriggers.SurviveMajorCatastrophe }, DescentUnused);
            yield return new KarmaScenario(2, Suite, "halcyon_idle_warns", Stages.TitanicPlains,
                new[] { KarmaTriggers.CompleteHalcyonShrine }, HalcyonIdle);
            yield return new KarmaScenario(2, Suite, "halcyon_draining_gives_advice", Stages.TitanicPlains,
                new[] { KarmaTriggers.CompleteHalcyonShrine }, HalcyonDraining);
            // The shrine changes the stage's teleporter for good, so it goes last in the stage.
            yield return new KarmaScenario(2, Suite, "descent_used_then_teleporter_warns", Stages.TitanicPlains,
                new[] { KarmaTriggers.SurviveMajorCatastrophe }, DescentUsed);
            yield return new KarmaScenario(2, Suite, "spawned_legendary_chest_silent", Stages.AbyssalDepths,
                new[] { KarmaTriggers.OpenHiddenGoldChest }, SpawnedChestSilent);
        }

        private static IEnumerator DescentUnused(Harness h)
        {
            ShrineCatastropheBehavior shrine = null;
            yield return h.Spawned<ShrineCatastropheBehavior>($"{Commands.SpawnInteractable} {GameNames.DescentShrineCard}", found => shrine = found);
            yield return h.StandBefore(shrine.gameObject);
            yield return h.ExpectWarning(Texts.Catastrophe);
        }

        // After the shrine is used it is silent itself, and the teleporter takes over the warning until it is charged.
        private static IEnumerator DescentUsed(Harness h)
        {
            TeleporterInteraction teleporter = TeleporterScenarios.Teleporter(h);
            ShrineCatastropheBehavior shrine = null;
            yield return h.Spawned<ShrineCatastropheBehavior>($"{Commands.SpawnInteractable} {GameNames.DescentShrineCard}", found => shrine = found);
            h.Cmd($"{Commands.GiveMoney} {Money}");
            PurchaseInteraction purchase = shrine.GetComponent<PurchaseInteraction>();
            yield return h.Interact(shrine.gameObject);
            yield return h.Until(() => purchase && !purchase.available, "the Shrine of Descent to be used");

            yield return h.StandBefore(shrine.gameObject);
            yield return h.ExpectNoWarning();
            yield return h.StandBefore(teleporter.gameObject);
            yield return h.ExpectWarning(Texts.Catastrophe);
        }

        private static IEnumerator HalcyonIdle(Harness h)
        {
            HalcyoniteShrineInteractable shrine = null;
            yield return h.Spawned<HalcyoniteShrineInteractable>($"{Commands.SpawnInteractable} {GameNames.HalcyonShrineCard}", found => shrine = found);
            yield return h.StandBefore(shrine.gameObject);
            yield return h.ExpectWarning(Texts.Halcyonite);
        }

        // Also used by MithrixPreviewScenarios.
        public static IEnumerator HalcyonDraining(Harness h)
        {
            HalcyoniteShrineInteractable shrine = null;
            yield return h.Spawned<HalcyoniteShrineInteractable>($"{Commands.SpawnInteractable} {GameNames.HalcyonShrineCard}", found => shrine = found);
            h.Cmd($"{Commands.GiveMoney} {Money}");
            yield return h.Interact(shrine.gameObject);
            yield return h.Wait(ShrineOpeningSeconds);
            yield return h.Interact(shrine.gameObject);
            yield return h.Until(() => shrine.interactions >= InteractionsWhenDraining, "the shrine to start draining");
            yield return h.ExpectAdvice(Texts.HalcyonAdvice);
        }

        // The same chest model, spawned by a command, isn't part of the stage file, so it isn't the Karma chest.
        private static IEnumerator SpawnedChestSilent(Harness h)
        {
            PurchaseInteraction chest = null;
            yield return h.Spawned<PurchaseInteraction>($"{Commands.SpawnInteractable} {GameNames.LegendaryChestCard}", found => chest = found);
            yield return h.StandBefore(chest.gameObject);
            yield return h.ExpectNoWarning();
        }
    }
}
