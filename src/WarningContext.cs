using RoR2;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarmaWarnings
{
    // What the detectors read each frame: the local player's body, Karma, the current stage, and what they're aiming at.
    // Also builds the warnings, so every detector agrees on which triggers still apply and how much Karma they add.
    internal sealed class WarningContext
    {
        public CharacterBody Body;
        public Inventory Inventory;
        public int Karma;
        public SceneDef Scene;
        public string SceneName = "";
        public int StageOrder;
        public readonly AimedTarget Target = new AimedTarget();

        private int sceneHandle = -1;

        // Stage 5 is the last of a loop: leaving it starts the next one. The game doesn't count leaving the moon, and
        // Prismatic Trials never loop.
        public bool LeavesLoopStage =>
            StageOrder == 5 && SceneName != "moon" && SceneName != "moon2" && !(Run.instance is WeeklyRun);

        public void Update(CharacterBody body, int karma, GameObject aimed)
        {
            Body = body;
            Inventory = body.inventory;
            Karma = karma;

            int handle = SceneManager.GetActiveScene().handle;
            if (handle != sceneHandle)
            {
                sceneHandle = handle;
                Scene = SceneCatalog.GetSceneDefForCurrentScene();
                SceneName = Scene ? Scene.cachedName : "";
                StageOrder = Scene ? Scene.stageOrder : 0;
            }

            Target.Refresh(aimed);
        }

        // True when the trigger is in this run and not already earned.
        public bool Applies(string id)
        {
            return KarmaTriggers.FindInRun(id) != null && !KarmaTriggerState.IsEarned(id);
        }

        // Karma that firing this trigger would add: its reward times the run's Karma Modifier, capped below the maximum.
        public int AmountFor(string id)
        {
            KarmaTriggerDef def = KarmaTriggers.FindInRun(id);
            return def != null ? KarmaGate.AmountFor(def.baseKarmaReward, Karma) : 0;
        }

        // A warning for the triggers in ids that still apply, or false if none do. includeLoop adds the new-loop Karma when
        // the action also leaves the last stage of a loop.
        public bool TryMake(string text, int priority, string[] ids, bool includeLoop, out KarmaWarning warning)
        {
            int amount = 0;
            foreach (string id in ids)
            {
                if (Applies(id))
                {
                    amount += AmountFor(id);
                }
            }
            if (amount > 0 && includeLoop && LeavesLoopStage && Applies(KarmaTriggers.LoopAtLeastOnce))
            {
                amount += AmountFor(KarmaTriggers.LoopAtLeastOnce);
            }

            amount = Mathf.Min(amount, KarmaManager.maxKarma - Karma);
            warning = new KarmaWarning { Amount = amount, Text = text, Priority = priority };
            return amount > 0;
        }
    }
}
