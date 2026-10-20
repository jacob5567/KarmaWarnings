using System.Collections.ObjectModel;
using RoR2;

namespace KarmaWarnings
{
    // The game's item-based Karma thresholds (RoR2.Karma.Collect* and GetEliteAffix), re-implemented read-only so the mod
    // can ask "would this pickup push me over?". The tier counts use GetTotalItemCountOfTier like the game does, so
    // temporary and rental items count; only the 50-item threshold counts permanent items alone.
    internal static class ItemRules
    {
        private const int LunarThreshold = 3;
        private const int MealThreshold = 3;
        private const int HallowedThreshold = 5;
        private const int LotsThreshold = 50;

        // Index = bit position in the masks the item detector builds.
        public static readonly string[] Ids =
        {
            KarmaTriggers.CollectLunarItems, KarmaTriggers.CollectMealItems, KarmaTriggers.CollectHallowedItems,
            KarmaTriggers.CollectLotsOfItems, KarmaTriggers.GetEliteAffix, KarmaTriggers.PledgeLoyalty
        };

        // What the player would end up holding, finishing "when you take this and have ...".
        public static readonly string[] Phrases =
        {
            "3 Lunar items", "3 Meal items", "5 Hallowed items", "50 items", "an Elite Aspect", "Beads of Fealty"
        };

        public static int IndexOf(string id)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                if (Ids[i] == id) return i;
            }
            return -1;
        }

        // True when this inventory already satisfies the trigger, which means the server has already fired it. Beads of
        // Fealty are an event, not a held state, so they can't be inferred this way.
        public static bool Met(int index, Inventory inventory)
        {
            switch (index)
            {
                case 0: return LunarCount(inventory, null, default) >= LunarThreshold;
                case 1:
                    return inventory.GetTotalItemCountOfTier(ItemTier.FoodTier) >= MealThreshold
                        || inventory.GetItemCountPermanent(DLC3Content.Items.WyrmOnHit) > 0;
                case 2: return inventory.GetTotalItemCountOfTier(ItemTier.Cursed) >= HallowedThreshold;
                case 3: return CountableItems(inventory) >= LotsThreshold;
                case 4: return IsEliteAspect(EquipmentCatalog.GetEquipmentDef(inventory.currentEquipmentIndex));
                default: return false;
            }
        }

        // True when taking this pickup would satisfy the trigger.
        public static bool WouldMeet(int index, Inventory inventory, PickupDef def, UniquePickup pickup)
        {
            ItemDef item = def.itemIndex != ItemIndex.None ? ItemCatalog.GetItemDef(def.itemIndex) : null;
            switch (index)
            {
                case 0: return LunarCount(inventory, def, pickup) >= LunarThreshold;
                case 1:
                    return (item && item.tier == ItemTier.FoodTier
                            && inventory.GetTotalItemCountOfTier(ItemTier.FoodTier) + 1 >= MealThreshold)
                        || def.itemIndex == DLC3Content.Items.WyrmOnHit.itemIndex;
                case 2:
                    return item && item.tier == ItemTier.Cursed
                        && inventory.GetTotalItemCountOfTier(ItemTier.Cursed) + 1 >= HallowedThreshold;
                case 3:
                    return IsCountable(item) && !pickup.isRental && !pickup.isTempItem
                        && CountableItems(inventory) + 1 >= LotsThreshold;
                case 4:
                    return def.equipmentIndex != EquipmentIndex.None
                        && IsEliteAspect(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex));
                case 5:
                    return def.itemIndex == RoR2Content.Items.LunarTrinket.itemIndex && !pickup.isRental;
                default: return false;
            }
        }

        // Lunar items plus one for a Lunar equipment. When the pickup is equipment it replaces the current one.
        private static int LunarCount(Inventory inventory, PickupDef def, UniquePickup pickup)
        {
            int count = inventory.GetTotalItemCountOfTier(ItemTier.Lunar);
            EquipmentIndex equipment = inventory.currentEquipmentIndex;
            if (def != null && def.itemIndex != ItemIndex.None
                && ItemCatalog.GetItemDef(def.itemIndex).tier == ItemTier.Lunar)
            {
                count++;
            }
            if (def != null && def.equipmentIndex != EquipmentIndex.None)
            {
                equipment = def.equipmentIndex;
            }
            EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipment);
            if (equipmentDef != null && equipmentDef.isLunar)
            {
                count++;
            }
            return count;
        }

        private static bool IsEliteAspect(EquipmentDef equipment)
        {
            return equipment != null && equipment.passiveBuffDef != null && equipment.passiveBuffDef.isElite;
        }

        private static bool IsCountable(ItemDef item)
        {
            return item && item.tier != ItemTier.Hex && !item.hidden && item.tier != ItemTier.NoTier && !item.isConsumed;
        }

        private static int CountableItems(Inventory inventory)
        {
            int total = 0;
            System.Collections.Generic.List<ItemIndex> order = inventory.itemAcquisitionOrder;
            for (int i = 0; i < order.Count; i++)
            {
                if (IsCountable(ItemCatalog.GetItemDef(order[i])))
                {
                    total += inventory.GetItemCountPermanent(order[i]);
                }
            }
            return total;
        }

        // The inventories the game's item triggers look at: every player's, as seen from any peer.
        public static ReadOnlyCollection<PlayerCharacterMasterController> PlayerControllers =>
            PlayerCharacterMasterController.instances;
    }
}
