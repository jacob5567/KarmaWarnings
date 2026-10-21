using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using RoR2;
using UnityEngine;

namespace KarmaWarnings
{
    // Warns during fights where the Karma comes from how it ends: a teleporter boss killed before the teleporter is 10%
    // charged, the Newt and SPEX kicking the player out, the elemental Elder Lemurians, and the Alloy Worship Unit. The
    // optional bosses (the Elder Lemurians, the Alloy Worship Unit and its eggs) only warn while the player aims at them,
    // since leaving them alone avoids the Karma.
    //
    // Which stage the player is in gates the body scans, so they only run in the few stages that need them.
    internal sealed class CombatDetector : IWarningDetector
    {
        // Under this much charge, killing the teleporter's boss adds Karma (RoR2.Karma.KillABossQuickly).
        private const float QuickKillChargeFraction = 0.1f;
        // Warn once the boss is this far down, so the player still has time to stand in the zone.
        private const float BossHealthFractionToWarn = 0.5f;
        // The Newt kicks the player out once hurt to this fraction of its health or lower, and SPEX does at its own. There
        // is no warning at or below these: the player is warned, not told afterwards.
        private const float NewtKickHealthFraction = 0.99f;
        private const float SpexKickHealthFraction = 0.9f;
        private const float AimDistance = 80f;
        private const float AimToleranceDegrees = 4f;

        private static readonly string[] BossIds = { KarmaTriggers.KillBossQuickly };
        private static readonly string[] NewtIds = { KarmaTriggers.KickedOutOfBazaar };
        private static readonly string[] SpexIds = { KarmaTriggers.KickedOutOfCompExchange };
        private static readonly string[] LemurianIds = { KarmaTriggers.KilledElementalLemurians };
        private static readonly string[] AwuIds = { KarmaTriggers.SuperRoboBallBossBeaten };

        // Looked up by name once the body catalog is ready, so the per-frame scans compare indexes, not object names
        // (reading a Unity object's name allocates a new string every time).
        private BodyIndex newtIndex = BodyIndex.None;
        private BodyIndex awuIndex = BodyIndex.None;
        private BodyIndex eggIndex = BodyIndex.None;

        // The elemental Elder Lemurians are told apart only by their master's name, so each body is checked once per
        // stage. Bodies whose master hasn't arrived yet aren't cached.
        private readonly Dictionary<CharacterBody, bool> elementalByBody = new Dictionary<CharacterBody, bool>();
        private int elementalSceneHandle = -1;

        // Once the Alloy Worship Unit has been seen the eggs stop warning, even after it dies. The encounter state that
        // counts the eggs only runs on the server, so a client can only tell the fight began from the boss's body.
        private int awuSceneHandle = -1;
        private bool awuSeen;

        public bool TryGetWarning(WarningContext context, out KarmaWarning warning)
        {
            switch (context.SceneName)
            {
                case "bazaar": return Newt(context, out warning);
                case "computationalexchange": return Spex(context, out warning);
                case "goolake":
                    if (ElementalLemurians(context, out warning)) return true;
                    break;
                case "shipgraveyard":
                    if (AlloyWorshipUnit(context, out warning)) return true;
                    break;
            }
            // Abandoned Aqueduct and Siren's Call have a teleporter too.
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

            return context.TryMake("when you kill the boss under 10% charge", WarningPriority.Combat,
                BossIds, false, out warning);
        }

        private bool Newt(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            CharacterBody newt = FindBody(Lookup(ref newtIndex, "ShopkeeperBody"));
            if (!newt) return false;

            // Only a warning while it can still be avoided: once the Newt is hurt past the kick threshold it's too late.
            float health = newt.healthComponent.healthFraction;
            if (health <= NewtKickHealthFraction || (health >= 1f && !IsAimingAt(context.Body, newt))) return false;

            return context.TryMake("when the Newt kicks you out", WarningPriority.Combat, NewtIds, false, out warning);
        }

        private static bool Spex(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            ComputationalExchangeController exchange = ComputationalExchangeController.instance;
            if (!exchange || !exchange.solusVendorBody || !exchange.isAwake) return false;

            CharacterBody spex = exchange.solusVendorBody;
            float health = spex.healthComponent.healthFraction;
            if (health <= SpexKickHealthFraction || (health >= 1f && !IsAimingAt(context.Body, spex))) return false;

            return context.TryMake("when SPEX kicks you out", WarningPriority.Combat, SpexIds, false, out warning);
        }

        // The two elemental Elder Lemurians in the hidden chamber have their own masters, placed in the scene rather than
        // spawned from the catalog: LemurianBruiserMasterFire and LemurianBruiserMasterIce, as seen in game. The F8 dump
        // lists the Elder Lemurian masters it finds.
        private bool ElementalLemurians(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (elementalSceneHandle != context.SceneHandle)
            {
                elementalSceneHandle = context.SceneHandle;
                elementalByBody.Clear();
            }

            if (!IsAimingAtElementalLemurian(context.Body)) return false;

            return context.TryMake("when you kill both elemental Elder Lemurians", WarningPriority.Combat, LemurianIds, false,
                out warning);
        }

        private bool IsAimingAtElementalLemurian(CharacterBody player)
        {
            ReadOnlyCollection<CharacterBody> bodies = CharacterBody.readOnlyInstancesList;
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] && IsElementalLemurian(bodies[i]) && IsAimingAt(player, bodies[i])) return true;
            }
            return false;
        }

        private bool IsElementalLemurian(CharacterBody body)
        {
            if (elementalByBody.TryGetValue(body, out bool elemental)) return elemental;

            CharacterMaster master = body.master;
            if (!master) return false;

            elemental = IsElementalLemurianMaster(master.name);
            elementalByBody[body] = elemental;
            return elemental;
        }

        private static bool IsElementalLemurianMaster(string name)
        {
            return name.StartsWith("LemurianBruiserMasterFire") || name.StartsWith("LemurianBruiserMasterIce");
        }

        // Destroying six Vulture Eggs summons the Alloy Worship Unit, and killing it adds Karma.
        private bool AlloyWorshipUnit(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (awuSceneHandle != context.SceneHandle)
            {
                awuSceneHandle = context.SceneHandle;
                awuSeen = false;
            }

            CharacterBody awu = FindBody(Lookup(ref awuIndex, "SuperRoboBallBossBody"));
            if (awu) awuSeen = true;
            if (awu)
            {
                return IsAimingAt(context.Body, awu)
                    && context.TryMake("when you kill the Alloy Worship Unit", WarningPriority.Combat, AwuIds, false, out warning);
            }

            return !awuSeen && IsAimingAtEgg(context.Body) && context.TryMake("when you kill the Alloy Worship Unit that six eggs summon",
                WarningPriority.Combat, AwuIds, false, out warning);
        }

        private bool IsAimingAtEgg(CharacterBody player)
        {
            BodyIndex egg = Lookup(ref eggIndex, "VultureEggBody");
            if (egg == BodyIndex.None) return false;

            ReadOnlyCollection<CharacterBody> bodies = CharacterBody.readOnlyInstancesList;
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] && bodies[i].bodyIndex == egg && IsAimingAt(player, bodies[i])) return true;
            }
            return false;
        }

        // One line for the F8 dump: what the fight checks for this stage can see.
        public string Describe(WarningContext context)
        {
            StringBuilder text = new StringBuilder();
            TeleporterInteraction teleporter = TeleporterInteraction.instance;
            if (teleporter)
            {
                text.Append($"teleporter {teleporter.activationState} at {teleporter.chargeFraction:P0}");
                BossGroup boss = teleporter.bossGroup;
                if (boss && boss.combatSquad && boss.combatSquad.memberCount > 0 && boss.totalMaxObservedMaxHealth > 0f)
                {
                    text.Append($", boss at {boss.totalObservedHealth / boss.totalMaxObservedMaxHealth:P0}");
                }
            }
            else
            {
                text.Append("no teleporter");
            }

            switch (context.SceneName)
            {
                case "bazaar":
                    CharacterBody newt = FindBody(Lookup(ref newtIndex, "ShopkeeperBody"));
                    text.Append(newt ? $"; Newt at {newt.healthComponent.healthFraction:P1}" : "; no Newt found");
                    break;
                case "computationalexchange":
                    ComputationalExchangeController exchange = ComputationalExchangeController.instance;
                    text.Append(exchange && exchange.solusVendorBody
                        ? $"; SPEX awake={exchange.isAwake} at {exchange.solusVendorBody.healthComponent.healthFraction:P1}"
                        : "; no SPEX found");
                    break;
                case "goolake":
                    // Every Elder Lemurian master, so the elemental ones' real names can be checked.
                    List<string> names = new List<string>();
                    foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body && body.master && body.master.name.Contains("LemurianBruiser"))
                        {
                            names.Add(body.master.name + (IsElementalLemurian(body) ? " (elemental)" : ""));
                        }
                    }
                    text.Append(names.Count > 0 ? "; Elder Lemurian masters: " + string.Join(", ", names) : "; no Elder Lemurians");
                    break;
                case "shipgraveyard":
                    BodyIndex egg = Lookup(ref eggIndex, "VultureEggBody");
                    int eggs = 0;
                    foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body && egg != BodyIndex.None && body.bodyIndex == egg) eggs++;
                    }
                    bool awu = FindBody(Lookup(ref awuIndex, "SuperRoboBallBossBody"));
                    text.Append($"; {eggs} Vulture Eggs left, Alloy Worship Unit {(awu ? "alive" : "not here")}");
                    break;
            }
            return text.ToString();
        }

        private static BodyIndex Lookup(ref BodyIndex cached, string bodyName)
        {
            if (cached == BodyIndex.None)
            {
                cached = BodyCatalog.FindBodyIndex(bodyName);
            }
            return cached;
        }

        private static CharacterBody FindBody(BodyIndex index)
        {
            if (index == BodyIndex.None) return null;

            ReadOnlyCollection<CharacterBody> bodies = CharacterBody.readOnlyInstancesList;
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] && bodies[i].bodyIndex == index) return bodies[i];
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
