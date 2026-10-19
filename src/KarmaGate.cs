using RoR2;
using UnityEngine;

namespace KarmaWarnings
{
    // Decides whether a Karma warning can matter at all, and how much Karma an action would really add.
    //
    // Reads the synced fields on KarmaManager (NetworkactualKarma, NetworkisKarmaEnabled) instead of GetEffectiveKarma(),
    // which stays 0 on a client until the first Karma change or stage start. Never call KarmaManager.ModifyKarma or
    // Util.KarmaEventTrigger: they are server-side and change the run.
    internal static class KarmaGate
    {
        // False when there is no run, Karma is turned off for it, or Karma is already at the maximum.
        public static bool TryGetKarma(out int karma)
        {
            karma = 0;
            KarmaManager manager = KarmaManager.instance;
            if (!manager || !manager.NetworkisKarmaEnabled) return false;

            karma = (int)manager.NetworkactualKarma;
            return karma < KarmaManager.maxKarma;
        }

        // What the game would add for a trigger with this base reward: the reward times the run's Karma Modifier, capped
        // at what's left below the maximum.
        public static int AmountFor(int reward, int karma)
        {
            int multiplier = Run.instance ? Run.instance.ruleBook.karmaMultiplier : 1;
            return Mathf.Min(reward * multiplier, KarmaManager.maxKarma - karma);
        }
    }
}
