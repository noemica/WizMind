using System.Diagnostics;
using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class WizardCommands(
        CogmindProcess cogmindProcess,
        GameDefinitions definitions,
        Input input,
        LuigiAiData luigiAiData
    )
    {
        private readonly CogmindProcess cogmindProcess = cogmindProcess;
        private readonly GameDefinitions definitions = definitions;
        private readonly Input input = input;
        private bool inWizardMode;
        private readonly LuigiAiData luigiAiData = luigiAiData;

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
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The name of the map.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        public bool GotoMap(string mapName, int? depth = null)
        {
            if (!this.definitions.MapNameToDefinition.TryGetValue(mapName, out var map))
            {
                throw new ArgumentException($"Map {mapName} is not supported");
            }

            return this.GotoMap(map, depth);
        }

        /// <summary>
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The type of the map.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        public bool GotoMap(MapType mapType, int? depth = null)
        {
            if (!this.definitions.MapTypeToDefinition.TryGetValue(mapType, out var map))
            {
                throw new ArgumentException($"Map {mapType} is not supported");
            }

            return this.GotoMap(map, depth);
        }

        /// <summary>
        /// Jumps to the specified map.
        /// </summary>
        /// <param name="map">The map to go to.</param>
        /// <param name="depth">Depth of the map to go to if multiple depths are available.</param>
        public bool GotoMap(MapDefinition map, int? depth = null)
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

            if (map.mainMapRequired)
            {
                // If a branch requires we visit a main map beforehand, go there first
                // This is required for submaps like Garrisons, DSFs, and Wastes
                if (!depth.HasValue)
                {
                    throw new ArgumentException($"Map {map.name} requires depth to be specified");
                }

                var mainMap = this.definitions.MainMaps[depth.Value];

                var success = this.GotoMap(mainMap, depth.Value);
                if (!success)
                {
                    throw new Exception($"Failed to go to main map {mainMap.name}");
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
                this.luigiAiData.InvalidateData(DataInvalidationType.WizardActionInvalidation);

                if (
                    lastAction == this.luigiAiData.LastAction
                    || this.luigiAiData.MapType != map.type
                    || (depth.HasValue && -this.luigiAiData.Depth != depth)
                )
                {
                    // Data hasn't updated yet
                    continue;
                }

                // Wait for the UI to finish loading after the map is done
                Thread.Sleep(TimeDuration.MapPostLoadSleep);

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

            Thread.Sleep(TimeDuration.EnterStringSleep);

            // Invalidate all data since tile data should now be present
            this.luigiAiData.InvalidateData(DataInvalidationType.WizardActionInvalidation);
        }
    }
}
