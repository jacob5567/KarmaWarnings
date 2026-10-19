using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Karma;

namespace KarmaWarnings
{
    // Which one-time Karma milestones are already earned this run, so they stop being warned about.
    //
    // The host reads the game's own flag. A client can't: BaseKarmaTrigger.isComplete is only set on the server. So a
    // client works it out from what it can see, and anything it can't tell counts as not earned (it warns).
    //
    // The mod never calls [Server] methods such as SetKarmaServer or Get/SetEventFlag: they are host-only and some change
    // the run. Clients infer state from the scene, inventory and DLC4ProgressionController instead.
    internal static class KarmaTriggerState
    {
        // A trigger whose Karma went up while the player was in its stage can't be left before it's earned, so a rise in
        // that stage means it's done. Several triggers can share a stage; they are all earned by the time it's left.
        private static readonly Dictionary<string, string[]> HomeStages = new Dictionary<string, string[]>
        {
            { "arena", new[] { KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena } },
            { "goldshores", new[] { KarmaTriggers.DefeatAurelionite } },
            { "meridian", new[] { KarmaTriggers.DefeatFalseSon } },
            { "solutionalhaunt", new[] { KarmaTriggers.DefeatSolusWing } },
            { "solusweb", new[] { KarmaTriggers.DefeatSolusHeart } },
            { "champrush", new[] { KarmaTriggers.BeatChampRushReaper } },
            { "ultima", new[] { KarmaTriggers.KillProvidence } },
            { "computationalexchange", new[] { KarmaTriggers.KickedOutOfCompExchange } }
        };

        private static readonly HashSet<string> earned = new HashSet<string>();
        private static int lastKarma = -1;

        public static void Install()
        {
            Run.onRunStartGlobal += OnRunBoundary;
            Run.onRunDestroyGlobal += OnRunBoundary;
            KarmaManager.onKarmaChangedClient += OnKarmaChanged;
        }

        public static void Uninstall()
        {
            Run.onRunStartGlobal -= OnRunBoundary;
            Run.onRunDestroyGlobal -= OnRunBoundary;
            KarmaManager.onKarmaChangedClient -= OnKarmaChanged;
        }

        private static void OnRunBoundary(Run run)
        {
            earned.Clear();
            lastKarma = -1;
        }

        private static void OnKarmaChanged(KarmaManager manager, int karma)
        {
            int previous = lastKarma;
            lastKarma = karma;
            if (previous < 0 || karma <= previous) return;

            SceneDef scene = SceneCatalog.GetSceneDefForCurrentScene();
            if (scene && HomeStages.TryGetValue(scene.cachedName, out string[] ids))
            {
                foreach (string id in ids)
                {
                    earned.Add(id);
                }
            }
        }

        // False for repeatable triggers (they can always fire again) and for anything not known to be done.
        public static bool IsEarned(string id)
        {
            KarmaManager manager = KarmaManager.instance;
            if (!manager) return false;

            KarmaTriggerDef def = manager.FindKarmaTriggerByIdentifier(id);
            BaseKarmaTrigger trigger = def != null ? manager.FindInstancedTrigger(def.karmaTriggerIndex) : null;
            if (trigger == null || trigger.IsRepeatable()) return false;
            if (trigger.isComplete || earned.Contains(id)) return true;

            if (InferFromWorld(id))
            {
                earned.Add(id);
                return true;
            }
            return false;
        }

        private static bool InferFromWorld(string id)
        {
            if (id == KarmaTriggers.BeatChampRushReaper)
            {
                DLC4ProgressionController progression = DLC4ProgressionController.instance;
                return progression && progression.ReaperBossDefeated;
            }

            // The server checks these on every inventory change, so if any player's inventory meets one now, it has fired.
            int index = ItemRules.IndexOf(id);
            if (index < 0) return false;

            foreach (PlayerCharacterMasterController player in ItemRules.PlayerControllers)
            {
                CharacterMaster master = player.master;
                if (master && master.inventory && ItemRules.Met(index, master.inventory)) return true;
            }
            return false;
        }

        // The F8 dump: every trigger this run uses, and whether the mod treats it as earned.
        public static string Describe()
        {
            StringBuilder text = new StringBuilder();
            foreach (string id in KarmaTriggers.All)
            {
                KarmaTriggerDef def = KarmaTriggers.FindInRun(id);
                if (def == null) continue;

                if (text.Length > 0)
                {
                    text.Append(", ");
                }
                text.Append(id).Append(" +").Append(def.baseKarmaReward).Append(IsEarned(id) ? " (earned)" : "");
            }
            return text.Length > 0 ? text.ToString() : "no triggers in this run";
        }
    }
}
