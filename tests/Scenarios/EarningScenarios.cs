using System.Collections;
using System.Collections.Generic;
using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // Scenarios that earn a one-time milestone by doing it. They run last, because an earned milestone is no longer warned
    // about for the rest of the run, so every scenario that needs it unearned would skip.
    internal static class EarningScenarios
    {
        private const string Suite = "earning";
        private const int LunarThreshold = 3;
        private const int MealThreshold = 3;
        private const int HallowedThreshold = 5;
        private const int FiftyThreshold = 50;
        private const float MoneyAmount = 100000;
        private const float ChestTimeout = 20f;

        public static IEnumerable<Scenario> All()
        {
            yield return Earn("lunar_items", KarmaTriggers.CollectLunarItems, h => EarnByTier(h, ItemTier.Lunar, LunarThreshold, KarmaTriggers.CollectLunarItems));
            yield return Earn("meal_items", KarmaTriggers.CollectMealItems, h => EarnByTier(h, ItemTier.FoodTier, MealThreshold, KarmaTriggers.CollectMealItems));
            yield return Earn("hallowed_items", KarmaTriggers.CollectHallowedItems, h => EarnByTier(h, ItemTier.Cursed, HallowedThreshold, KarmaTriggers.CollectHallowedItems));
            yield return Earn("fifty_items", KarmaTriggers.CollectLotsOfItems, FiftyItems);
            yield return Earn("elite_aspect", KarmaTriggers.GetEliteAffix, EliteAspect);
            // Beads of Fealty count every time they're picked up from the ground, so this isn't a one-time milestone.
            yield return new KarmaScenario(1, Suite, "beads_of_fealty_counts_every_pickup", Stages.TitanicPlains, null, Beads);
            yield return new KarmaScenario(4, Suite, "legendary_chest", Stages.AbyssalDepths,
                new[] { KarmaTriggers.OpenHiddenGoldChest }, OpenChest, earns: true);
        }

        private static Scenario Earn(string name, string trigger, System.Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, new[] { trigger }, run, earns: true);
        }

        private static IEnumerator EarnByTier(Harness h, ItemTier tier, int threshold, string trigger)
        {
            string item = GameNames.ItemOfTier(tier);
            if (item == null) throw h.Skip($"no {tier} item is enabled in this run");

            yield return ItemScenarios.Give(h, item, threshold);
            yield return h.ExpectEarned(trigger);
            yield return NoMoreWarning(h, item);
        }

        private static IEnumerator FiftyItems(Harness h)
        {
            yield return ItemScenarios.Give(h, GameNames.FillerItem, FiftyThreshold);
            yield return h.ExpectEarned(KarmaTriggers.CollectLotsOfItems);
            yield return NoMoreWarning(h, GameNames.FillerItem);
        }

        private static IEnumerator EliteAspect(Harness h)
        {
            h.Cmd($"{Commands.GiveEquipment} {GameNames.EliteAspect}");
            yield return h.ExpectEarned(KarmaTriggers.GetEliteAffix);
            yield return NoMoreWarning(h, GameNames.EliteAspect);
        }

        // The game counts Beads of Fealty when a pickup is taken (GenericPickupController.onPickupConsumed), not when
        // they're given, and the trigger repeats. So: take one, check the Karma rose, and check the next one still warns.
        private static IEnumerator Beads(Harness h)
        {
            int before = (int)KarmaManager.instance.NetworkactualKarma;
            GenericPickupController pickup = h.SpawnPickup(GameNames.Beads);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.BeadsPhrase);
            yield return h.Interact(pickup.gameObject);
            yield return h.Until(() => KarmaManager.instance.NetworkactualKarma > before, "Karma to rise after taking Beads of Fealty");

            GenericPickupController next = h.SpawnPickup(GameNames.Beads);
            yield return h.StandBefore(next.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.BeadsPhrase);
        }

        // With the milestone earned, a pickup of the same kind that would cross the threshold again says nothing.
        private static IEnumerator NoMoreWarning(Harness h, string item)
        {
            GenericPickupController pickup = h.SpawnPickup(item);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectNoWarning();
        }

        // Opens the preplaced chest the way the player does: aim at it and use it.
        private static IEnumerator OpenChest(Harness h)
        {
            PurchaseInteraction chest = null;
            yield return h.Until(() => chest = FixedPlaceScenarios.PreplacedChest(), "the stage's Legendary Chest to appear", ChestTimeout);
            yield return h.StandBefore(chest.gameObject);
            h.Cmd($"{Commands.GiveMoney} {MoneyAmount}");

            InteractionDriver driver = h.Body.GetComponent<InteractionDriver>();
            Interactor interactor = h.Body.GetComponent<Interactor>();
            if (!driver || !interactor) throw h.Fail("the player body has no interaction components");

            yield return h.Until(() => driver.currentInteractable, "the chest to be aimed at");
            interactor.AttemptInteraction(driver.currentInteractable);
            yield return h.ExpectEarned(KarmaTriggers.OpenHiddenGoldChest);
        }
    }
}
