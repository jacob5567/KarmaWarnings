using RoR2;

namespace KarmaWarnings
{
    // The game's Karma trigger identifiers (the [RegisterKarmaTrigger] names in RoR2.Karma), and lookups for them.
    internal static class KarmaTriggers
    {
        public const string ArtifactWorldComplete = "ArtifactWorldComplete";
        public const string BeatChampRushReaper = "BeatChampRushReaper";
        public const string CollectHallowedItems = "CollectHallowedItems";
        public const string CollectLotsOfItems = "CollectLotsOfItems";
        public const string CollectLunarItems = "CollectLunarItems";
        public const string CollectMealItems = "CollectMealItems";
        public const string CompletedArena = "CompletedArena";
        public const string CompleteHalcyonShrine = "CompleteHalcyonShrine";
        public const string DefeatAurelionite = "DefeatAurelionite";
        public const string DefeatFalseSon = "DefeatFalseSon";
        public const string DefeatSolusHeart = "DefeatSolusHeart";
        public const string DefeatSolusWing = "DefeatSolusWing";
        public const string EnterArena = "EnterArena";
        public const string GetEliteAffix = "GetEliteAffix";
        public const string KickedOutOfBazaar = "KickedOutOfBazaar";
        public const string KickedOutOfCompExchange = "KickedOutOfCompExchange";
        public const string KillBossQuickly = "KillBossQuickly";
        public const string KilledElementalLemurians = "KilledElementalLemurians";
        public const string KillProvidence = "KillProvidence";
        public const string LoopAtLeastOnce = "LoopAtLeastOnce";
        public const string OpenHiddenGoldChest = "OpenHiddenGoldChest";
        public const string PledgeLoyalty = "PledgeLoyalty";
        public const string SuperRoboBallBossBeaten = "SuperRoboBallBossBeaten";
        public const string SurviveMajorCatastrophe = "SurviveMajorCatastrophe";

        // The triggers worth listing in the F8 dump. ReachedMysterySpace is left out on purpose: see the README notes.
        public static readonly string[] All =
        {
            ArtifactWorldComplete, BeatChampRushReaper, CollectHallowedItems, CollectLotsOfItems, CollectLunarItems,
            CollectMealItems, CompletedArena, CompleteHalcyonShrine, DefeatAurelionite, DefeatFalseSon,
            DefeatSolusHeart, DefeatSolusWing, EnterArena, GetEliteAffix, KickedOutOfBazaar, KickedOutOfCompExchange,
            KillBossQuickly, KilledElementalLemurians, KillProvidence, LoopAtLeastOnce, OpenHiddenGoldChest,
            PledgeLoyalty, SuperRoboBallBossBeaten, SurviveMajorCatastrophe
        };

        // Null when the trigger isn't in this run: Karma is off, or the trigger's expansion isn't enabled. A client's
        // KarmaManager builds the same trigger list, so this works for clients too.
        public static KarmaTriggerDef FindInRun(string id)
        {
            KarmaManager manager = KarmaManager.instance;
            if (!manager) return null;

            KarmaTriggerDef def = manager.FindKarmaTriggerByIdentifier(id);
            return def != null && manager.IsRunUsingTrigger(def.karmaTriggerIndex) ? def : null;
        }
    }
}
