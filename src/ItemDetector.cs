using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace KarmaWarnings
{
    // Warns when picking something up would push the local player over a Karma item threshold: a pickup they're aiming
    // at, a shop terminal they're aiming at, or a pickup lying close enough to be taken just by walking over it.
    //
    // Not covered yet: an open item-choice panel, a ready CHEF meal, and items still falling out of a chest.
    internal sealed class ItemDetector : IWarningDetector
    {
        // How close a pickup has to be to count as one the player is about to walk over.
        private const float TouchRange = 5f;

        private readonly Dictionary<int, string> textByMask = new Dictionary<int, string>();

        public bool TryGetWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (!context.Inventory) return false;

            int mask = AimedMask(context) | NearbyMask(context);
            if (mask == 0) return false;

            int amount = 0;
            for (int i = 0; i < ItemRules.Ids.Length; i++)
            {
                if ((mask & (1 << i)) != 0 && !CountedEarlier(mask, i))
                {
                    amount += context.AmountFor(ItemRules.Ids[i]);
                }
            }
            amount = Mathf.Min(amount, KarmaManager.maxKarma - context.Karma);
            if (amount <= 0) return false;

            warning = new KarmaWarning { Amount = amount, Text = TextFor(mask), Priority = WarningPriority.Item };
            return true;
        }

        // True when an earlier set bit is for the same trigger, whose Karma is already counted.
        private static bool CountedEarlier(int mask, int index)
        {
            for (int j = 0; j < index; j++)
            {
                if ((mask & (1 << j)) != 0 && ItemRules.Ids[j] == ItemRules.Ids[index]) return true;
            }
            return false;
        }

        // The pickup, or the shop terminal's item, that the player is aiming at. A terminal warns only when the player can
        // afford it right now, and not when it's paid for with items, where the player gives one up. A hidden terminal
        // ("?") still warns: its item is synced to every player, only its display is hidden.
        private static int AimedMask(WarningContext context)
        {
            AimedTarget target = context.Target;
            if (target.Pickup)
            {
                return MaskFor(context, target.Pickup.pickup);
            }

            PurchaseInteraction purchase = target.Purchase;
            if (target.Shop && purchase && purchase.available && IsPaidWithMoney(purchase) && CanAfford(context, purchase))
            {
                return MaskFor(context, target.Shop.CurrentPickup());
            }
            return 0;
        }

        // The game's own check before a purchase goes through. It only reads synced state: the player's money or lunar
        // coins, the terminal's cost, and the cost adjustments (Longstanding Solitude, free unlocks).
        private static bool CanAfford(WarningContext context, PurchaseInteraction purchase)
        {
            Interactor interactor = context.Body.GetComponent<Interactor>();
            return interactor && purchase.CanBeAffordedByInteractor(interactor);
        }

        private static bool IsPaidWithMoney(PurchaseInteraction purchase)
        {
            return purchase.costType == CostTypeIndex.Money || purchase.costType == CostTypeIndex.LunarCoin;
        }

        private static int NearbyMask(WarningContext context)
        {
            int mask = 0;
            List<GenericPickupController> pickups = InstanceTracker.GetInstancesList<GenericPickupController>();
            Vector3 position = context.Body.corePosition;
            for (int i = 0; i < pickups.Count; i++)
            {
                GenericPickupController pickup = pickups[i];
                if (!pickup || pickup == context.Target.Pickup) continue;
                if ((pickup.transform.position - position).sqrMagnitude > TouchRange * TouchRange) continue;

                if (IsTakenOnTouch(context, pickup.pickup))
                {
                    mask |= MaskFor(context, pickup.pickup);
                }
            }
            return mask;
        }

        // The rules from GenericPickupController.OnTriggerStay: some items always need an interact press, and equipment is
        // only taken on touch when it isn't Lunar and the equipment slots aren't full. The game also holds back items from
        // a mutually exclusive group the player already carries; that case isn't checked, so it may warn a little early.
        private static bool IsTakenOnTouch(WarningContext context, UniquePickup pickup)
        {
            PickupDef def = PickupCatalog.GetPickupDef(pickup.pickupIndex);
            if (def == null) return false;

            if (def.itemIndex != ItemIndex.None)
            {
                ItemTierDef tier = ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(def.itemIndex).tier);
                if (tier)
                {
                    if (tier.pickupRules == ItemTierDef.PickupRules.ConfirmAll) return false;
                    if (tier.pickupRules == ItemTierDef.PickupRules.ConfirmFirst
                        && context.Inventory.GetItemCountEffective(def.itemIndex) <= 0)
                    {
                        return false;
                    }
                }
            }

            if (def.equipmentIndex != EquipmentIndex.None)
            {
                if (EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).isLunar || context.Inventory.EquipmentSetFull())
                {
                    return false;
                }
            }
            return true;
        }

        // Bit i is set when taking the pickup would satisfy ItemRules.Ids[i], a trigger that still applies.
        private static int MaskFor(WarningContext context, UniquePickup pickup)
        {
            PickupDef def = PickupCatalog.GetPickupDef(pickup.pickupIndex);
            if (def == null) return 0;

            int mask = 0;
            for (int i = 0; i < ItemRules.Ids.Length; i++)
            {
                if (context.Applies(ItemRules.Ids[i]) && ItemRules.WouldMeet(i, context.Inventory, def, pickup))
                {
                    mask |= 1 << i;
                }
            }
            return mask;
        }

        // Cached by mask, so the string is built once per combination instead of every frame.
        private string TextFor(int mask)
        {
            if (!textByMask.TryGetValue(mask, out string text))
            {
                StringBuilder builder = new StringBuilder("when you take this and have ");
                bool first = true;
                for (int i = 0; i < ItemRules.Phrases.Length; i++)
                {
                    if ((mask & (1 << i)) == 0) continue;

                    builder.Append(first ? "" : " and ").Append(ItemRules.Phrases[i]);
                    first = false;
                }
                text = builder.ToString();
                textByMask[mask] = text;
            }
            return text;
        }
    }
}
