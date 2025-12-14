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
        }

        public bool ProcessRun()
        {
            // Add maximum number of slots ahead of time so we don't
            // have to pick them on evolutions
            this.ws.WizardCommands.AddSlots(SlotType.Utility, 19);

            // Add for 100% hacking success
            this.ws.WizardCommands.AttachItem("Architect God Chip A");

            // Go to the depth's Garrison and take a normal exit
            for (var depth = NumDepths; depth >= 2; depth--)
            {
                this.ws.WizardCommands.GotoMap(this.ws.Definitions.MainMaps[depth].type, depth);
                if (!this.TryFindAndEnterGarrison())
                {
                    this.ws.WizardCommands.GotoMap(MapType.MAP_GAR, depth);
                }
                this.TakeExit();

                // Check whether we looped
                if (this.ws.LuigiAiData.Depth == depth)
                {
                    this.state.NumLoopsPerDepth[depth] =
                        this.state.NumLoopsPerDepth.GetValueOrDefault(depth) + 1;
                }

                this.state.NumLoopAttemptsPerDepth[depth] =
                    this.state.NumLoopAttemptsPerDepth.GetValueOrDefault(depth) + 1;
            }

            return true;
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

        private bool TryFindAndEnterGarrison()
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
                return false;
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

            return true;

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

        private class State : IScriptState
        {
            public Dictionary<int, int> NumLoopAttemptsPerDepth { get; set; } = [];

            public Dictionary<int, int> NumLoopsPerDepth { get; set; } = [];

            public int NumRuns { get; set; }
        }
    }
}
