using RoR2;

namespace KarmaWarnings
{
    // Warns while the player aims at an interactable whose use commits them to a Karma milestone: the Shrine of Descent,
    // the Altar of Gold, the hidden Legendary Chests, and the Halcyon Shrine.
    internal sealed class InteractableDetector : IWarningDetector
    {
        private const string GoldChestName = "GOLDCHEST_NAME";
        // The shrine counts presses: the first starts it, the second (after the opening animation) starts the draining, and
        // a press after that cashes out early. isDraining isn't used for this: it is only true while a player with gold is
        // inside the shrine's radius, so it is false when you walk up to a shrine that is mid-drain.
        private const int InteractionsWhenDraining = 2;

        // Scenes with a preplaced Legendary Chest that counts. The same chest model spawned by the director elsewhere
        // doesn't, and is told apart by not being part of the scene file (its network scene id is empty).
        private static readonly string[] HiddenChestScenes = { "dampcavesimple", "rootjungle", "repurposedcrater" };

        private const string CatastropheText = "Surviving the catastrophe teleporter";

        private static readonly string[] CatastropheIds = { KarmaTriggers.SurviveMajorCatastrophe };
        private static readonly string[] AltarIds = { KarmaTriggers.DefeatAurelionite };
        private static readonly string[] ChestIds = { KarmaTriggers.OpenHiddenGoldChest };
        private static readonly string[] HalcyonIds = { KarmaTriggers.CompleteHalcyonShrine };

        public bool TryGetWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            AimedTarget target = context.Target;
            if (!target.Root) return false;

            // The Shrine of Descent makes the next teleporter a catastrophe teleporter; surviving it is the +1. The Reaper's
            // Karma is warned about at the Ritual Portal instead (PortalDetector), where it's actually committed.
            if (target.Descent && IsAvailable(target))
            {
                return context.TryMake(CatastropheText, WarningPriority.Stage, CatastropheIds, false, out warning);
            }

            if (target.Teleporter && !target.Teleporter.isCharged && IsCatastropheTeleporter())
            {
                return context.TryMake(CatastropheText, WarningPriority.Stage, CatastropheIds, false, out warning);
            }

            if (target.Statue && target.Statue.portalType == PortalStatueBehavior.PortalType.Goldshores && IsAvailable(target))
            {
                return context.TryMake("Gilded Coast: can't leave until Aurelionite dies",
                    WarningPriority.Stage, AltarIds, false, out warning);
            }

            if (IsHiddenChest(context) && IsAvailable(target))
            {
                return context.TryMake("Opening this Legendary Chest", WarningPriority.Interaction, ChestIds, false,
                    out warning);
            }

            if (target.Halcyon)
            {
                bool draining = target.Halcyon.interactions >= InteractionsWhenDraining;
                string text = draining
                    ? "Interact again before it fills to avoid Karma"
                    : "Fully draining this shrine and killing the Halcyonite";
                bool applies = context.TryMake(text, WarningPriority.Interaction, HalcyonIds, false, out warning);
                // Once it's draining the message is advice, so it has no "Increases Karma" headline.
                warning.HideHeadline = draining;
                return applies;
            }

            return false;
        }

        // The game's own client-side test for lighting the teleporter's catastrophe particles. Every input is synced.
        private static bool IsCatastropheTeleporter()
        {
            DLC4ProgressionController progression = DLC4ProgressionController.instance;
            return progression && progression.catastropheProvider && progression.catastropheProvider.WillTriggerMajorCatastrophe();
        }

        private static bool IsAvailable(AimedTarget target)
        {
            return !target.Purchase || target.Purchase.available;
        }

        private static bool IsHiddenChest(WarningContext context)
        {
            AimedTarget target = context.Target;
            if (!target.Purchase || target.Purchase.displayNameToken != GoldChestName) return false;
            if (System.Array.IndexOf(HiddenChestScenes, context.SceneName) < 0) return false;

            return target.Identity && target.Identity.sceneId.Value != 0;
        }
    }
}
