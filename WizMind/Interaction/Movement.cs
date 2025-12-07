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
        /// Converts a movement direction to the associated numpad key.
        /// </summary>
        /// <param name="direction">The direction to move.</param>
        /// <returns>A <see cref="Keys"/> indicating which numpad key to press.</returns>
        public static Keys DirectionToKey(MovementDirection direction)
        {
            return direction switch
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
        }

        /// <summary>
        /// Moves in the given direction.
        /// </summary>
        /// <param name="direction">The direction to move in.</param>
        /// <returns><c>true</c> if the move was successful, otherwise <c>false</c>.</returns>
        public bool Move(MovementDirection direction)
        {
            // TODO need to make sure we can actually move into the tile
            var key = DirectionToKey(direction);
            this.input.SendKeystroke(key);

            var lastAction = this.luigiAiData.LastAction;
            this.luigiAiData.InvalidateData(DataInvalidationType.AdvancingAction, true);

            return lastAction != this.luigiAiData.LastAction;
        }

        /// <summary>
        /// Waits 1 turn.
        /// </summary>
        public void Wait()
        {
            this.input.SendKeystroke(Keys.NumPad5);

            this.luigiAiData.InvalidateData(DataInvalidationType.AdvancingAction, true);
        }
    }
}
