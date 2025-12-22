using System.Diagnostics;
using WizMind.Analysis;
using WizMind.Definitions;
using WizMind.Instances;
using WizMind.LuigiAi;
using WizMind.Utilities;

namespace WizMind.Interaction
{
    public class WizardCommands(
        CogmindProcess cogmindProcess,
        GameDefinitions definitions,
        Input input,
        LuigiAiData luigiAiData,
        MachineHacking machineHacking,
        Movement movement,
        PropAnalysis propAnalysis,
        TileAnalysis tileAnalysis
    )
    {
        private enum XDirection
        {
            Left,
            None,
            Right,
        }

        private enum YDirection
        {
            Up,
            None,
            Down,
        }

        private readonly CogmindProcess cogmindProcess = cogmindProcess;
        private readonly GameDefinitions definitions = definitions;
        private readonly Input input = input;
        private bool inWizardMode;
        private readonly LuigiAiData luigiAiData = luigiAiData;
        private readonly MachineHacking machineHacking = machineHacking;
        private readonly Movement movement = movement;
        private readonly PropAnalysis propAnalysis = propAnalysis;
        private readonly TileAnalysis tileAnalysis = tileAnalysis;

        /// <summary>
        /// Adds the specified number of slots to the specified slot type.
        /// </summary>
        /// <param name="slot">The slot to expand.</param>
        /// <param name="numSlots">The number of slots to add.</param>
        public void AddSlots(SlotType slot, int numSlots)
        {
            while (numSlots > 0)
            {
                // Can only add up to 9 slots at once through the command
                var slotsToAdd = Math.Min(numSlots, 9);
                var slotString = slot switch
                {
                    SlotType.Power => "po",
                    SlotType.Propulsion => "pr",
                    SlotType.Utility => "ut",
                    SlotType.Weapon => "we",
                    _ => throw new Exception("Invalid slot type"),
                };

                // Send the add slots command now
                this.EnterWizardModeCommand($"as {slotString}{slotsToAdd}");
                numSlots -= slotsToAdd;
            }
        }

        /// <summary>
        /// Spawns an item attached to Cogmind.
        /// </summary>
        /// <remarks>
        /// Does not verify that the part can actually be attached. If it
        /// doesn't fit, it will go into inventory or on the floor instead.
        /// </remarks>
        /// <param name="itemName">The name of the item to create.</param>
        public void AttachItem(string itemName)
        {
            if (!this.definitions.ItemNameToId.ContainsKey(itemName))
            {
                throw new ArgumentException($"Item {itemName} does not exist");
            }

            // Ideally invalidate data here but items aren't updated after wizard mode commands
            this.EnterWizardModeCommand($"a {itemName}");
        }

        /// <summary>
        /// Enters a command in the wizard mode console.
        /// </summary>
        /// <param name="command">The command to enter.</param>
        public void EnterWizardModeCommand(string command)
        {
            this.EnsureWizardMode();

            // Open the wizard mode console
            this.input.SendKeystroke(Keys.D, KeyModifier.AltShift);
            Thread.Sleep(TimeDuration.WizardConsoleSleep);

            this.input.SendString(command);
            Thread.Sleep(TimeDuration.EnterStringSleep);

            this.input.SendKeystroke(Keys.Enter);
            Thread.Sleep(TimeDuration.EnterStringSleep);
        }

        /// <summary>
        /// Starts wizard mode if it is not already enabled.
        /// </summary>
        public void EnsureWizardMode()
        {
            if (this.inWizardMode)
            {
                return;
            }

            // Verify wizard mode key exists, if it doesn't then things won't work right
            var wizardKeyPath = Path.Combine(
                Directory.GetParent(this.cogmindProcess.Process.MainModule!.FileName)!.FullName,
                "wizard_access_private_key_do_not_share.txt"
            );
            if (!Path.Exists(wizardKeyPath))
            {
                throw new Exception("Wizard mode key is missing");
            }

            // Send the wizard mode command twice to start it
            this.input.SendKeystroke(Keys.D, KeyModifier.AltShift);
            this.input.SendKeystroke(Keys.D, KeyModifier.AltShift);

            // If we are starting from a state with wizard mode active, we will
            // be in the console now and need to exit it
            this.input.SendKeystroke(Keys.Escape);
            Thread.Sleep(TimeDuration.EnterStringSleep);

            this.inWizardMode = true;
        }

        /// <summary>
        /// Spawns an item in Cogmind's inventory.
        /// </summary>
        /// <remarks>
        /// Does not verify that the part can actually fit in inventory. If it
        /// doesn't fit, it will go on the floor instead.
        /// </remarks>
        /// <param name="itemName">The name of the item to create.</param>
        public void GiveItem(string itemName)
        {
            if (!this.definitions.ItemNameToId.ContainsKey(itemName))
            {
                throw new ArgumentException($"Item {itemName} does not exist");
            }

            // Ideally invalidate data here but items aren't updated after wizard mode commands
            this.EnterWizardModeCommand($"g {itemName}");
        }

        /// <summary>
        /// Jumps to the main map on the specified depth.
        /// </summary>
        /// <param name="depth">Depth of the map to go to.</param>
        /// <param name="force">Whether to force teleport to the map if Cogmind is already on it.</param>
        public bool GotoMainMap(int depth, bool force = false)
        {
            var map = this.definitions.MainMaps[depth];

            return this.GotoMap(map, depth, force);
        }

        /// <summary>
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The name of the map.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        /// <param name="force">Whether to force teleport to the map if Cogmind is already on it.</param>
        public bool GotoMap(string mapName, int? depth = null, bool force = false)
        {
            if (!this.definitions.MapNameToDefinition.TryGetValue(mapName, out var map))
            {
                throw new ArgumentException($"Map {mapName} is not supported");
            }

            return this.GotoMap(map, depth, force);
        }

        /// <summary>
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The type of the map.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        /// <param name="force">Whether to force teleport to the map if Cogmind is already on it.</param>
        public bool GotoMap(MapType mapType, int? depth = null, bool force = false)
        {
            if (!this.definitions.MapTypeToDefinition.TryGetValue(mapType, out var map))
            {
                throw new ArgumentException($"Map {mapType} is not supported");
            }

            return this.GotoMap(map, depth, force);
        }

        /// <summary>
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The map to go to.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        /// <param name="force">Whether to force teleport to the map if Cogmind is already on it.</param>
        public bool GotoMap(MapDefinition map, int? depth = null, bool force = false)
        {
            this.EnsureWizardMode();

            if (depth.HasValue && depth != MapDefinition.NoMapDepth)
            {
                if (depth < 1 || depth > 10)
                {
                    throw new ArgumentException($"Depth of {depth} is invalid");
                }

                if (depth > map.firstDepth || depth < map.lastDepth)
                {
                    throw new ArgumentException(
                        $"Depth of {depth} is not valid for map {map.name}"
                    );
                }
            }

            if (
                !force
                && this.luigiAiData.MapType == map.type
                && (!depth.HasValue || this.luigiAiData.Depth == depth.Value)
            )
            {
                // Already on the map, don't teleport to it again unless we
                // are forcing the map teleport
                return true;
            }

            if (map.mainMapRequired)
            {
                // If a branch requires we visit a main map beforehand, go there first
                // This is required for submaps like Garrisons, DSFs, and Wastes
                if (!depth.HasValue)
                {
                    throw new ArgumentException($"Map {map.name} requires depth to be specified");
                }

                var mainMap = this.definitions.MainMaps[depth.Value];

                // Only need to go to the main map if not already forced
                if (
                    this.luigiAiData.MapType != mainMap.type
                    || this.luigiAiData.Depth != depth.Value
                )
                {
                    var success = this.GotoMap(mainMap, depth.Value, force);
                    if (!success)
                    {
                        throw new Exception($"Failed to go to main map {mainMap.name}");
                    }
                }
            }

            // Depth is only processed as a single digit in the command
            var depthCommand = depth == 10 ? 0 : depth;

            // Enter the jump to map command now
            this.EnterWizardModeCommand(
                $"goto {map.tag}{(depthCommand.HasValue ? depthCommand : string.Empty)}"
            );

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var lastAction = this.luigiAiData.LastAction;

            // Attempt to load the map periodically, it may take several
            // seconds to load. When we have confirmed the new map then
            // it's safe to continue
            while (stopwatch.ElapsedMilliseconds < TimeDuration.MapLoadTime)
            {
                Thread.Sleep(TimeDuration.MapLoadSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);

                if (
                    lastAction == this.luigiAiData.LastAction
                    || this.luigiAiData.MapType != map.type
                    || (depth.HasValue && this.luigiAiData.Depth != depth)
                )
                {
                    // Data hasn't updated yet
                    continue;
                }

                // Wait for the UI to finish loading after the map is done
                Thread.Sleep(TimeDuration.PostMapLoadSleep);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Reveals all tiles in the current map.
        /// </summary>
        /// <param name="full">Whether to reveal all tiles and items or just the outline of rooms.</param>
        public void RevealMap(bool full = true)
        {
            this.EnsureWizardMode();

            // The first command reveals just the outlines of rooms and machines
            this.input.SendKeystroke(Keys.K, KeyModifier.AltCtrlShift);

            if (full)
            {
                // The second command reveals all tiles and items
                this.input.SendKeystroke(Keys.K, KeyModifier.AltCtrlShift);
            }

            Thread.Sleep(TimeDuration.RevealMapSleep);

            // Invalidate all data since tile data should now be present
            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
        }

        /// <summary>
        /// Teleports to the specified tile.
        /// </summary>
        /// <param name="tile">The tile to teleport to.</param
        public void TeleportToTile(MapTile tile)
        {
            this.TeleportToTile(tile.X, tile.Y);
        }

        /// <summary>
        /// Teleports to the tile at the specified x/y point.
        /// </summary>
        /// <param name="coordinates">The coordinates of the tile to teleport to.</param>
        public void TeleportToTile(MapPoint point)
        {
            this.TeleportToTile(point.X, point.Y);
        }

        /// <summary>
        /// Teleports to the tile at the specified x/y position.
        /// </summary>
        /// <param name="x">The x tile to teleport to.</param>
        /// <param name="y">The y tile to teleport to.</param>
        public void TeleportToTile(int x, int y)
        {
            this.EnsureWizardMode();

            // Move the cursor into position
            this.MoveCursorToPosition(x, y);

            // Disable keyboard mode which will center the cursor at the
            // focused tile
            this.input.SendKeystroke(Keys.F2);

            // Alt + right click on the tile to teleport
            this.input.SendMousepress(MouseButton.RightButton, keyModifier: KeyModifier.Alt);

            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
        }

        /// <summary>
        /// Searches and teleports to the nearest Garrison access, then hacks
        /// it open and enters it.
        /// </summary>
        /// <remarks>
        /// Assumes that Cogmind has 100% hack chance for opening Garriisons,
        /// either through direct hacking bonus or RIF.
        /// </remarks>
        /// <returns><c>true</c> if the garrison was successfully entered.</returns>
        public bool TryFindAndEnterGarrison()
        {
            this.RevealMap();
            var cogmindPosition = this.luigiAiData.CogmindCoordinates;

            // Find the closest interactive Garrison tile
            var garrisonTile = this
                .propAnalysis.FindTilesByPropType(PropType.GarrisonAccess, true)
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
            var tiles = this.luigiAiData.Tiles;
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
                garrisonTile.X < this.luigiAiData.MapWidth - 2
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
                garrisonTile.Y < this.luigiAiData.MapHeight - 2
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

            this.TeleportToTile(targetTile);
            this.machineHacking.OpenHackingPopup(direction);
            this.machineHacking.PerformHack(Keys.A); // Open hack is always first
            this.machineHacking.CloseHackingPopup();

            // We are on the stairs so enter them now
            this.movement.EnterStairs();

            return true;

            bool IsGarrisonEntranceTile(MapTile tile)
            {
                if (tile.Prop != null)
                {
                    return false;
                }

                var tiles = this.luigiAiData.Tiles;
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

        private void MoveCursorToPosition(int x, int y)
        {
            // When holding shift the cursor moves 4 tiles instead of just 1
            const int ShiftDistance = 4;

            // With examineEntersKeyboardMode=1, X ensures we are always in
            // keyboard mode. If we're starting in mouse mode, the first X will
            // always jump to the center of the tile Cogmind is on. However, if
            // we're in keyboard mode to start, focus may not be handled
            // appropriately if Cogmind is not the focused application.
            // X -> F2 -> X guarantees that we will always end up with the
            // correct focus and position.
            this.input.SendKeystroke(Keys.X);
            this.input.SendKeystroke(Keys.F2);
            this.input.SendKeystroke(Keys.X);

            Thread.Sleep(TimeDuration.CursorAppearSleep);

            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            var (currentX, currentY) = this.luigiAiData.MapCursorPosition;

            while (currentX != x || currentY != y)
            {
                var absXDiff = Math.Abs(currentX - x);
                var minXMoves = (absXDiff / ShiftDistance) + (absXDiff % ShiftDistance);

                var absYDiff = Math.Abs(currentY - y);
                var minYMoves = (absYDiff / ShiftDistance) + (absYDiff % ShiftDistance);

                var xDirection = x switch
                {
                    _ when x < currentX => XDirection.Left,
                    _ when x == currentX => XDirection.None,
                    _ => XDirection.Right,
                };
                var yDirection = y switch
                {
                    _ when y < currentY => YDirection.Up,
                    _ when y == currentY => YDirection.None,
                    _ => YDirection.Down,
                };

                // Use shift if:
                //   We are only moving along one axis and the number of tiles
                //       to traverse is >= 4 along that axis.
                //   We are moving along multiple axes and the number of tiles
                //       to traverse is >= 4 along both axes.
                //   We are moving along multiple axes and the number of tiles
                //       to traverse is only >= along 1 axis but the total
                //       number of cursor moves is not increased by jumping
                //       over the target tile for one axis.
                //   Note: As a result of this last case, we can end up jumping
                //       back and forth across one axis over the exit, though
                //       it doesn't add any extra time to do so. At some point
                //       it might potentially be nice to get rid of this.
                var useShift =
                    absXDiff >= ShiftDistance
                        && (
                            yDirection == YDirection.None
                            || absYDiff >= ShiftDistance
                            || ((ShiftDistance - absYDiff) <= minXMoves)
                        )
                    || (
                        absYDiff >= ShiftDistance
                        && (
                            xDirection == XDirection.None || (ShiftDistance - absXDiff) <= minYMoves
                        )
                    );

                // Determine which way to move
                var key = (x, y) switch
                {
                    _ when xDirection == XDirection.Left && yDirection == YDirection.Down =>
                        Keys.NumPad1,
                    _ when xDirection == XDirection.None && yDirection == YDirection.Down =>
                        Keys.NumPad2,
                    _ when xDirection == XDirection.Right && yDirection == YDirection.Down =>
                        Keys.NumPad3,
                    _ when xDirection == XDirection.Left && yDirection == YDirection.None =>
                        Keys.NumPad4,
                    _ when xDirection == XDirection.Right && yDirection == YDirection.None =>
                        Keys.NumPad6,
                    _ when xDirection == XDirection.Left && yDirection == YDirection.Up =>
                        Keys.NumPad7,
                    _ when xDirection == XDirection.None && yDirection == YDirection.Up =>
                        Keys.NumPad8,
                    _ when xDirection == XDirection.Right && yDirection == YDirection.Up =>
                        Keys.NumPad9,
                    _ => Keys.NumPad5,
                };

                // Update the cursor position
                this.input.SendKeystroke(key, useShift ? KeyModifier.Shift : KeyModifier.None);

                // Update cursor position
                Thread.Sleep(TimeDuration.CursorMoveSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
                (currentX, currentY) = this.luigiAiData.MapCursorPosition;
            }
        }
    }
}
