using System;
using System.Collections.Generic;
using System.Text;
using WizMind.Instances;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class ECACountScript : IScript
    {
        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws)
        {
            this.ws = ws;
        }

        public void Run()
        {
            while (true)
            {
                // Add for 100% hacking success
                this.ws.WizardCommands.AttachItem("Architect God Chip A");

                for (var depth = 8; depth >= 1; depth--)
                {
                    // Start by teleporting to the main map
                    var map = this.ws.Definitions.MainMaps[depth];
                    this.ws.WizardCommands.GotoMap(map, depth);

                    this.FindAndEnterGarrison();
                }
            }
        }

        private void FindAndEnterGarrison()
        {
            this.ws.WizardCommands.RevealMap();

            // Find the closest interactive Garrison tile
            var garrisonTile = this.ws.LuigiAiData.AllTiles.First(
                (tile) =>
                    tile.Prop?.Name == "Garrison Access" && tile.Prop?.InteractivePiece == true
            );

            // Teleport to the open side of the Garrison, then open it.
            // The tile the stairs will be on are closed off on 3/4 directions,
            // while all other cardinally adjacent tiles are part of the
            // Garrison access itself
            var tiles = this.ws.LuigiAiData.Tiles;
            MapTile targetTile;
            MovementDirection direction;

            if (tiles[garrisonTile.X - 1, garrisonTile.Y].Prop == null)
            {
                targetTile = tiles[garrisonTile.X - 1, garrisonTile.Y];
                direction = MovementDirection.Right;
            }
            else if (tiles[garrisonTile.X + 1, garrisonTile.Y].Prop == null)
            {
                targetTile = tiles[garrisonTile.X + 1, garrisonTile.Y];
                direction = MovementDirection.Left;
            }
            else if (tiles[garrisonTile.X, garrisonTile.Y - 1].Prop == null)
            {
                targetTile = tiles[garrisonTile.X, garrisonTile.Y - 1];
                direction = MovementDirection.Down;
            }
            else if (tiles[garrisonTile.X, garrisonTile.Y + 1].Prop == null)
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
            this.ws.Input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);
            Thread.Sleep(TimeDuration.MapLeaveConfirmationSleep);
            this.ws.Input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);
        }
    }
}
