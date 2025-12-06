using System;
using System.Collections.Generic;
using System.Text;

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

    public class Movement(Input input)
    {
        private readonly Input input = input;

        public void Move(MovementDirection direction)
        {
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
        }

        /// <summary>
        /// Waits 1 turn
        /// </summary>
        public void Wait()
        {
            this.input.SendKeystroke(Keys.NumPad5);
        }
    }
}
