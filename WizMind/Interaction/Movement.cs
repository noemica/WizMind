using System;
using System.Collections.Generic;
using System.Text;
using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public enum MovementDirection
    {
        DownLeft,
        Down,
        DownRight,
        Left,
        Right,
        UpLeft,
        Up,
        UpRight,
    }

    public class Movement(Input input, LuigiAiData luigiAIData)
    {
        private readonly Input input = input;

        private readonly LuigiAiData luigiAiData = luigiAIData;

        /// <summary>
        /// Moves in the given direction.
        /// </summary>
        /// <param name="direction">The direction to move in.</param>
        /// <returns><c>true</c> if the move was successful, otherwise <c>false</c>.</returns>
        public bool Move(MovementDirection direction)
        {
            // TODO need to make sure we can actually move into the tile
            var key = direction switch
            {
                MovementDirection.DownLeft => Keys.NumPad1,
                MovementDirection.Down => Keys.NumPad2,
                MovementDirection.DownRight => Keys.NumPad3,
                MovementDirection.Left => Keys.NumPad4,
                MovementDirection.Right => Keys.NumPad6,
                MovementDirection.UpLeft => Keys.NumPad7,
                MovementDirection.Up => Keys.NumPad8,
                MovementDirection.UpRight => Keys.NumPad9,
                _ => throw new Exception($"Invalid movement direction {direction}"),
            };

            this.input.SendKeystroke(key);

            var lastAction = this.luigiAiData.LastAction;
            this.luigiAiData.InvalidateData(DataInvalidationType.GameActionInvalidation, true);

            return lastAction != this.luigiAiData.LastAction;
        }

        /// <summary>
        /// Waits 1 turn.
        /// </summary>
        public void Wait()
        {
            this.input.SendKeystroke(Keys.NumPad5);

            this.luigiAiData.InvalidateData(DataInvalidationType.GameActionInvalidation, true);
        }
    }
}
