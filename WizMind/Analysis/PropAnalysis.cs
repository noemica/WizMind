using WizMind.Definitions;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind.Analysis
{
    public enum PropType
    {
        Fabricator,
        DoorTerminal,
        GarrisonAccess,
        GarrisonTerminal,
        RecyclingUnit,
        RepairStation,
        RifInstaller,
        Scanalyzer,
        Terminal,
    }

    public class PropAnalysis(LuigiAiData luigiAiData)
    {
        private static readonly Dictionary<PropType, Func<Prop, bool>> propClassFilters = new()
        {
            { PropType.Fabricator, (prop) => prop.Name == "Fabricator" },
            { PropType.DoorTerminal, (prop) => prop.Name == "Door Terminal" },
            { PropType.GarrisonAccess, (prop) => prop.Name == "Garrison Access" },
            { PropType.GarrisonTerminal, (prop) => prop.Name == "Garrison Terminal" },
            { PropType.RecyclingUnit, (prop) => prop.Name == "Recycling Unit" },
            { PropType.RepairStation, (prop) => prop.Name == "Repair Station" },
            { PropType.RifInstaller, (prop) => prop.Name == "RIF Installer" },
            { PropType.Scanalyzer, (prop) => prop.Name == "Scanalyzer" },
            { PropType.Terminal, (prop) => prop.Name == "Terminal" },
        };

        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Calculates the names and frequencies of each prop type on the map.
        /// </summary>
        /// <remarks>
        /// Only works off of known tiles. If the whole map should be checked,
        /// call <see cref="WizardCommands.RevealMap(bool)"/> first.
        /// </remarks>
        /// <returns>A dictionary of prop names to their occurrences.</returns>
        public Dictionary<string, int> CalculatePropCounts()
        {
            var height = this.luigiAiData.MapHeight;
            var width = this.luigiAiData.MapWidth;

            var propCounts = new Dictionary<string, int>();
            var processed = new bool[width, height];

            // Process each tile
            foreach (var tile in this.luigiAiData.AllTiles)
            {
                // Skip if already processed
                if (processed[tile.X, tile.Y])
                {
                    continue;
                }

                processed[tile.X, tile.Y] = true;

                if (tile.Prop is { } prop)
                {
                    propCounts[prop.Definition.Name] =
                        propCounts.GetValueOrDefault(prop.Definition.Name) + 1;
                    CheckSurroundingTileForProp(processed, tile);
                }
            }

            return propCounts;

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
                        this.luigiAiData.Tiles[tile.X - 1, tile.Y],
                        propName
                    );
                }

                // Check right tile
                if (tile.X + 1 < this.luigiAiData.MapWidth && !processed[tile.X + 1, tile.Y])
                {
                    CheckTileForProp(
                        processed,
                        this.luigiAiData.Tiles[tile.X + 1, tile.Y],
                        propName
                    );
                }

                // Check up tile
                if (tile.Y > 0 && !processed[tile.X, tile.Y - 1])
                {
                    CheckTileForProp(
                        processed,
                        this.luigiAiData.Tiles[tile.X, tile.Y - 1],
                        propName
                    );
                }

                // Check down tile
                if (tile.Y + 1 < this.luigiAiData.MapHeight && !processed[tile.X, tile.Y + 1])
                {
                    CheckTileForProp(
                        processed,
                        this.luigiAiData.Tiles[tile.X, tile.Y + 1],
                        propName
                    );
                }
            }

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
        }

        /// <summary>
        /// Finds all tiles where there is a prop on the tile with the given name.
        /// </summary>
        /// <param name="name">The name of the prop to look for.</param>
        /// <param name="onlyInteractive">Whether to only show interactive prop.</param>
        /// <returns>A list of found tiles.</returns>
        public List<MapTile> FindTilesByPropName(string name, bool onlyInteractive = false)
        {
            return
            [
                .. this.luigiAiData.AllTiles.Where(tile =>
                    (tile.Prop?.Name) != name || !onlyInteractive || tile.Prop.InteractivePiece
                ),
            ];
        }

        /// <summary>
        /// Finds all tiles where the type of the tile matches the given class.
        /// </summary>
        /// <param name="type">The type or category of the tile.</param>
        /// <param name="onlyInteractive">Whether to only show interactive prop.</param>
        /// <returns>A list of found tiles.</returns>
        public List<List<MapTile>> FindTileGroupsByPropType(PropType type)
        {
            var groups = new List<List<MapTile>>();

            // Separate tiles into groups
            var tilesToProcess = this.FindTilesByPropType(type);
            while (tilesToProcess.Count > 0)
            {
                var tile = tilesToProcess[0];
                tilesToProcess.RemoveAt(0);

                // Skip tile if there's no prop
                if (tile.Prop == null)
                {
                    continue;
                }

                // Start with the first tile
                var group = new List<MapTile>() { tile };
                groups.Add(group);

                // Continue processing the list for new tiles each time a new
                // item is added. This is needed to ensure that the group is
                // fully connected and not based on tile processing order.
                var processGroup = true;
                while (processGroup)
                {
                    processGroup = false;

                    for (int i = tilesToProcess.Count - 1; i >= 0; i--)
                    {
                        var tileToCheck = tilesToProcess[i];
                        if (TryAddTileToGroup(group, tileToCheck))
                        {
                            tilesToProcess.RemoveAt(i);
                            processGroup = true;
                        }
                    }
                }
            }

            return groups;

            static bool TryAddTileToGroup(List<MapTile> group, MapTile tile)
            {
                foreach (var groupTile in group)
                {
                    // If a tile is adjacent to any tile in the group and has
                    // the same prop type then add it to the group
                    if (
                        (
                            (tile.X + 1 == groupTile.X && tile.Y == groupTile.Y)
                            || (tile.X - 1 == groupTile.X && tile.Y == groupTile.Y)
                            || (tile.X == groupTile.X && tile.Y + 1 == groupTile.Y)
                            || (tile.X == groupTile.X && tile.Y - 1 == groupTile.Y)
                        )
                        && tile.Prop?.Definition == groupTile.Prop?.Definition
                    )
                    {
                        group.Add(tile);
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Finds all tiles where the type of the tile matches the given class.
        /// </summary>
        /// <param name="type">The type or category of the tile.</param>
        /// <param name="onlyInteractive">Whether to only show interactive prop.</param>
        /// <returns>A list of found tiles.</returns>
        public List<MapTile> FindTilesByPropType(PropType type, bool onlyInteractive = false)
        {
            var filter = propClassFilters[type];
            var tiles = this
                .luigiAiData.AllTiles.Where(tile =>
                    tile.Prop != null
                    && (!onlyInteractive || tile.Prop.InteractivePiece)
                    && filter(tile.Prop)
                )
                .ToList();

            return tiles;
        }

        /// <summary>
        /// Determines whether the tile has the given prop type.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <param name="type">The prop type to check.</param>
        /// <returns><c>true</c> if the tile has a prop of the given type, otherwise <c>false</c>.</returns>
        public bool IsPropType(MapTile tile, PropType type)
        {
            var filter = propClassFilters[type];
            return tile.Prop != null && filter(tile.Prop);
        }
    }
}
