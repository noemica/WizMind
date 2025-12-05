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
            var allCounts = new Dictionary<PropDefinition, int>();
            var allCountsAverages = new Dictionary<PropDefinition, float>();
            var countsByDepth = new List<Dictionary<PropDefinition, int>>();
            var countsByDepthAverages = new List<Dictionary<PropDefinition, float>>();
            var numRuns = 0;

            // Create lookup of definitions by depth
            for (var depth = 8; depth >= 1; depth--)
            {
                countsByDepth.Add([]);
                countsByDepthAverages.Add([]);
            }

            while (true)
            {
                for (var depth = 8; depth >= 1; depth--)
                {
                    // Count the props at each depth
                    var newPropCounts = this.ProcessDepth(depth);
                    var propCounts = countsByDepth[depth - 1];

                    // Combine the counts with the old dictionary
                    foreach (var (prop, newCount) in newPropCounts)
                    {
                        propCounts[prop] = propCounts.GetValueOrDefault(prop) + newCount;
                        allCounts[prop] = allCounts.GetValueOrDefault(prop) + newCount;
                    }
                }

                numRuns += 1;

                foreach (var (prop, count) in allCounts)
                {
                    allCountsAverages[prop] = (float)count / numRuns;
                }

                foreach (var (counts, averageCounts) in countsByDepth.Zip(countsByDepthAverages))
                {
                    foreach (var (prop, count) in averageCounts)
                    {
                        averageCounts[prop] = (float)count / numRuns;
                    }
                }

                this.ws.GameState.SelfDestruct();
            }
        }

        private Dictionary<PropDefinition, int> ProcessDepth(int depth)
        {
            this.ws.WizardCommands.GotoMap(MapType.MAP_GAR, depth);
            this.ws.WizardCommands.RevealMap();

            return this.ws.PropAnalysis.CalculatePropCounts();
        }
    }
}
