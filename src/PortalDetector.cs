using System.Collections.Generic;
using RoR2;

namespace KarmaWarnings
{
    // Warns while the player aims at a portal that leads to a stage they can't leave without a Karma milestone, and at
    // any exit from the last stage of a loop.
    //
    // A portal is told apart by its context token, or by the stage it leads to when that is part of the prefab. Portals
    // whose destination the server sets at runtime (Soulbound Threshold, Bulwark's Ambry, Terminal Lucidity) can only be
    // matched by token. Run.nextStageScene is not synced to clients, so it is never read.
    internal sealed class PortalDetector : IWarningDetector
    {
        private sealed class Row
        {
            public readonly string Text;
            public readonly string[] Ids;

            public Row(string text, params string[] ids)
            {
                Text = text;
                Ids = ids;
            }
        }

        private static readonly Row Arena = new Row("Void Fields: starting and finishing the cells",
            KarmaTriggers.EnterArena, KarmaTriggers.CompletedArena);
        private static readonly Row GildedCoast = new Row("Gilded Coast: can't leave until Aurelionite dies",
            KarmaTriggers.DefeatAurelionite);
        private static readonly Row Ambry = new Row("Bulwark's Ambry: can't leave until the trial ends",
            KarmaTriggers.ArtifactWorldComplete);
        private static readonly Row Threshold = new Row("Soulbound Threshold: kill the Anima Harvester or die to leave",
            KarmaTriggers.BeatChampRushReaper);
        private static readonly Row Lucidity = new Row("Terminal Lucidity: killing the Echo of Providence",
            KarmaTriggers.KillProvidence);
        private static readonly Row NeuralSanctum = new Row("Neural Sanctum: killing the Solus Heart",
            KarmaTriggers.DefeatSolusHeart);
        private static readonly Row SolutionalHaunt = new Row("Solutional Haunt: killing the Solus Wing",
            KarmaTriggers.DefeatSolusWing);
        private static readonly Row Meridian = new Row("Prime Meridian: killing False Son",
            KarmaTriggers.DefeatFalseSon);

        private static readonly Dictionary<string, Row> ByToken = new Dictionary<string, Row>
        {
            { "PORTAL_ARENA_CONTEXT", Arena },
            { "PORTAL_GOLDSHORES_CONTEXT", GildedCoast },
            { "PORTAL_ARTIFACTWORLD_CONTEXT", Ambry },
            { "PORTAL_DIALER_LAPTOP_CONTEXT", Ambry },
            { "PORTAL_CHAMP_RUSH_CONTEXT", Threshold },
            { "PORTAL_ULTIMA_CONTEXT", Lucidity },
            { "PORTAL_SOLUSWEB_CONTEXT", NeuralSanctum }
        };

        private static readonly Dictionary<string, Row> ByDestination = new Dictionary<string, Row>
        {
            { "arena", Arena },
            { "goldshores", GildedCoast },
            { "artifactworld", Ambry },
            { "artifactworld01", Ambry },
            { "artifactworld02", Ambry },
            { "artifactworld03", Ambry },
            { "champrush", Threshold },
            { "ultima", Lucidity },
            { "solusweb", NeuralSanctum },
            { "solutionalhaunt", SolutionalHaunt },
            { "meridian", Meridian }
        };

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
            if (token == null || !ByToken.TryGetValue(token, out row))
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

        private static bool LoopWarning(WarningContext context, out KarmaWarning warning)
        {
            warning = default;
            if (!context.Applies(KarmaTriggers.LoopAtLeastOnce)) return false;

            warning = new KarmaWarning
            {
                Amount = context.AmountFor(KarmaTriggers.LoopAtLeastOnce),
                Text = "Starting a new loop",
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
