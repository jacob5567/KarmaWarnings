using BepInEx;
using ModTestRunner;

namespace KarmaWarnings.Tests
{
    // Registers the KarmaWarnings in-game tests with ModTestRunner, which runs them (mod_test --mod KarmaWarnings).
    // Dev-only: it is never packaged or published.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(KarmaWarningsPlugin.PluginGUID)]
    [BepInDependency(ModTestRunnerPlugin.PluginGUID)]
    public class KarmaWarningsTestsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "sevenstack.KarmaWarnings.Tests";
        public const string PluginName = "KarmaWarnings.Tests";
        public const string PluginVersion = "0.0.1";

        private void Awake()
        {
            TestRegistry.Register(new KarmaWarningsTests());
        }
    }
}
