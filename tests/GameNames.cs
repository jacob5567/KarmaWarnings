using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // Every name the scenarios take from the game, beyond ModTestRunner's Stages and Commands, in one place so a game
    // patch means one edit.
    // Several were taken from the Addressables catalog and DebugToolkit's help text and not yet run in game: a wrong one
    // shows up as a scenario that fails or skips with the name in its message.
    internal static class GameNames
    {
        // spawn_portal arguments.
        public const string NullPortal = "null";
        public const string GoldPortal = "gold";
        public const string ArtifactPortal = "artifact";
        public const string GreenPortal = "green";
        public const string BluePortal = "blue";
        public const string CelestialPortal = "celestial";

        // spawn_interactable arguments (spawn card names).
        public const string DescentShrineCard = "iscShrineCatastrophe";
        public const string HalcyonShrineCard = "iscShrineHalcyonite";
        public const string LegendaryChestCard = "iscGoldChest";
        public const string MultishopCard = "iscTripleShop";

        // Portals that no command spawns: Addressables keys of their prefabs.
        public const string RitualPortalPrefab = "RoR2/DLC4/ChampRushPortal.prefab";
        public const string VeilPortalPrefab = "RoR2/DLC4/champrush/PortalToUltima.prefab";
        public const string DecryptedPortalPrefab = "RoR2/DLC3/SolusWebPortal.prefab";
        // The spawn card for the portal to Solutional Haunt (HardwareProgPortal_Haunt). SolutionalHauntPortal is a
        // different portal: it has no fixed destination.
        public const string SolusWingPortalCard = "iscHardwareProgPortal_Haunt";

        // Monsters, as spawn_ai (master) and spawn_body (body) arguments.
        public const string AlloyWorshipUnitMaster = "SuperRoboBallBoss";
        public const string NewtBody = "ShopkeeperBody";
        public const string NewtSpawnTrigger = "SpawnShopkeeperTrigger";
        public const string FireLemurianMaster = "LemurianBruiserMasterFire";
        public const string IceLemurianMaster = "LemurianBruiserMasterIce";
        public const string VultureEggBody = "VultureEggBody";

        // Items and equipment by their catalog names.
        public static readonly string FillerItem = RoR2Content.Items.Syringe.name;
        public static readonly string Beads = RoR2Content.Items.LunarTrinket.name;
        public static readonly string SauteedWorms = DLC3Content.Items.WyrmOnHit.name;
        public static readonly string EliteAspect = RoR2Content.Equipment.AffixRed.name;

        // The first usable item of a tier that the run has enabled, or null (for example the Meal tier without DLC3).
        // Beads of Fealty is skipped: it has its own scenarios and would add its own phrase.
        public static string ItemOfTier(ItemTier tier)
        {
            return Harness.ItemOfTier(tier, item => item.name == Beads);
        }
    }

    // The banner texts in MESSAGES.md.
    internal static class Texts
    {
        public const string NullPortal = "when you start and finish the Void cells";
        public const string GoldPortal = "when you kill Aurelionite";
        public const string ArtifactPortal = "when you finish the Artifact Trial";
        public const string RitualPortal = "when you kill the Anima Harvester";
        public const string VeilPortal = "when you kill the secret boss";
        public const string DecryptedPortal = "when you kill the Solus Heart";
        public const string SolusWingPortal = "when you kill the Solus Wing";
        public const string GreenPortal = "when you kill False Son";
        public const string NewLoop = "when you start a new loop";
        public const string GoToMoon = "when you go to the moon";

        public const string Catastrophe = "when you survive the catastrophe teleporter";
        public const string LegendaryChest = "when you open this Legendary Chest";
        public const string Halcyonite = "when you kill the Halcyonite";
        public const string HalcyonAdvice = "Interact again before it fills to avoid Karma";

        public const string QuickBoss = "when you kill the boss under 10% charge";
        public const string Newt = "when the Newt kicks you out";
        public const string Spex = "when SPEX kicks you out";
        public const string Lemurians = "when you kill both elemental Elder Lemurians";
        public const string AlloyWorshipUnit = "when you kill the Alloy Worship Unit";
        public const string EggsSummon = "when you kill the Alloy Worship Unit that six eggs summon";

        public const string TakeThis = "when you take this and have ";
        public const string Lunar = "3 Lunar items";
        public const string Meal = "3 Meal items";
        public const string SauteedWorms = "Saut\u00e9ed Worms";
        public const string Hallowed = "5 Hallowed items";
        public const string Fifty = "50 items";
        public const string Elite = "an Elite Aspect";
        public const string BeadsPhrase = "Beads of Fealty";

        // The Mithrix preview, by Karma level (index 0 is unused): the banner line after "Mithrix at Karma N: ", and the
        // sidebar title.
        public static readonly string[] MithrixLines =
        {
            null,
            "shields himself after every 10% of health lost",
            "+30% speed; leap bombs, gravity pull, dash spikes",
            "hammer and needle attacks leave Lunar Ruin spikes",
            "landings call a meteor storm",
            "summons Karmic Spires that slow and shoot you",
            "halved cooldowns, elite monsters, new phase 4 attacks",
            "+1% damage, nothing new",
            "+50% max health",
        };
        public static readonly string[] MithrixTitles =
        {
            null, "Damage shell", "+30% speed", "Lunar Ruin spikes", "Meteor storm", "Karmic Spires", "Halved cooldowns",
            "Damage only", "+50% max health",
        };
    }
}
