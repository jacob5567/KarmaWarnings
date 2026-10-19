using BepInEx;
using BepInEx.Logging;
using RoR2;

namespace KarmaWarnings
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RiskOfOptionsCompat.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class KarmaWarningsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "sevenstack.KarmaWarnings";
        public const string PluginName = "KarmaWarnings";
        public const string PluginVersion = PluginVersionInfo.Version;

        internal static ManualLogSource Log;
        internal static PluginConfig Settings;

        private void Awake()
        {
            Log = Logger;
            Settings = new PluginConfig(Config);

            if (RiskOfOptionsCompat.Enabled)
            {
                RiskOfOptionsCompat.Register(Settings);
            }
        }

        private void Update()
        {
            if (Settings.DebugLogging.Value && Settings.DumpKey.Value.IsDown())
            {
                DumpKarma();
            }
        }

        // Reads the synced fields rather than GetEffectiveKarma(), which stays 0 on a client until the first Karma
        // change or stage start. Never call KarmaManager.ModifyKarma or Util.KarmaEventTrigger: they are server-side
        // and change the run.
        private static void DumpKarma()
        {
            KarmaManager manager = KarmaManager.instance;
            if (!manager)
            {
                Log.LogInfo("Karma dump: no KarmaManager (not in a run)");
                return;
            }

            Log.LogInfo($"Karma dump: enabled={manager.NetworkisKarmaEnabled} karma={manager.NetworkactualKarma}/{KarmaManager.maxKarma}");
        }
    }
}
