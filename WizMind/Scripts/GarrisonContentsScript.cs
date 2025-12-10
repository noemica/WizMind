using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class GarrisonContentsScript : IScript
    {
        private const int NumDepths = 8;

        private readonly Dictionary<string, int> allItemCounts = [];
        private readonly Dictionary<string, float> allItemsCountsAverage = [];
        private readonly List<Dictionary<string, int>> itemCountsByDepth = [];
        private readonly List<Dictionary<string, float>> itemCountsByDepthAverages = [];

        private readonly Dictionary<PropDefinition, int> allPropCounts = [];
        private readonly Dictionary<PropDefinition, float> allPropCountsAverage = [];
        private readonly List<Dictionary<PropDefinition, int>> propCountsByDepth = [];
        private readonly List<Dictionary<PropDefinition, float>> propCountsByDepthAverages = [];

        private readonly Dictionary<string, int> allTileCounts = [];
        private readonly Dictionary<string, float> allTilesCountsAverage = [];
        private readonly List<Dictionary<string, int>> tileCountsByDepth = [];
        private readonly List<Dictionary<string, float>> tileCountsByDepthAverages = [];

        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws)
        {
            this.ws = ws;
        }

        public void Run()
        {
            var numRuns = 0;

            // Create lookup of definitions by depth
            for (var depth = NumDepths; depth >= 1; depth--)
            {
                this.itemCountsByDepth.Add([]);
                this.itemCountsByDepthAverages.Add([]);
                this.propCountsByDepth.Add([]);
                this.propCountsByDepthAverages.Add([]);
                this.tileCountsByDepth.Add([]);
                this.tileCountsByDepthAverages.Add([]);
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
                            this.allItemCounts,
                            newItemCounts,
                            this.itemCountsByDepth[depth - 1]
                        );
                        UpdateDepthStats(
                            this.allPropCounts,
                            newPropCounts,
                            this.propCountsByDepth[depth - 1]
                        );
                        UpdateDepthStats(
                            this.allTileCounts,
                            newTileCounts,
                            this.tileCountsByDepth[depth - 1]
                        );
                    }

                    numRuns += 1;

                    // Update average stats
                    UpdateAverageStats(
                        this.allItemCounts,
                        this.allItemsCountsAverage,
                        this.itemCountsByDepth,
                        this.itemCountsByDepthAverages,
                        numRuns
                    );

                    UpdateAverageStats(
                        this.allPropCounts,
                        this.allPropCountsAverage,
                        this.propCountsByDepth,
                        this.propCountsByDepthAverages,
                        numRuns
                    );

                    UpdateAverageStats(
                        this.allTileCounts,
                        this.allTilesCountsAverage,
                        this.tileCountsByDepth,
                        this.tileCountsByDepthAverages,
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
