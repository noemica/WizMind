using WizMind.LuigiAi;

namespace WizMind.Analysis
{
    public class ItemAnalysis(LuigiAiData luigiAiData)
    {
        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Calculates the names and frequencies of each item on the map.
        /// </summary>
        /// <remarks>
        /// Only works off of known tiles. If the whole map should be checked,
        /// call <see cref="WizardCommands.RevealMap(bool)"/> first.
        /// </remarks>
        /// <returns>A dictionary of item names to their occurrences.</returns>
        public Dictionary<string, int> CalculateItemCounts()
        {
            var itemCounts = new Dictionary<string, int>();

            // Process each tile
            foreach (var tile in this.luigiAiData.AllTiles)
            {
                if (tile.Item is not null)
                {
                    itemCounts[tile.Item.Name] = itemCounts.GetValueOrDefault(tile.Item.Name) + 1;
                }
            }

            return itemCounts;
        }
    }
}
