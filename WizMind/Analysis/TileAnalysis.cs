using WizMind.Instances;
using WizMind.Interaction;
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

        /// <summary>
        /// Gets tiles surrounding the given tile in cardinal directions.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>An enumerable of surrounding map tiles.</returns>
        public IEnumerable<MapTile> GetSurroundingCardinalTiles(MapTile tile)
        {
            return this.GetSurroundingDirectionTiles(
                tile,
                [
                    MovementDirection.Down,
                    MovementDirection.Left,
                    MovementDirection.Right,
                    MovementDirection.Up,
                ]
            );
        }

        /// <summary>
        /// Gets tiles surrounding the given tile in the given directions.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>An enumerable of surrounding map tiles.</returns>
        public IEnumerable<MapTile> GetSurroundingDirectionTiles(
            MapTile tile,
            IEnumerable<MovementDirection> directions
        )
        {
            foreach (var direction in directions)
            {
                if (this.GetTileInDirection(tile, direction) is { } surroundingTile)
                {
                    yield return surroundingTile;
                }
            }
        }

        /// <summary>
        /// Gets tiles surrounding the given tile in ordinal directions.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>An enumerable of surrounding map tiles.</returns>
        public IEnumerable<MapTile> GetSurroundingOrdinalTiles(MapTile tile)
        {
            return this.GetSurroundingDirectionTiles(
                tile,
                [
                    MovementDirection.DownLeft,
                    MovementDirection.DownRight,
                    MovementDirection.UpRight,
                    MovementDirection.UpLeft,
                ]
            );
        }

        /// <summary>
        /// Gets tiles surrounding the given tile in all directions.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>An enumerable of surrounding map tiles.</returns>
        public IEnumerable<MapTile> GetSurroundingTiles(MapTile tile)
        {
            return this.GetSurroundingDirectionTiles(
                tile,
                [
                    MovementDirection.DownLeft,
                    MovementDirection.Down,
                    MovementDirection.DownRight,
                    MovementDirection.Right,
                    MovementDirection.UpRight,
                    MovementDirection.Up,
                    MovementDirection.UpLeft,
                    MovementDirection.Left,
                ]
            );
        }

        public MapTile? GetTileInDirection(MapTile tile, MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.DownLeft:
                    if (tile.X > 0 && tile.Y < this.luigiAiData.MapHeight - 1)
                    {
                        return this.luigiAiData.Tiles[tile.X - 1, tile.Y + 1];
                    }
                    break;

                case MovementDirection.Down:
                    if (tile.Y < this.luigiAiData.MapHeight - 1)
                    {
                        return this.luigiAiData.Tiles[tile.X, tile.Y + 1];
                    }
                    break;

                case MovementDirection.DownRight:
                    if (
                        tile.X < this.luigiAiData.MapWidth - 1
                        && tile.Y < this.luigiAiData.MapHeight - 1
                    )
                    {
                        return this.luigiAiData.Tiles[tile.X + 1, tile.Y + 1];
                    }
                    break;

                case MovementDirection.Right:
                    if (tile.X < this.luigiAiData.MapWidth - 1)
                    {
                        return this.luigiAiData.Tiles[tile.X + 1, tile.Y];
                    }
                    break;

                case MovementDirection.UpRight:
                    if (tile.X < this.luigiAiData.MapWidth - 1 && tile.Y > 0)
                    {
                        return this.luigiAiData.Tiles[tile.X + 1, tile.Y - 1];
                    }
                    break;

                case MovementDirection.Up:
                    if (tile.Y > 0)
                    {
                        return this.luigiAiData.Tiles[tile.X, tile.Y - 1];
                    }
                    break;

                case MovementDirection.UpLeft:
                    if (tile.X > 0 && tile.Y > 0)
                    {
                        return this.luigiAiData.Tiles[tile.X - 1, tile.Y - 1];
                    }
                    break;

                case MovementDirection.Left:
                    if (tile.X > 0)
                    {
                        return this.luigiAiData.Tiles[tile.X - 1, tile.Y];
                    }
                    break;
            }

            // If we didn't match a case above, the tile doesn't exist because
            // we are at the edge of the map
            return null;
        }

        /// <summary>
        /// Determines whether the tile is of the given type.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <param name="type">The tile type to check.</param>
        /// <returns><c>true</c> if the tile is of the given tile type, otherwise <c>false</c>.</returns>
        public bool IsTileType(MapTile tile, TileType type)
        {
            var filter = tileClassFilters[type];
            return filter(tile);
        }
    }
}
