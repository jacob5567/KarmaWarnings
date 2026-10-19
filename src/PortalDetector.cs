using System.Collections.Generic;
using RoR2;

namespace KarmaWarnings
{
    // Warns while the player aims at a portal that leads to a stage they can't leave without a Karma milestone, and at
    // any exit from the last stage of a loop.
    //
    // The portal dialer on Sky Meadow and Helminth Hatchery (the Compound Generator and its laptop) gets no warning:
    // only the portal it opens is a commitment.
    // A portal is told apart by its context token, or by the stage it leads to when that is part of the prefab. Portals
    // whose destination the server sets at runtime (Soulbound Threshold, Bulwark's Ambry, Terminal Lucidity) can only be
    // matched by token. Run.nextStageScene is not synced to clients, so it is never read.
    internal sealed class PortalDetector : IWarningDetector
    {
        private sealed class Row
        {
            public readonly string Text;
            public readonly string[] Ids;
            // When set, a token match also needs the object's name to start with this. Some other objects reuse a portal's
            // prompt text: the DLC4 trial scenes' "RiftIdle" has the Ambry portal's.
            public string RequiredObjectName;
            // When set, and the portal's destination can be read, it has to be one of these stages. The server sets a
            // dialer portal's destination after spawning it, so a client usually sees the prefab's default instead, and
            // when nothing is set the object name above is all there is to go on.
            public string[] AllowedDestinations;

            public Row(string text, params string[] ids)
            {
                Text = text;
                Ids = ids;
            }
        }

        private static readonly Row Arena = new Row("when you start and finish the Void cells",
            KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena);
        private static readonly Row GildedCoast = new Row("when you kill Aurelionite",
            KarmaTriggers.DefeatAurelionite);
        // Bulwark's Ambry and its three Seekers of the Storm variants (Aphelian, Tar and Wisp Research).
        private static readonly string[] AmbryStages = { "artifactworld", "artifactworld01", "artifactworld02", "artifactworld03" };

        private static readonly Row Ambry = new Row("when you finish the Artifact Trial",
            KarmaTriggers.ArtifactWorldComplete)
        {
            RequiredObjectName = "PortalArtifactworld",
            AllowedDestinations = AmbryStages
        };
        private static readonly Row Threshold = new Row("when you kill the Anima Harvester",
            KarmaTriggers.BeatChampRushReaper);
        private static readonly Row Lucidity = new Row("when you kill the secret boss",
            KarmaTriggers.KillProvidence);
        private static readonly Row NeuralSanctum = new Row("when you kill the Solus Heart",
            KarmaTriggers.DefeatSolusHeart);
        private static readonly Row SolutionalHaunt = new Row("when you kill the Solus Wing",
            KarmaTriggers.DefeatSolusWing);
        private static readonly Row Meridian = new Row("when you kill False Son",
            KarmaTriggers.DefeatFalseSon);

        private static readonly Dictionary<string, Row> ByToken = new Dictionary<string, Row>
        {
            { "PORTAL_ARENA_CONTEXT", Arena },
            { "PORTAL_GOLDSHORES_CONTEXT", GildedCoast },
            { "PORTAL_ARTIFACTWORLD_CONTEXT", Ambry },
            { "PORTAL_CHAMP_RUSH_CONTEXT", Threshold },
            { "PORTAL_ULTIMA_CONTEXT", Lucidity },
            { "PORTAL_SOLUSWEB_CONTEXT", NeuralSanctum }
        };

        private static readonly Dictionary<string, Row> ByDestination = BuildByDestination();

        private static Dictionary<string, Row> BuildByDestination()
        {
            Dictionary<string, Row> byDestination = new Dictionary<string, Row>
            {
                { "arena", Arena },
                { "goldshores", GildedCoast },
                { "champrush", Threshold },
                { "ultima", Lucidity },
                { "solusweb", NeuralSanctum },
                { "solutionalhaunt", SolutionalHaunt },
                { "meridian", Meridian }
            };
            foreach (string stage in AmbryStages)
            {
                byDestination[stage] = Ambry;
            }
            return byDestination;
        }

        private static readonly string[] NoIds = new string[0];

        public bool TryGetWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            AimedTarget target = context.Target;
            if (!target.Root) return false;

            bool isExit = target.Exit || (target.Teleporter && target.Teleporter.isCharged);
            string destination = target.Exit ? DestinationName(context, target.Exit) : null;

            Row row = null;
            string token = target.ContextToken;
            if (token != null && ByToken.TryGetValue(token, out row) && !MatchesRow(target, row))
            {
                row = null;
            }
            if (row == null)
            {
                if (destination != null)
                {
                    ByDestination.TryGetValue(destination, out row);
                }
            }

            if (row != null && context.TryMake(row.Text, WarningPriority.Stage, row.Ids, isExit, out warning))
            {
                return true;
            }

            // Any other way out of the last stage of a loop starts the next loop.
            return isExit && context.LeavesLoopStage
                && LoopWarning(context, out warning);
        }

        // Whether a portal matched by its prompt text really is that portal: the right object, leading somewhere that fits.
        private static bool MatchesRow(AimedTarget target, Row row)
        {
            if (row.RequiredObjectName != null
                && !(target.Generic && target.Generic.name.StartsWith(row.RequiredObjectName)))
            {
                return false;
            }

            SceneDef destination = target.Exit ? target.Exit.destinationScene : null;
            return row.AllowedDestinations == null || !destination
                || System.Array.IndexOf(row.AllowedDestinations, destination.cachedName) >= 0;
        }

        private static bool LoopWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (!context.Applies(KarmaTriggers.LoopAtLeastOnce)) return false;

            warning = new KarmaWarning
            {
                Amount = context.AmountFor(KarmaTriggers.LoopAtLeastOnce),
                Text = "when you start a new loop",
                Priority = WarningPriority.Loop
            };
            return warning.Amount > 0;
        }

        // The stage a portal leads to, when the prefab says. A green portal picks the alternate for the next stage number.
        private static string DestinationName(WarningContext context, SceneExitController exit)
        {
            SceneDef scene = exit.destinationScene;
            if (exit.isAlternatePath)
            {
                switch (context.StageOrder + 1)
                {
                    case 1: scene = exit.tier1AlternateDestinationScene; break;
                    case 2: scene = exit.tier2AlternateDestinationScene; break;
                    case 3: scene = exit.tier3AlternateDestinationScene; break;
                    case 4: scene = exit.tier4AlternateDestinationScene; break;
                    case 5: scene = exit.tier5AlternateDestinationScene; break;
                }
            }
            return scene ? scene.cachedName : null;
        }
    }
}
