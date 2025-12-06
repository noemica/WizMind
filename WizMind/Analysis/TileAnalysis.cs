using System;
using System.Collections.Generic;
using System.Text;
using WizMind.Definitions;
using WizMind.Instances;
using WizMind.LuigiAi;

namespace WizMind.Analysis
{
    public class TileAnalysis(LuigiAiData luigiAiData)
    {
        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Calculates the names and frequencies of each tile on the map.
        /// </summary>
        /// <remarks>
        /// Only works off of known tiles. If the whole map should be checked,
        /// call <see cref="WizardCommands.RevealMap(bool)"/> first.
        /// </remarks>
        /// <returns>A dictionary of tile names to their occurrences.</returns>
        public Dictionary<string, int> CalculateTileCounts()
        {
            var tileCounts = new Dictionary<string, int>();

            // Process each tile
            foreach (var tile in this.luigiAiData.AllTiles)
            {
                tileCounts[tile.Name] = tileCounts.GetValueOrDefault(tile.Name) + 1;
            }

            return tileCounts;
        }
    }
}
