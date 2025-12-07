using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class GarrisonContentsScript : IScript
    {
        private const int NumDepths = 8;

        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws)
        {
            this.ws = ws;
        }

        public void Run()
        {
            var allItemCounts = new Dictionary<string, int>();
            var allItemsCountsAverage = new Dictionary<string, float>();
            var itemCountsByDepth = new List<Dictionary<string, int>>();
            var itemCountsByDepthAverages = new List<Dictionary<string, float>>();

            var allPropCounts = new Dictionary<PropDefinition, int>();
            var allPropCountsAverage = new Dictionary<PropDefinition, float>();
            var propCountsByDepth = new List<Dictionary<PropDefinition, int>>();
            var propCountsByDepthAverages = new List<Dictionary<PropDefinition, float>>();

            var allTileCounts = new Dictionary<string, int>();
            var allTilesCountsAverage = new Dictionary<string, float>();
            var tileCountsByDepth = new List<Dictionary<string, int>>();
            var tileCountsByDepthAverages = new List<Dictionary<string, float>>();

            var numRuns = 0;

            // Create lookup of definitions by depth
            for (var depth = NumDepths; depth >= 1; depth--)
            {
                itemCountsByDepth.Add([]);
                itemCountsByDepthAverages.Add([]);
                propCountsByDepth.Add([]);
                propCountsByDepthAverages.Add([]);
                tileCountsByDepth.Add([]);
                tileCountsByDepthAverages.Add([]);
            }

            while (true)
            {
                try
                {
                    for (var depth = NumDepths; depth >= 1; depth--)
                    {
                        // Count the items, props, and tiles at each depth
                        var (newItemCounts, newPropCounts, newTileCounts) = this.ProcessDepth(
                            depth
                        );

                        // Combine the counts with the old dictionary
                        UpdateDepthStats(
                            allItemCounts,
                            newItemCounts,
                            itemCountsByDepth[depth - 1]
                        );
                        UpdateDepthStats(
                            allPropCounts,
                            newPropCounts,
                            propCountsByDepth[depth - 1]
                        );
                        UpdateDepthStats(
                            allTileCounts,
                            newTileCounts,
                            tileCountsByDepth[depth - 1]
                        );
                    }

                    numRuns += 1;

                    // Update average stats
                    UpdateAverageStats(
                        allItemCounts,
                        allItemsCountsAverage,
                        itemCountsByDepth,
                        itemCountsByDepthAverages,
                        numRuns
                    );

                    UpdateAverageStats(
                        allPropCounts,
                        allPropCountsAverage,
                        propCountsByDepth,
                        propCountsByDepthAverages,
                        numRuns
                    );

                    UpdateAverageStats(
                        allTileCounts,
                        allTilesCountsAverage,
                        tileCountsByDepth,
                        tileCountsByDepthAverages,
                        numRuns
                    );

                    // Start a new run
                    this.ws.GameState.SelfDestruct();
                }
                catch (Exception ex)
                {
                    // If we run into an exception, end the run and try again
                    Console.WriteLine(ex.Message);
                    this.ws.GameState.SelfDestruct();
                }
            }
        }

        private static void UpdateDepthStats<TKey>(
            Dictionary<TKey, int> allCounts,
            Dictionary<TKey, int> newCounts,
            Dictionary<TKey, int> depthCounts
        )
            where TKey : notnull
        {
            foreach (var (key, newCount) in newCounts)
            {
                depthCounts[key] = depthCounts.GetValueOrDefault(key) + newCount;
                allCounts[key] = allCounts.GetValueOrDefault(key) + newCount;
            }
        }

        private static void UpdateAverageStats<TKey>(
            Dictionary<TKey, int> counts,
            Dictionary<TKey, float> countsAverage,
            List<Dictionary<TKey, int>> countsByDepth,
            List<Dictionary<TKey, float>> countsByDepthAverage,
            int numRuns
        )
            where TKey : notnull
        {
            foreach (var (key, count) in counts)
            {
                countsAverage[key] = (float)count / (numRuns * NumDepths);
            }

            foreach (var (countDict, averageCounts) in countsByDepth.Zip(countsByDepthAverage))
            {
                foreach (var (key, count) in countDict)
                {
                    averageCounts[key] = (float)count / numRuns;
                }
            }
        }

        private (
            Dictionary<string, int> ItemCounts,
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
                this.ws.ItemAnalysis.CalculateItemCounts(),
                this.ws.PropAnalysis.CalculatePropCounts(),
                this.ws.TileAnalysis.CalculateTileCounts()
            );
        }
    }
}
