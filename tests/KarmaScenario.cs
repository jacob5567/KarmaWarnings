using System;
using System.Collections;
using ModTestRunner;

namespace KarmaWarnings.Tests
{
    // A KarmaWarnings scenario: a Scenario plus the Karma triggers it needs unearned. Phases: 1 gates, portals, items;
    // 2 teleporter, shrines; 3 combat; 4 fixed places.
    internal sealed class KarmaScenario : Scenario
    {
        // Triggers the scenario needs unearned. It is skipped when one isn't in the run or is already earned.
        public readonly string[] Triggers;

        // A scenario that earns a trigger by doing it runs last, after everything that needs that trigger unearned.
        public KarmaScenario(int phase, string suite, string name, string scene, string[] triggers, Func<Harness, IEnumerator> run,
            bool earns = false)
            : base(phase, suite, name, scene, run, runsLast: earns)
        {
            Triggers = triggers ?? new string[0];
        }
    }
}
