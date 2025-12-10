using WizMind.Instances;
using WizMind.LuigiAi;

namespace WizMind.Analysis
{
    public enum TileType
    {
        Door,
        Earth,
        EmergencyAccess,
        Floor,
        PhaseWall,
        Stairs,
        Wall,
    }

    public class TileAnalysis(LuigiAiData luigiAiData)
    {
        private static readonly Dictionary<TileType, Func<MapTile, bool>> tileClassFilters = new()
        {
            { TileType.Door, (tile) => tile.Name.StartsWith("DOOR_") },
            { TileType.Earth, (tile) => tile.Name.StartsWith("EARTH") },
            { TileType.EmergencyAccess, (tile) => tile.Name.StartsWith("SHORTCUT_") },
            { TileType.Floor, (tile) => tile.Name.StartsWith("FLOOR_") },
            { TileType.PhaseWall, (tile) => tile.Name.StartsWith("PHASEWEALL_") },
            { TileType.Stairs, (tile) => tile.Name.StartsWith("STAIRS_") },
            { TileType.Wall, (tile) => tile.Name.StartsWith("WALL_") },
        };

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

        /// <summary>
        /// Finds all tiles where the type of the tile matches the given class.
        /// </summary>
        /// <param name="type">The type or category of the tile.</param>
        /// <returns>A list of found tiles.</returns>
        public List<MapTile> FindTilesByType(TileType type)
        {
            var filter = tileClassFilters[type];
            return [.. this.luigiAiData.AllTiles.Where(tile => filter(tile))];
        }

        /// <summary>
        /// Finds all tiles where the tile name matches the given name.
        /// </summary>
        /// <param name="name">The name of the tile to find.</param>
        /// <returns>A list of found tiles.</returns>
        public List<MapTile> FindTilesByName(string name)
        {
            return [.. this.luigiAiData.AllTiles.Where(tile => tile.Name == name)];
        }
    }
}
