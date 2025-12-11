using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class GarrisonContentsScript : IScript
    {
        private const int NumDepths = 8;

        private State state = null!;
        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws, object? state)
        {
            this.ws = ws;
            this.state = state as State ?? new State();

            this.state.Initialize();
        }

        public Type SerializableStateType => typeof(State);

        public IScriptState SerializableState => this.state;

        public bool ProcessRun()
        {
            for (var depth = NumDepths; depth >= 1; depth--)
            {
                // Count the items, props, and tiles at each depth
                var (newItemCounts, newPropCounts, newTileCounts) = this.ProcessDepth(depth);

                // Combine the counts with the old dictionary
                UpdateDepthStats(
                    this.state.AllItemCounts,
                    newItemCounts,
                    this.state.ItemCountsByDepth[depth - 1]
                );
                UpdateDepthStats(
                    this.state.AllPropCounts,
                    newPropCounts,
                    this.state.PropCountsByDepth[depth - 1]
                );
                UpdateDepthStats(
                    this.state.AllTileCounts,
                    newTileCounts,
                    this.state.TileCountsByDepth[depth - 1]
                );
            }

            // Update average stats
            UpdateAverageStats(
                this.state.AllItemCounts,
                this.state.AllItemsCountsAverage,
                this.state.ItemCountsByDepth,
                this.state.ItemCountsByDepthAverages,
                this.state.NumRuns
            );

            UpdateAverageStats(
                this.state.AllPropCounts,
                this.state.AllPropCountsAverage,
                this.state.PropCountsByDepth,
                this.state.PropCountsByDepthAverages,
                this.state.NumRuns
            );

            UpdateAverageStats(
                this.state.AllTileCounts,
                this.state.AllTilesCountsAverage,
                this.state.TileCountsByDepth,
                this.state.TileCountsByDepthAverages,
                this.state.NumRuns
            );

            return true;
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
            Dictionary<string, int> PropCounts,
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

        private class State : IScriptState
        {
            public Dictionary<string, int> AllItemCounts { get; set; } = [];

            public Dictionary<string, float> AllItemsCountsAverage { get; set; } = [];

            public Dictionary<string, int> AllPropCounts { get; set; } = [];

            public Dictionary<string, float> AllPropCountsAverage { get; set; } = [];

            public Dictionary<string, int> AllTileCounts { get; set; } = [];

            public Dictionary<string, float> AllTilesCountsAverage { get; set; } = [];

            public List<Dictionary<string, int>> ItemCountsByDepth { get; set; } = [];

            public bool Initialized { get; set; }

            public List<Dictionary<string, float>> ItemCountsByDepthAverages { get; set; } = [];

            public int NumRuns { get; set; }

            public List<Dictionary<string, int>> PropCountsByDepth { get; set; } = [];

            public List<Dictionary<string, float>> PropCountsByDepthAverages { get; set; } = [];

            public List<Dictionary<string, int>> TileCountsByDepth { get; set; } = [];

            public List<Dictionary<string, float>> TileCountsByDepthAverages { get; } = [];

            public void Initialize()
            {
                if (this.Initialized)
                {
                    return;
                }

                // Create lookup of definitions by depth
                for (var depth = NumDepths; depth >= 1; depth--)
                {
                    this.ItemCountsByDepth.Add([]);
                    this.ItemCountsByDepthAverages.Add([]);
                    this.PropCountsByDepth.Add([]);
                    this.PropCountsByDepthAverages.Add([]);
                    this.TileCountsByDepth.Add([]);
                    this.TileCountsByDepthAverages.Add([]);
                }

                this.Initialized = true;
            }
        }
    }
}
