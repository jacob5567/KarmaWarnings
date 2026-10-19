using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;

namespace KarmaWarnings
{
    // Warns during fights where the Karma comes from how it ends: a teleporter boss killed before the teleporter is 10%
    // charged, the Newt and SPEX kicking the player out, the elemental Elder Lemurians, and the Alloy Worship Unit.
    //
    // Which stage the player is in gates every check, so the body scan only runs in the few stages that need it.
    internal sealed class CombatDetector : IWarningDetector
    {
        // Under this much charge, killing the teleporter's boss adds Karma (RoR2.Karma.KillABossQuickly).
        private const float QuickKillChargeFraction = 0.1f;
        // Warn once the boss is this far down, so the player still has time to stand in the zone.
        private const float BossHealthFractionToWarn = 0.5f;
        // The Newt kicks the player out once hurt to this fraction of its health or lower, and SPEX does at its own.
        private const float NewtKickHealthFraction = 0.99f;
        private const float SpexKickHealthFraction = 0.9f;
        private const float AimDistance = 80f;
        private const float AimToleranceDegrees = 4f;

        private static readonly string[] BossIds = { KarmaTriggers.KillBossQuickly };
        private static readonly string[] NewtIds = { KarmaTriggers.KickedOutOfBazaar };
        private static readonly string[] SpexIds = { KarmaTriggers.KickedOutOfCompExchange };
        private static readonly string[] LemurianIds = { KarmaTriggers.KilledElementalLemurians };
        private static readonly string[] AwuIds = { KarmaTriggers.SuperRoboBallBossBeaten };

        private BodyIndex newtIndex = BodyIndex.None;

        public bool TryGetWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            switch (context.SceneName)
            {
                case "bazaar": return Newt(context, out warning);
                case "computationalexchange": return Spex(context, out warning);
                case "goolake": return ElementalLemurians(context, out warning);
                case "shipgraveyard": return AlloyWorshipUnit(context, out warning);
            }
            return TeleporterBoss(context, out warning);
        }

        private static bool TeleporterBoss(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            TeleporterInteraction teleporter = TeleporterInteraction.instance;
            if (!teleporter || teleporter.isCharged || !teleporter.holdoutZoneController || !teleporter.bossGroup) return false;

            BossGroup boss = teleporter.bossGroup;
            if (!boss.combatSquad || boss.combatSquad.memberCount == 0 || boss.totalMaxObservedMaxHealth <= 0f) return false;
            if (teleporter.chargeFraction > QuickKillChargeFraction) return false;
            if (boss.totalObservedHealth / boss.totalMaxObservedMaxHealth > BossHealthFractionToWarn) return false;

            return context.TryMake("Killing the boss before the teleporter is 10% charged adds Karma", WarningPriority.Combat,
                BossIds, false, out warning);
        }

        private bool Newt(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (newtIndex == BodyIndex.None)
            {
                newtIndex = BodyCatalog.FindBodyIndex("ShopkeeperBody");
            }

            CharacterBody newt = FindBody(body => body.bodyIndex == newtIndex);
            if (!newt) return false;

            float health = newt.healthComponent.healthFraction;
            if (health >= 1f && !IsAimingAt(context.Body, newt)) return false;

            string text = health <= NewtKickHealthFraction
                ? "The Newt is going to kick you out of the Bazaar"
                : "Hurting the Newt gets you kicked out of the Bazaar";
            return context.TryMake(text, WarningPriority.Combat, NewtIds, false, out warning);
        }

        private static bool Spex(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            ComputationalExchangeController exchange = ComputationalExchangeController.instance;
            if (!exchange || !exchange.solusVendorBody || !exchange.isAwake) return false;

            CharacterBody spex = exchange.solusVendorBody;
            float health = spex.healthComponent.healthFraction;
            if (health >= 1f && !IsAimingAt(context.Body, spex)) return false;

            string text = health <= SpexKickHealthFraction
                ? "SPEX is going to kick you out of the Computational Exchange"
                : "Hurting SPEX much more gets you kicked out of the Computational Exchange";
            return context.TryMake(text, WarningPriority.Combat, SpexIds, false, out warning);
        }

        // The two elemental Elder Lemurians in the hidden chamber have their own masters. The names are from the scene's
        // spawn setup and haven't been checked in game, so DebugLogging logs what the scan finds.
        private static bool ElementalLemurians(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            CharacterBody lemurian = FindBody(body => body.master && IsElementalLemurianMaster(body.master.name));
            if (!lemurian) return false;

            return context.TryMake("Killing both elemental Elder Lemurians adds Karma", WarningPriority.Combat, LemurianIds, false,
                out warning);
        }

        private static bool IsElementalLemurianMaster(string name)
        {
            return name.StartsWith("LemurianBruiserFireMaster") || name.StartsWith("LemurianBruiserIceMaster");
        }

        // Destroying six Vulture Eggs summons the Alloy Worship Unit, and killing it adds Karma. The same name test the
        // game uses to find the eggs.
        private static bool AlloyWorshipUnit(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            CharacterBody awu = FindBody(body => body.name.StartsWith("SuperRoboBallBossBody"));
            if (awu)
            {
                return context.TryMake("Killing the Alloy Worship Unit adds Karma", WarningPriority.Combat, AwuIds, false, out warning);
            }

            CharacterBody egg = FindBody(body => body.name.Contains("VultureEgg") && IsAimingAt(context.Body, body));
            return egg && context.TryMake("Destroying six Vulture Eggs summons the Alloy Worship Unit, and killing it adds Karma",
                WarningPriority.Combat, AwuIds, false, out warning);
        }

        private static CharacterBody FindBody(System.Predicate<CharacterBody> match)
        {
            ReadOnlyCollection<CharacterBody> bodies = CharacterBody.readOnlyInstancesList;
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] && match(bodies[i])) return bodies[i];
            }
            return null;
        }

        // Whether the player's crosshair is on the target, from the aim direction alone. There is no raycast: the game's
        // own aim logic isn't ours to call.
        private static bool IsAimingAt(CharacterBody player, CharacterBody target)
        {
            if (!player.inputBank) return false;

            Vector3 toTarget = target.corePosition - player.inputBank.aimOrigin;
            float distance = toTarget.magnitude;
            if (distance > AimDistance || distance < 0.01f) return false;

            float tolerance = AimToleranceDegrees + Mathf.Atan(target.radius / distance) * Mathf.Rad2Deg;
            return Vector3.Angle(player.inputBank.aimDirection, toTarget) <= tolerance;
        }
    }
}
