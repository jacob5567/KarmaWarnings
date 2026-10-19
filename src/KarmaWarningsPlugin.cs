using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;

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

            KarmaTriggerState.Install();
            HUD.onHudTargetChangedGlobal += OnHudTargetChanged;
        }

        private void OnDestroy()
        {
            KarmaTriggerState.Uninstall();
            HUD.onHudTargetChangedGlobal -= OnHudTargetChanged;
        }

        // Fires whenever any HUD changes the body it is showing, so each HUD gets exactly one controller.
        private static void OnHudTargetChanged(HUD hud)
        {
            if (hud && !hud.GetComponent<KarmaWarningsController>())
            {
                hud.gameObject.AddComponent<KarmaWarningsController>();
            }
        }

        private void Update()
        {
            if (Settings.DebugLogging.Value && Settings.DumpKey.Value.IsDown())
            {
                DumpKarma();
            }
        }

        // Logs the synced Karma state and what each HUD's controller is doing. See KarmaGate for why it reads the synced
        // fields.
        private static void DumpKarma()
        {
            KarmaManager manager = KarmaManager.instance;
            if (!manager)
            {
                Log.LogInfo("Karma dump: no KarmaManager (not in a run)");
                return;
            }

            int multiplier = Run.instance ? Run.instance.ruleBook.karmaMultiplier : 1;
            Log.LogInfo($"Karma dump: enabled={manager.NetworkisKarmaEnabled} karma={manager.NetworkactualKarma}/{KarmaManager.maxKarma} multiplier=x{multiplier} canWarn={KarmaGate.TryGetKarma(out _)}");
            Log.LogInfo("  triggers in this run: " + KarmaTriggerState.Describe());
            foreach (KarmaWarningsController controller in KarmaWarningsController.Instances)
            {
                Log.LogInfo("  " + controller.Describe());
            }
        }
    }
}
