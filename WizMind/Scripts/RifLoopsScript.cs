using WizMind.Analysis;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;
using WizMind.Utilities;

namespace WizMind.Scripts
{
    public class RifLoopsScript : IScript
    {
        private State state = null!;
        private ScriptWorkspace ws = null!;

        public IScriptState SerializableState => this.state;

        public Type SerializableStateType => typeof(State);

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

            // Discover all 0b10 branch depths
            this.ws.GameState.SaveGame();
            this.ws.WizardCommands.GotoMap(MapType.MAP_STO);
            var storageDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_EXT);
            var extensionDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_ARM);
            var armoryDepth = this.ws.LuigiAiData.Depth;
            this.ws.WizardCommands.GotoMap(MapType.MAP_SEC);
            var section7Depth = this.ws.LuigiAiData.Depth;

            var loopDepths = new HashSet<int> { storageDepth, extensionDepth, armoryDepth, 3, 2 };

            // Need to load the game or else the game does funky things with
            // evolved slots disappearing which causes the evolution screen
            // to show up
            this.ws.GameState.LoadGame();

            // Start at depth 8
            this.ws.WizardCommands.GotoMap(MapType.MAP_MAT, 8);

            Dictionary<int, int> depthInstallers = new()
            {
                { 8, 0 },
                { 7, 0 },
                { 6, 0 },
                { 5, 0 },
                { 4, 0 },
                { 3, 0 },
                { 2, 0 },
                { 1, 0 },
            };

            var allInstallers = 0;
            var looped = false;
            var prevDepth = 8;

            int processedInstallers;

            while (this.ws.LuigiAiData.Depth > 1)
            {
                var depth = this.ws.LuigiAiData.Depth;
                var mapType = this.ws.LuigiAiData.MapType;

                if (depth != prevDepth)
                {
                    looped = false;
                }

                switch (mapType)
                {
                    case MapType.MAP_MAT:
                    case MapType.MAP_FAC:
                    case MapType.MAP_RES:
                    case MapType.MAP_ACC:
                    case MapType.MAP_STO:
                    case MapType.MAP_HUB:
                    case MapType.MAP_TES:
                    case MapType.MAP_QUA:
                    case MapType.MAP_SEC:
                        // Maps with garrisons, try to go in one
                        if (this.ws.WizardCommands.TryFindAndEnterGarrison())
                        {
                            if (!looped)
                            {
                                // Only can loop once since we aren't taking the
                                // guaranteed loop prefab
                                this.state.AttemptedLoopsByInstalled[allInstallers] =
                                    this.state.AttemptedLoopsByInstalled.GetValueOrDefault(
                                        allInstallers
                                    ) + 1;
                            }

                            var branchLoopIndex = looped ? 1 : 0;

                            // Can branch if we are on a main branchable depth
                            // or in a branch with a subsequent branch map
                            var canBranch =
                                (
                                    loopDepths.Contains(depth)
                                    && this.ws.Definitions.IsMainMapType(mapType)
                                )
                                || mapType == MapType.MAP_STO
                                || (
                                    (mapType == MapType.MAP_QUA || mapType == MapType.MAP_TES)
                                    && depth == section7Depth
                                );

                            if (canBranch)
                            {
                                this.state.AttemptedBranchByInstalled[branchLoopIndex][
                                    allInstallers
                                ] =
                                    this.state.AttemptedBranchByInstalled[branchLoopIndex]
                                        .GetValueOrDefault(allInstallers) + 1;
                            }

                            // If we entered a Garrison then process it now
                            processedInstallers = this.ProcessGarrison();

                            if (
                                this.ws.LuigiAiData.Depth == depth
                                && this.ws.LuigiAiData.MapType == mapType
                            )
                            {
                                // Successful loop
                                this.state.SuccessfulLoopsByInstalled[allInstallers] =
                                    this.state.SuccessfulLoopsByInstalled.GetValueOrDefault(
                                        allInstallers
                                    ) + 1;
                                looped = true;
                            }

                            if (
                                canBranch
                                && this.ws.LuigiAiData.Depth == depth
                                && this.ws.LuigiAiData.MapType != mapType
                            )
                            {
                                // Successful branch
                                this.state.EnteredBranchByInstalled[branchLoopIndex][
                                    allInstallers
                                ] =
                                    this.state.EnteredBranchByInstalled[branchLoopIndex]
                                        .GetValueOrDefault(allInstallers) + 1;
                            }

                            allInstallers += processedInstallers;
                            depthInstallers[depth] += processedInstallers;
                        }
                        else if (
                            (mapType == MapType.MAP_TES || mapType == MapType.MAP_QUA)
                            && depth == section7Depth
                        )
                        {
                            // If we're in a Research branch without a Garrison,
                            // manually jump to Section 7 to check for another one
                            this.ws.WizardCommands.GotoMap(MapType.MAP_SEC);
                        }
                        else
                        {
                            // Otherwise no Garrison available, so just bail
                            this.ws.WizardCommands.GotoMainMap(depth - 1);
                        }
                        break;

                    case MapType.MAP_EXT:
                        // In Extension, jump to Hub to try to get another Garrison
                        this.ws.WizardCommands.GotoMap(MapType.MAP_HUB);
                        break;

                    case MapType.MAP_REC:
                        // In Recycling we have to skip one depth to exit
                        this.ws.WizardCommands.GotoMainMap(depth - 2);
                        break;

                    case MapType.MAP_GAR:
                        throw new Exception("Should have exited the Garrison by now");

                    default:
                        // Map has no garrison, just teleport to the next main floor
                        this.ws.WizardCommands.GotoMainMap(depth - 1);
                        break;
                }

                prevDepth = depth;
            }

            // Now on access, process one final Garrison since we will be locked out afterwards
            this.ws.WizardCommands.TryFindAndEnterGarrison();
            processedInstallers = this.ProcessGarrison();
            allInstallers += processedInstallers;
            depthInstallers[1] += processedInstallers;

            // Save installer state
            this.state.InstallersPerRun.Add(allInstallers);

            foreach (var (depth, installers) in depthInstallers)
            {
                this.state.InstallersPerDepth[depth].Add(installers);
            }

            return true;
        }

        private IEnumerable<(MapTile installerTile, MapTile adjacentTile)> GetInstallerLocations(
            List<List<MapTile>> installerGroups
        )
        {
            foreach (var group in installerGroups)
            {
                var surroundingTiles = new HashSet<MapTile>();

                // Get a list of tiles surrounding the installer
                foreach (var tile in group)
                {
                    foreach (
                        var surroundingTile in this.ws.TileAnalysis.GetSurroundingCardinalTiles(
                            tile
                        )
                    )
                    {
                        surroundingTiles.Add(surroundingTile);
                    }
                }

                // There should be 1 tile in the middle of the installer that
                // is surrounded on 3 sides by the installer prop. This is the
                // install tile.
                var installLocation = surroundingTiles.First(tile =>
                    this.ws.TileAnalysis.GetSurroundingCardinalTiles(tile)
                        .Where(t => this.ws.PropAnalysis.IsPropType(t, PropType.RifInstaller))
                        .Count() == 3
                );

                var adjacentTile = this
                    .ws.TileAnalysis.GetSurroundingCardinalTiles(installLocation)
                    .First(tile => tile.Prop == null);

                yield return (installLocation, adjacentTile);
            }
        }

        private int ProcessGarrison()
        {
            // Find all RIF installers first
            this.ws.WizardCommands.RevealMap();

            var installerGroups = this.ws.PropAnalysis.FindTileGroupsByPropType(
                PropType.RifInstaller
            );

            var rifInstallers = 0;
            var currentCoordinates = this.ws.LuigiAiData.CogmindCoordinates;

            foreach (
                var (installerTile, adjacentTile) in this.GetInstallerLocations(installerGroups)
            )
            {
                currentCoordinates = installerTile.Coordinates;

                // Teleport to the adjacent tile to the installer tile first
                this.ws.WizardCommands.TeleportToTile(adjacentTile);

                MovementDirection direction;
                if (adjacentTile.X < installerTile.X)
                {
                    direction = MovementDirection.Right;
                }
                else if (adjacentTile.X > installerTile.X)
                {
                    direction = MovementDirection.Left;
                }
                else if (adjacentTile.Y < installerTile.Y)
                {
                    direction = MovementDirection.Down;
                }
                else
                {
                    direction = MovementDirection.Up;
                }

                // Move into the installer tile to use it
                this.ws.Movement.Move(direction);
                rifInstallers += 1;
            }

            // Find the closest exit and take it
            var stairsCoords = this
                .ws.TileAnalysis.FindTilesByType(TileType.Stairs)
                .Select(tile => tile.Coordinates)
                .OrderBy(currentCoordinates.CalculateMaxDistance)
                .First();
            this.ws.WizardCommands.TeleportToTile(stairsCoords);
            this.ws.Movement.EnterStairs(garrisonStairs: true);

            return rifInstallers;
        }

        private class State : IScriptState
        {
            public int NumRuns { get; set; }

            public bool Initialized { get; set; }

            public Dictionary<int, List<int>> InstallersPerDepth { get; set; } = [];

            public List<int> InstallersPerRun { get; set; } = [];

            public List<Dictionary<int, int>> AttemptedBranchByInstalled { get; set; } = [];

            public Dictionary<int, int> AttemptedLoopsByInstalled { get; set; } = [];

            public List<Dictionary<int, int>> EnteredBranchByInstalled { get; set; } = [];

            public Dictionary<int, int> SuccessfulLoopsByInstalled { get; set; } = [];

            public void Initialize()
            {
                if (this.Initialized)
                {
                    return;
                }

                this.Initialized = true;

                for (var i = 1; i <= 8; i++)
                {
                    this.InstallersPerDepth[i] = [];
                }

                // Add loop and non-looped branch entrance types
                this.AttemptedBranchByInstalled.Add([]);
                this.AttemptedBranchByInstalled.Add([]);

                this.EnteredBranchByInstalled.Add([]);
                this.EnteredBranchByInstalled.Add([]);
            }
        }
    }
}
