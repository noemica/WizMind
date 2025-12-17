using WizMind.Analysis;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;
using WizMind.Utilities;

namespace WizMind.Scripts
{
    public class GarrisonLoopScript : IScript
    {
        private State state = null!;
        private ScriptWorkspace ws = null!;

        private const int NumDepths = 8;

        public Type SerializableStateType => typeof(State);

        public IScriptState SerializableState => this.state;

        public void Initialize(ScriptWorkspace ws, object? state)
        {
            this.ws = ws;

            this.state = state as State ?? new State();
            this.state.Initialize();
        }

        public bool ProcessRun()
        {
            // Add maximum number of slots ahead of time so we don't
            // have to pick them on evolutions
            this.ws.WizardCommands.AddSlots(SlotType.Utility, 19);

            // Add for 100% hacking success
            this.ws.WizardCommands.AttachItem("Architect God Chip A");

            // Discover Storage and Extension depths
            this.ws.GameState.SaveGame();
            this.ws.WizardCommands.GotoMap(MapType.MAP_STO);
            var storageDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_EXT);
            var extensionDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_ARM);
            var armoryDepth = this.ws.LuigiAiData.Depth;

            // Need to load the game or else the game does funky things with
            // evolved slots disappearing which causes the evolution screen
            // to show up
            this.ws.GameState.LoadGame();

            // Create list of depths to visit
            var depthsToVisit = new List<int>();
            if (storageDepth != 9)
            {
                depthsToVisit.Add(storageDepth);
            }

            depthsToVisit.Add(extensionDepth);

            if (armoryDepth != extensionDepth && armoryDepth == 4)
            {
                depthsToVisit.Add(armoryDepth);
            }

            depthsToVisit.Add(3);
            depthsToVisit.Add(2);

            // Go to the depth's Garrison and take a normal exit
            foreach (var depth in depthsToVisit)
            {
                this.ws.WizardCommands.GotoMap(this.ws.Definitions.MainMaps[depth].type, depth);

                var loop = 0;
                while (true)
                {
                    this.FindAndEnterGarrison();
                    this.TakeExit();

                    this.state.LoopAttemptsPerDepth[depth][loop] =
                        this.state.LoopAttemptsPerDepth[depth].GetValueOrDefault(loop) + 1;

                    // Check whether we looped
                    if (this.ws.LuigiAiData.Depth == depth)
                    {
                        // Save loop
                        this.state.LoopsPerDepth[depth][loop] =
                            this.state.LoopsPerDepth[depth].GetValueOrDefault(loop) + 1;

                        // Save map we looped into
                        if (!this.state.LoopedMapPerDepth[depth].TryGetValue(loop, out var maps))
                        {
                            maps = [];
                            this.state.LoopedMapPerDepth[depth][loop] = maps;
                        }
                        maps[this.ws.LuigiAiData.MapType] =
                            maps.GetValueOrDefault(this.ws.LuigiAiData.MapType) + 1;

                        if (this.ws.LuigiAiData.MapType != this.ws.Definitions.MainMaps[depth].type)
                        {
                            // Looped into a branch, exit now
                            this.state.BranchLoopsPerDepth[depth][loop] =
                                this.state.BranchLoopsPerDepth[depth].GetValueOrDefault(loop) + 1;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    loop += 1;
                }
            }

            return true;
        }

        private void FindAndEnterGarrison()
        {
            this.ws.WizardCommands.RevealMap();
            var cogmindPosition = this.ws.LuigiAiData.CogmindCoordinates;

            // Find the closest interactive Garrison tile
            var garrisonTile = this
                .ws.PropAnalysis.FindTilesByPropType(PropType.GarrisonAccess, true)
                .OrderBy(tile => tile.Coordinates.CalculateMaxDistance(cogmindPosition))
                .FirstOrDefault();

            if (garrisonTile == null)
            {
                // Couldn't find a Garrison, return unsccessful
                throw new Exception("Couldn't find garrison tile");
            }

            // Teleport to the open side of the Garrison, then open it.
            // The tile the stairs will be on are closed off on 3/4 directions,
            // while all other cardinally adjacent tiles are part of the
            // Garrison access itself
            var tiles = this.ws.LuigiAiData.Tiles;
            MapTile targetTile;
            MovementDirection direction;

            if (
                garrisonTile.X > 1
                && IsGarrisonEntranceTile(tiles[garrisonTile.X - 1, garrisonTile.Y])
            )
            {
                targetTile = tiles[garrisonTile.X - 1, garrisonTile.Y];
                direction = MovementDirection.Right;
            }
            else if (
                garrisonTile.X < this.ws.LuigiAiData.MapWidth - 2
                && IsGarrisonEntranceTile(tiles[garrisonTile.X + 1, garrisonTile.Y])
            )
            {
                targetTile = tiles[garrisonTile.X + 1, garrisonTile.Y];
                direction = MovementDirection.Left;
            }
            else if (
                garrisonTile.Y > 1
                && IsGarrisonEntranceTile(tiles[garrisonTile.X, garrisonTile.Y - 1])
            )
            {
                targetTile = tiles[garrisonTile.X, garrisonTile.Y - 1];
                direction = MovementDirection.Down;
            }
            else if (
                garrisonTile.Y < this.ws.LuigiAiData.MapHeight - 2
                && IsGarrisonEntranceTile(tiles[garrisonTile.X, garrisonTile.Y + 1])
            )
            {
                targetTile = tiles[garrisonTile.X, garrisonTile.Y + 1];
                direction = MovementDirection.Up;
            }
            else
            {
                throw new Exception("Couldn't find open Garrison tile");
            }

            this.ws.WizardCommands.TeleportToTile(targetTile);
            this.ws.MachineHacking.OpenHackingPopup(direction);
            this.ws.MachineHacking.PerformHack(Keys.A); // Open hack is always first
            this.ws.MachineHacking.CloseHackingPopup();

            // We are on the stairs so enter them now
            this.ws.Movement.EnterStairs();

            return;

            bool IsGarrisonEntranceTile(MapTile tile)
            {
                if (tile.Prop != null)
                {
                    return false;
                }

                var tiles = this.ws.LuigiAiData.Tiles;
                var surroundingTiles = new List<MapTile>
                {
                    { tiles[tile.X + 1, tile.Y] },
                    { tiles[tile.X, tile.Y - 1] },
                    { tiles[tile.X - 1, tile.Y] },
                    { tiles[tile.X, tile.Y + 1] },
                };

                return surroundingTiles.Where(tile => tile.Prop?.Name == "Garrison Access").Count()
                    == 3;
            }
        }

        private void TakeExit()
        {
            this.ws.WizardCommands.RevealMap();

            var allStairsCoords = this
                .ws.TileAnalysis.FindTilesByType(TileType.Stairs)
                .Select(tile => tile.Coordinates)
                .ToList();

            // Perform Force(Override) then take the nearest stairs
            var cogmindPosition = this.ws.LuigiAiData.CogmindCoordinates;
            var terminal = this
                .ws.PropAnalysis.FindTilesByPropType(PropType.GarrisonTerminal, true)
                .OrderBy(tile => cogmindPosition.CalculateMaxDistance(tile.Coordinates))
                .First();

            // Teleport left of the Terminal
            this.ws.WizardCommands.TeleportToTile(terminal.X - 1, terminal.Y);
            this.ws.MachineHacking.OpenHackingPopup(MovementDirection.Right);
            this.ws.MachineHacking.PerformManualHack("Force(Override)");
            this.ws.MachineHacking.CloseHackingPopup();

            // Find closest exit, then take it
            cogmindPosition = new MapPoint(terminal.X - 1, terminal.Y);
            var closestExit = allStairsCoords
                .OrderBy(coordinates => cogmindPosition.CalculateMaxDistance(coordinates))
                .First();
            this.ws.WizardCommands.TeleportToTile(closestExit);
            this.ws.Movement.EnterStairs(garrisonStairs: true);
        }

        private class State : IScriptState
        {
            public Dictionary<int, Dictionary<int, int>> BranchLoopsPerDepth { get; set; } = [];

            public bool Initialized { get; set; }

            public Dictionary<int, Dictionary<int, int>> LoopAttemptsPerDepth { get; set; } = [];

            public Dictionary<int, Dictionary<int, int>> LoopsPerDepth { get; set; } = [];

            public Dictionary<
                int,
                Dictionary<int, Dictionary<MapType, int>>
            > LoopedMapPerDepth { get; set; } = [];

            public int NumRuns { get; set; }

            public void Initialize()
            {
                if (this.Initialized)
                {
                    return;
                }

                this.Initialized = true;

                for (var i = 1; i <= NumDepths; i++)
                {
                    this.BranchLoopsPerDepth[i] = [];
                    this.LoopAttemptsPerDepth[i] = [];
                    this.LoopsPerDepth[i] = [];
                    this.LoopedMapPerDepth[i] = [];
                }
            }
        }
    }
}
