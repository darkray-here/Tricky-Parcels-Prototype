using UnityEngine;

namespace TrickyParcels
{
    public enum Direction { Up, Right, Down, Left }

    public enum TileType { Empty, Conveyor, Sorter, Spawn, ChuteBlue, ChuteOrange }

    public enum PackageLabel { Blue, Orange }

    public enum ToolMode { Conveyor, Sorter }

    public static class DirectionUtil
    {
        public static Vector2Int ToOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return new Vector2Int(0, 1);
                case Direction.Right: return new Vector2Int(1, 0);
                case Direction.Down: return new Vector2Int(0, -1);
                case Direction.Left: return new Vector2Int(-1, 0);
                default: return Vector2Int.zero;
            }
        }

        // Used both for rotating a placed conveyor and for finding a Sorter's
        // second output arm (which always sits 90 degrees clockwise of the first).
        public static Direction RotateClockwise(this Direction dir)
        {
            return (Direction)(((int)dir + 1) % 4);
        }

        public static float ToZRotationDegrees(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return 90f;
                case Direction.Right: return 0f;
                case Direction.Down: return -90f;
                case Direction.Left: return 180f;
                default: return 0f;
            }
        }
    }
}
