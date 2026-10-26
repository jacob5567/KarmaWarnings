using System.Collections;
using System.Collections.Generic;
using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // Aiming at pickups and shop terminals that would push the player over an item threshold. The earned side (no warning
    // once a threshold is met) is in EarningScenarios.
    internal static class ItemScenarios
    {
        private const string Suite = "items";
        private const int LunarThreshold = 3;
        private const int MealThreshold = 3;
        private const int HallowedThreshold = 5;
        private const int FiftyThreshold = 50;
        // Enough for any multishop terminal at any difficulty and stage count a test run reaches.
        private const uint ShopMoney = 100000;
        private const uint AllMoney = 100000000;

        public static IEnumerable<Scenario> All()
        {
            yield return Make("lunar_pickup", KarmaTriggers.CollectLunarItems, h => TierPickup(h, ItemTier.Lunar, LunarThreshold, Texts.Lunar));
            yield return Make("lunar_pickup_below_threshold", KarmaTriggers.CollectLunarItems, LunarBelowThreshold);
            yield return Make("meal_pickup", KarmaTriggers.CollectMealItems, h => TierPickup(h, ItemTier.FoodTier, MealThreshold, Texts.Meal));
            yield return Make("sauteed_worms_pickup", KarmaTriggers.CollectMealItems, SauteedWormsPickup);
            yield return Make("hallowed_pickup", KarmaTriggers.CollectHallowedItems, h => TierPickup(h, ItemTier.Cursed, HallowedThreshold, Texts.Hallowed));
            yield return Make("fifty_items_pickup", KarmaTriggers.CollectLotsOfItems, FiftyPickup);
            yield return Make("elite_aspect_pickup", KarmaTriggers.GetEliteAffix, ElitePickup);
            yield return Make("beads_pickup", KarmaTriggers.PledgeLoyalty, BeadsPickup);
            yield return new KarmaScenario(1, Suite, "lunar_and_beads_phrases_join", Stages.TitanicPlains,
                new[] { KarmaTriggers.CollectLunarItems, KarmaTriggers.PledgeLoyalty }, LunarAndBeads);
            yield return Make("shop_terminal", KarmaTriggers.CollectLotsOfItems, ShopTerminal);
            yield return Make("shop_terminal_unaffordable_silent", KarmaTriggers.CollectLotsOfItems, ShopTerminalUnaffordable);
            yield return Make("far_pickup_silent", KarmaTriggers.CollectLotsOfItems, FarPickupSilent);
        }

        private static Scenario Make(string name, string trigger, System.Func<Harness, IEnumerator> run)
        {
            return new KarmaScenario(1, Suite, name, Stages.TitanicPlains, new[] { trigger }, run);
        }

        // One short of the threshold in a tier, then a pickup of that tier.
        public static IEnumerator TierPickup(Harness h, ItemTier tier, int threshold, string phrase)
        {
            string item = GameNames.ItemOfTier(tier);
            if (item == null) throw h.Skip($"no {tier} item is enabled in this run");

            yield return Give(h, item, threshold - 1);
            GenericPickupController pickup = h.SpawnPickup(item);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + phrase);
        }

        private static IEnumerator LunarBelowThreshold(Harness h)
        {
            string item = GameNames.ItemOfTier(ItemTier.Lunar);
            if (item == null) throw h.Skip("no Lunar item is enabled in this run");

            yield return Give(h, item, LunarThreshold - 2);
            GenericPickupController pickup = h.SpawnPickup(item);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectNoWarning();
        }

        public static IEnumerator FiftyPickup(Harness h)
        {
            yield return Give(h, GameNames.FillerItem, FiftyThreshold - 1);
            GenericPickupController pickup = h.SpawnPickup(GameNames.FillerItem);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.Fifty);
        }

        private static IEnumerator ElitePickup(Harness h)
        {
            GenericPickupController pickup = h.SpawnPickup(GameNames.EliteAspect);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.Elite);
        }

        // Sauteed Worms counts for the Meal trigger on its own, with no other Meal items held.
        private static IEnumerator SauteedWormsPickup(Harness h)
        {
            GenericPickupController pickup = h.SpawnPickup(GameNames.SauteedWorms);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.SauteedWorms);
        }

        private static IEnumerator BeadsPickup(Harness h)
        {
            GenericPickupController pickup = h.SpawnPickup(GameNames.Beads);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.BeadsPhrase);
        }

        // Two phrases at once are joined with "and", in the order of the table in MESSAGES.md.
        private static IEnumerator LunarAndBeads(Harness h)
        {
            string lunar = GameNames.ItemOfTier(ItemTier.Lunar);
            if (lunar == null) throw h.Skip("no Lunar item is enabled in this run");

            yield return Give(h, lunar, LunarThreshold - 1);
            GenericPickupController pickup = h.SpawnPickup(GameNames.Beads);
            yield return h.StandBefore(pickup.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.Lunar + " and " + Texts.BeadsPhrase);
        }

        // A multishop terminal you can afford warns like a pickup does, whether it shows its item or "?".
        private static IEnumerator ShopTerminal(Harness h)
        {
            yield return Give(h, GameNames.FillerItem, FiftyThreshold - 1);
            ShopTerminalBehavior terminal = null;
            yield return h.Spawned<ShopTerminalBehavior>($"{Commands.SpawnInteractable} {GameNames.MultishopCard}", found => terminal = found);
            yield return SetMoney(h, ShopMoney);

            yield return h.StandBefore(terminal.gameObject);
            yield return h.ExpectWarning(Texts.TakeThis + Texts.Fifty);
        }

        // The same terminal with no money: buying it isn't possible, so there is nothing to warn about.
        private static IEnumerator ShopTerminalUnaffordable(Harness h)
        {
            yield return Give(h, GameNames.FillerItem, FiftyThreshold - 1);
            ShopTerminalBehavior terminal = null;
            yield return h.Spawned<ShopTerminalBehavior>($"{Commands.SpawnInteractable} {GameNames.MultishopCard}", found => terminal = found);
            yield return SetMoney(h, 0);

            yield return h.StandBefore(terminal.gameObject);
            yield return h.ExpectNoWarning();
        }

        // Sets the player's money to exactly this amount. give_money with a negative amount stops at 0.
        private static IEnumerator SetMoney(Harness h, uint money)
        {
            h.Cmd($"{Commands.GiveMoney} -{AllMoney}");
            yield return h.Until(() => h.Body.master.money == 0, "no money");
            if (money == 0) yield break;

            h.Cmd($"{Commands.GiveMoney} {money}");
            yield return h.Until(() => h.Body.master.money >= money, $"{money} money");
        }

        // Nothing is aimed at and the pickup is out of reach.
        private static IEnumerator FarPickupSilent(Harness h)
        {
            yield return Give(h, GameNames.FillerItem, FiftyThreshold - 1);
            h.SpawnPickup(GameNames.FillerItem);
            yield return h.AimAway();
            yield return h.ExpectNoWarning();
        }

        // Gives permanent items with DebugToolkit and waits until the inventory has them.
        public static IEnumerator Give(Harness h, string item, int count)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(item);
            if (index == ItemIndex.None) throw h.Fail($"no item called {item}");

            h.Cmd($"{Commands.GiveItem} {item} {count}");
            yield return h.Until(() => h.Body.inventory.GetItemCountPermanent(index) >= count, $"{count} {item} in the inventory");
        }
    }
}
