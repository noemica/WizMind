using System.Diagnostics;
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
        /// Enters stairs that we are on top of or 1 tile away from.
        /// </summary>
        /// <param name="direction">
        /// The direction to move to enter the stairs.
        /// If <c>null</c>, the tile Cogmind is currently on.
        /// </param>
        /// <param name="garrisonStairs">
        /// Whether the stairs leaving through are inside a Garrison.
        /// If so, an extra input to leave is required.
        /// </param>
        public void EnterStairs(MovementDirection? direction = null, bool garrisonStairs = false)
        {
            var lastAction = this.luigiAiData.LastAction;

            if (direction == null)
            {
                // Enter stairs underneath us with 2 >s
                this.input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);
                Thread.Sleep(TimeDuration.MapLeaveConfirmationSleep);
                this.input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);

                if (garrisonStairs)
                {
                    // Do the extra > input required to leave inside a Garrison
                    Thread.Sleep(TimeDuration.MapLeaveConfirmationSleep);
                    this.input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);
                }

                // Wait for the map to finish loading
                WaitForMapUpdate(lastAction);

                return;
            }

            // Enter the stairs in the given direction
            var key = DirectionToKey(direction.Value);
            this.input.SendKeystroke(key);
            Thread.Sleep(TimeDuration.MapLeaveConfirmationSleep);
            this.input.SendKeystroke(key);

            if (garrisonStairs)
            {
                // Do the extra > input required to leave inside a Garrison
                Thread.Sleep(TimeDuration.MapLeaveConfirmationSleep);
                this.input.SendKeystroke(Keys.OemPeriod, KeyModifier.Shift);
            }

            WaitForMapUpdate(lastAction);

            return;

            void WaitForMapUpdate(int lastAction)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var lastDepth = this.luigiAiData.Depth;
                var mapType = this.luigiAiData.MapType;

                // Attempt to load the map periodically, it may take several
                // seconds to load. When we have confirmed the new map then
                // it's safe to continue. New map should be accompanied by a
                // new action number and either a new map type or map depth.
                // It's not possible to loop the exact same map to the exact
                // same depth.
                while (stopwatch.ElapsedMilliseconds < TimeDuration.MapLoadTime)
                {
                    Thread.Sleep(TimeDuration.MapLoadSleep);
                    this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);

                    if (
                        lastAction == this.luigiAiData.LastAction
                        || (
                            this.luigiAiData.MapType == mapType
                            && this.luigiAiData.Depth == lastDepth
                        )
                    )
                    {
                        // Data hasn't updated yet
                        continue;
                    }

                    // Wait for the UI to finish loading after the map is done
                    Thread.Sleep(TimeDuration.PostMapLoadSleep);
                    return;
                }

                throw new Exception("Failed to take stairs to new map");
            }
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
