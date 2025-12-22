namespace WizMind.Utilities
{
    public readonly struct MapPoint(int x, int y)
    {
        public readonly int X = x;
        public readonly int Y = y;

        /// <summary>
        /// Calculates the greatest distance between the 2 points, either the
        /// x or y position.
        /// </summary>
        /// <param name="other">The point to compare.</param>
        /// <returns>The higher difference.</returns>
        public int CalculateMaxDistance(MapPoint other)
        {
            return Math.Max(Math.Abs(this.X - other.X), Math.Abs(this.Y - other.Y));
        }

        /// <summary>
        /// Deconstructs this struct into x/y values.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public void Deconstruct(out int x, out int y)
        {
            x = this.X;
            y = this.Y;
        }
    }
}
