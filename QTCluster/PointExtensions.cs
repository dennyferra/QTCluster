using System.Drawing;

namespace Clustering
{
    /// <summary>
    /// Extension methods for System.Drawing.Point
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        /// Calculates the distance squared between two points.
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        public static int DistanceSquared(this Point p1, Point p2)
        {
            int diffX = p2.X - p1.X;
            int diffY = p2.Y - p1.Y;
            return diffX * diffX + diffY * diffY;
        }
    }
}