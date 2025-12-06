using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class GarrisonStatsScript : IScript
    {
        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws)
        {
            this.ws = ws;
        }

        public void Run()
        {
            const int NumDepths = 8;

            var allPropCounts = new Dictionary<PropDefinition, int>();
            var allPropCountsAverage = new Dictionary<PropDefinition, float>();
            var allTileCounts = new Dictionary<string, int>();
            var allTilesCountsAverage = new Dictionary<string, float>();
            var propCountsByDepth = new List<Dictionary<PropDefinition, int>>();
            var propCountsByDepthAverages = new List<Dictionary<PropDefinition, float>>();
            var tileCountsByDepth = new List<Dictionary<string, int>>();
            var tileCountsByDepthAverages = new List<Dictionary<string, float>>();
            var numRuns = 0;

            // Create lookup of definitions by depth
            for (var depth = NumDepths; depth >= 1; depth--)
            {
                propCountsByDepth.Add([]);
                propCountsByDepthAverages.Add([]);
                tileCountsByDepth.Add([]);
                tileCountsByDepthAverages.Add([]);
            }

            while (true)
            {
                for (var depth = NumDepths; depth >= 1; depth--)
                {
                    // Count the props and tiles at each depth
                    var (newPropCounts, newTileCounts) = this.ProcessDepth(depth);
                    var propCounts = propCountsByDepth[depth - 1];
                    var tileCounts = tileCountsByDepth[depth - 1];

                    // Combine the counts with the old dictionary
                    foreach (var (prop, newCount) in newPropCounts)
                    {
                        propCounts[prop] = propCounts.GetValueOrDefault(prop) + newCount;
                        allPropCounts[prop] = allPropCounts.GetValueOrDefault(prop) + newCount;
                    }

                    foreach (var (tile, newCount) in newTileCounts)
                    {
                        tileCounts[tile] = tileCounts.GetValueOrDefault(tile) + newCount;
                        allTileCounts[tile] = allTileCounts.GetValueOrDefault(tile) + newCount;
                    }
                }

                numRuns += 1;

                // Update average stats
                foreach (var (prop, count) in allPropCounts)
                {
                    allPropCountsAverage[prop] = (float)count / (numRuns * NumDepths);
                }

                foreach (
                    var (counts, averageCounts) in propCountsByDepth.Zip(propCountsByDepthAverages)
                )
                {
                    foreach (var (prop, count) in counts)
                    {
                        averageCounts[prop] = (float)count / numRuns;
                    }
                }

                foreach (var (tile, count) in allTileCounts)
                {
                    allTilesCountsAverage[tile] = (float)count / (numRuns * NumDepths);
                }

                foreach (
                    var (counts, averageCounts) in tileCountsByDepth.Zip(tileCountsByDepthAverages)
                )
                {
                    foreach (var (tile, count) in counts)
                    {
                        averageCounts[tile] = (float)count / numRuns;
                    }
                }

                this.ws.GameState.SelfDestruct();
            }
        }

        private (
            Dictionary<PropDefinition, int> PropCounts,
            Dictionary<string, int> TileCounts
        ) ProcessDepth(int depth)
        {
            this.ws.WizardCommands.GotoMap(MapType.MAP_GAR, depth);

            // For some reason, loop exits only appear after a turn has passed.
            // Wait one turn before revealing so we can see the exit.
            this.ws.Movement.Wait();

            this.ws.WizardCommands.RevealMap();

            return (
                this.ws.PropAnalysis.CalculatePropCounts(),
                this.ws.TileAnalysis.CalculateTileCounts()
            );
        }
    }
}
