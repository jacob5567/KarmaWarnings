using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModTestRunner;
using RoR2;

namespace KarmaWarnings.Tests
{
    // The KarmaWarnings scenarios, and what the runner needs to know about the mod: when they can run, when a stage is
    // ready, what to put back, and what the mod saw at a failure.
    internal sealed class KarmaWarningsTests : ModTests
    {
        public override string Name => KarmaWarningsPlugin.PluginName;

        public override IEnumerable<Scenario> Scenarios()
        {
            return KarmaGateScenarios.All()
                .Concat(PortalScenarios.All())
                .Concat(ItemScenarios.All())
                .Concat(TeleporterScenarios.All())
                .Concat(ShrineScenarios.All())
                .Concat(CombatScenarios.All())
                .Concat(FixedPlaceScenarios.All())
                .Concat(MithrixPreviewScenarios.All())
                .Concat(EarningScenarios.All());
        }

        public override string Precondition(Scenario scenario)
        {
            if (!KarmaManager.instance) return "no Karma manager";

            // TryGetKarma is also false at the maximum, which has the Karma it read in the out value.
            if (!KarmaGate.TryGetKarma(out int karma) && karma < KarmaManager.maxKarma) return "Karma is off for this run";

            PluginConfig settings = KarmaWarningsPlugin.Settings;
            if (!settings.Enabled.Value) return "Enabled is off in the KarmaWarnings config";
            if (settings.DebugAlwaysShow.Value) return "DebugAlwaysShow is on in the KarmaWarnings config";

            if (scenario is KarmaScenario karmaScenario)
            {
                foreach (string id in karmaScenario.Triggers)
                {
                    if (KarmaTriggers.FindInRun(id) == null) return $"{id} is not in this run";
                    if (KarmaTriggerState.IsEarned(id)) return $"{id} is already earned";
                }
            }
            return null;
        }

        // The banner is read through the HUD controller, which attaches a little after the stage loads.
        public override bool Ready()
        {
            return KarmaWarningsChecks.Controller != null;
        }

        public override IEnumerator Cleanup(Harness h)
        {
            h.Cmd($"{Commands.KarmaCommand} 0");
            yield break;
        }

        // The mod's F8 line for this HUD, which names the aimed-at object, the exit and the fight checks.
        public override string Diagnose()
        {
            KarmaWarningsController controller = KarmaWarningsChecks.Controller;
            return controller != null ? controller.Describe() : null;
        }
    }
}
