using WizMind.Definitions;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind.Analysis
{
    public class PropAnalysis
    {
        private readonly CogmindProcess cogmindProcess;

        public PropAnalysis(CogmindProcess cogmindProcess)
        {
            this.cogmindProcess = cogmindProcess;
        }

        private LuigiAiData LuigiAiData => this.cogmindProcess.LuigiAiData;

        /// <summary>
        /// Calculates the names and frequencies of each prop type on the map.
        /// </summary>
        /// <remarks>
        /// Only works off of known tiles. If the whole map should be checked,
        /// call <see cref="WizardCommands.RevealMap(bool)"/> first.
        /// </remarks>
        /// <returns>A dictionary of prop names to their occurrences.</returns>
        public Dictionary<PropDefinition, int> CalculatePropCounts()
        {
            var propCounts = new Dictionary<PropDefinition, int>();
            var visited = new List<List<bool>>();

            var height = this.LuigiAiData.MapHeight;
            var width = this.LuigiAiData.MapWidth;

            // Create list of processed elements
            var processed = new bool[width, height];

            // Process each tile
            foreach (var tile in this.LuigiAiData.AllTiles)
            {
                // Skip if already processed
                if (processed[tile.X, tile.Y])
                {
                    continue;
                }

                processed[tile.X, tile.Y] = true;

                if (tile.Prop is { } prop)
                {
                    propCounts[prop.Definition] = propCounts.GetValueOrDefault(prop.Definition) + 1;
                    CheckSurroundingTileForProp(processed, tile);
                }
            }

            return propCounts;

            void CheckTileForProp(bool[,] processed, MapTile tile, string propName)
            {
                // Recursively check if the tile has the same prop type as the
                // given name.
                if (tile.Prop is { } prop && prop.Name == propName)
                {
                    processed[tile.X, tile.Y] = true;
                    CheckSurroundingTileForProp(processed, tile);
                }
            }

            void CheckSurroundingTileForProp(bool[,] processed, MapTile tile)
            {
                // Recursively check to see if unprocessed tiles around the
                // given tile match the prop type. If so, then mark them as
                // processed so we don't count them multiple times
                var propName = tile.Prop!.Name;

                // Check left tile
                if (tile.X > 0 && !processed[tile.X - 1, tile.Y])
                {
                    CheckTileForProp(
                        processed,
                        this.LuigiAiData.GetTile(tile.X - 1, tile.Y),
                        propName
                    );
                }

                // Check right tile
                if (tile.X + 1 < this.LuigiAiData.MapWidth && !processed[tile.X + 1, tile.Y])
                {
                    CheckTileForProp(
                        processed,
                        this.LuigiAiData.GetTile(tile.X + 1, tile.Y),
                        propName
                    );
                }

                // Check up tile
                if (tile.Y > 0 && !processed[tile.X, tile.Y - 1])
                {
                    CheckTileForProp(
                        processed,
                        this.LuigiAiData.GetTile(tile.X, tile.Y - 1),
                        propName
                    );
                }

                // Check down tile
                if (tile.Y + 1 < this.LuigiAiData.MapHeight && !processed[tile.X, tile.Y + 1])
                {
                    CheckTileForProp(
                        processed,
                        this.LuigiAiData.GetTile(tile.X, tile.Y + 1),
                        propName
                    );
                }
            }
        }
    }
}
