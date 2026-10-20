using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace KarmaWarnings
{
    // One per HUD. Each frame it checks that a warning can matter (the mod is on, the local player has a body, Karma is
    // on for the run and below its maximum), asks each detector for a warning, and shows the most important one on the
    // banner.
    //
    // Read-only: see KarmaGate for the game methods that must never be called.
    internal sealed class KarmaWarningsController : MonoBehaviour
    {
        private static readonly List<KarmaWarningsController> instances = new List<KarmaWarningsController>();

        private readonly WarningContext context = new WarningContext();
        private readonly PortalDetector portals = new PortalDetector();
        private readonly CombatDetector combat = new CombatDetector();
        private IWarningDetector[] detectors;

        private HUD hud;
        private WarningBanner banner;
        private CharacterBody driverBody;
        private InteractionDriver driver;
        private bool showing;
        private KarmaWarning current;

        public static IReadOnlyList<KarmaWarningsController> Instances => instances;

        private void Awake()
        {
            hud = GetComponent<HUD>();
            detectors = new IWarningDetector[] { portals, new InteractableDetector(), combat, new ItemDetector() };
        }

        private void OnEnable()
        {
            instances.Add(this);
        }

        private void OnDisable()
        {
            instances.Remove(this);
        }

        private void Update()
        {
            if (!banner)
            {
                if (!hud.mainContainer) return;
                banner = WarningBanner.Create(hud);
            }

            showing = TryGetWarning(out current);
            if (showing)
            {
                banner.Show(current);
            }
            else
            {
                banner.Hide();
            }
        }

        private bool TryGetWarning(out KarmaWarning warning)
        {
            warning = default;
            PluginConfig settings = KarmaWarningsPlugin.Settings;
            if (!settings.Enabled.Value) return false;

            if (TryFindDetectorWarning(out warning)) return true;

            if (settings.DebugAlwaysShow.Value)
            {
                // The lowest priority, so it never hides a real warning. It skips the Karma gates, so it shows with Karma off
                // or at the maximum.
                warning = new KarmaWarning { Amount = 1, Text = "when DebugAlwaysShow is on", Priority = WarningPriority.Debug };
                return true;
            }
            return false;
        }

        private bool TryFindDetectorWarning(out KarmaWarning warning)
        {
            warning = default;

            // The local player's own body, not whoever the HUD is spectating: only your own actions matter.
            CharacterBody body = hud.localUserViewer?.cachedBody;
            if (!body || !KarmaGate.TryGetKarma(out int karma)) return false;

            // Reads the target the game is already outlining. Never call InteractionDriver.FindBestInteractableObject: it
            // resets the driver's check cooldown.
            if (driverBody != body)
            {
                driverBody = body;
                driver = body.GetComponent<InteractionDriver>();
            }
            context.Update(body, karma, driver ? driver.currentInteractable : null);

            bool found = false;
            foreach (IWarningDetector detector in detectors)
            {
                if (detector.TryGetWarning(context, out KarmaWarning candidate) && (!found || candidate.Priority > warning.Priority))
                {
                    warning = candidate;
                    found = true;
                }
            }
            return found;
        }

        // The F8 dump: what the banner shows, what the player is aiming at, and what the fight checks see. The context is
        // what the last frame with a living body saw.
        public string Describe()
        {
            CharacterBody body = hud.localUserViewer?.cachedBody;
            string warning = showing ? $"showing \"{current.Text}\" (+{current.Amount})" : "nothing to show";
            string aimed = context.Target.Root ? context.Target.Root.name : "nothing";
            return $"HUD of {(body ? body.GetDisplayName() : "no body")}: {warning}; stage {context.SceneName} (order {context.StageOrder}), aiming at {aimed}"
                + "\n    exit: " + portals.Describe(context)
                + "\n    fights: " + combat.Describe(context);
        }
    }
}
