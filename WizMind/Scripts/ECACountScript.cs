using WizMind.Analysis;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;
using WizMind.Utilities;

namespace WizMind.Scripts
{
    public class ECACountScript : IScript
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

            // Discover Warlord and Section 7 depths
            this.ws.WizardCommands.GotoMap(MapType.MAP_WAR);
            var warlordDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_SEC);
            var section7Depth = this.ws.LuigiAiData.Depth;

            var ecaScore = 0.0f;
            var totalLoops = 0;

            for (var depth = NumDepths; depth >= 1; depth--)
            {
                var loopsPerDepth = 0;

                if (depth == warlordDepth)
                {
                    // Skip Warlord depth, assuming we always head to
                    // caves without trying to go into a Garrison
                    continue;
                }

                var map = this.ws.Definitions.MainMaps[depth];
                if (this.ws.LuigiAiData.MapType != map.type || this.ws.LuigiAiData.Depth != depth)
                {
                    // Start by teleporting to the main map if needed
                    this.ws.WizardCommands.GotoMap(map, depth);
                }

                var continueLooping = true;

                // Process until we don't loop anymore
                while (this.ws.LuigiAiData.Depth == depth && continueLooping)
                {
                    // First save the visit to the map
                    map = this.ws.Definitions.MapTypeToDefinition[this.ws.LuigiAiData.MapType];

                    var mapVisits = this.state.MapVisitsPerDepth[depth];
                    mapVisits[map.name] = mapVisits.GetValueOrDefault(map.name) + 1;

                    if (this.TryFindAndEnterGarrison())
                    {
                        // If we entered a Garrison successfully, process the
                        // contents for ECA bonus
                        ecaScore += this.CalculateEcaBonus(depth);
                        this.TakeLoopOrNormalExit();

                        if (this.ws.LuigiAiData.Depth == depth)
                        {
                            // Still on same depth means we looped, either to
                            // another main floor or to a branch
                            totalLoops += 1;
                            loopsPerDepth += 1;
                        }

                        if (depth == 1)
                        {
                            // Only allow 1 loop on Access since the Garrisons
                            // get locked down afterwards
                            break;
                        }
                    }
                    else
                    {
                        switch (this.ws.LuigiAiData.MapType)
                        {
                            case MapType.MAP_EXT:
                            {
                                // Teleport to Hub since it has a Garrison
                                this.ws.WizardCommands.GotoMap(MapType.MAP_HUB);
                                break;
                            }

                            case MapType.MAP_QUA:
                                goto case MapType.MAP_TES;

                            case MapType.MAP_TES:
                            {
                                // For Testing/Quarantine, try going to S7 as
                                // there may be a Garrison there
                                if (this.ws.LuigiAiData.Depth == section7Depth)
                                {
                                    this.ws.WizardCommands.GotoMap(MapType.MAP_SEC);
                                }
                                else
                                {
                                    // If no S7 on this floor then give up
                                    continueLooping = false;
                                }
                                break;
                            }

                            default:
                                // Found another branch map without a Garrison
                                // Just give up and jump to next floor
                                continueLooping = false;
                                break;
                        }
                    }
                }

                this.state.LoopsPerDepth[depth].Add(loopsPerDepth);
            }

            this.state.EcaScorePerRun.Add(ecaScore);
            this.state.LoopsPerRun.Add(totalLoops);

            return true;
        }

        private float CalculateEcaBonus(int depth)
        {
            this.ws.WizardCommands.RevealMap();

            // Calculate ECA bonus
            // Initial .02 is from interior compromised
            var bonus = 0.02f;

            foreach (var (prop, number) in this.ws.PropAnalysis.CalculatePropCounts())
            {
                // Add bonus per machine types
                if (prop == "RIF Installer")
                {
                    bonus += number * 0.02f;
                }
                else if (prop == "Garrison Relay")
                {
                    bonus += number * 0.01f;
                }
                else if (prop == "Phase Generator")
                {
                    bonus += number * 0.01f;
                }
            }

            // Add ECA bonuses to stats
            this.state.EcaScorePerGarrison.Add(bonus);
            this.state.EcaScorePerGarrisonByDepth[depth].Add(bonus);

            return bonus;
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

        private void TakeLoopOrNormalExit()
        {
            var initialStairsCoords = this
                .ws.TileAnalysis.FindTilesByType(TileType.Stairs)
                .Select(tile => tile.Coordinates)
                .ToList();

            // The first turn we enter a map, loop exits are not yet active so
            // we will only find 3 stairs initially. After waiting a turn and
            // doing another reveal, we will find the 4th exit which is a
            // guaranteed loop exit if it exists.
            this.ws.Movement.Wait();
            this.ws.WizardCommands.RevealMap();

            var allStairsCoords = this
                .ws.TileAnalysis.FindTilesByType(TileType.Stairs)
                .Select(tile => tile.Coordinates)
                .ToList();

            if (allStairsCoords.Count != initialStairsCoords.Count)
            {
                // There is a loop exit present, find and then take it
                foreach (var stairsCoords in allStairsCoords)
                {
                    if (initialStairsCoords.Contains(stairsCoords))
                    {
                        continue;
                    }

                    this.ws.WizardCommands.TeleportToTile(stairsCoords);
                    this.ws.Movement.EnterStairs(garrisonStairs: true);

                    return;
                }

                throw new Exception("Found loop stairs but failed to take them");
            }

            // Without a loop exit, perform Force(Override) then take the
            // nearest stairs
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
            public bool Initialized { get; set; }

            public List<float> EcaScorePerRun { get; set; } = [];

            public List<float> EcaScorePerGarrison { get; set; } = [];

            public Dictionary<int, List<float>> EcaScorePerGarrisonByDepth { get; set; } = [];

            public List<int> LoopsPerRun { get; set; } = [];

            public Dictionary<int, List<int>> LoopsPerDepth { get; set; } = [];

            public Dictionary<int, Dictionary<string, int>> MapVisitsPerDepth { get; set; } = [];

            public int NumRuns { get; set; }

            public void Initialize()
            {
                if (!this.Initialized)
                {
                    for (var depth = NumDepths; depth >= 1; depth--)
                    {
                        this.EcaScorePerGarrisonByDepth[depth] = [];
                        this.LoopsPerDepth[depth] = [];
                        this.MapVisitsPerDepth[depth] = [];
                    }

                    this.Initialized = true;
                }
            }
        }
    }
}
