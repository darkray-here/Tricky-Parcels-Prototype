using UnityEngine;

namespace TrickyParcels
{
    public enum Direction { Up, Right, Down, Left }

    // Generic now: which specific chute a Chute cell delivers to is stored on
    // TileData.chuteLabel, not as separate enum values.
    public enum TileType { Empty, Conveyor, Sorter, Delay, Spawn, Chute }

    // Wildcard is a package that can enter ANY chute (Level 3 "Peak Season").
    public enum PackageLabel { Blue, Orange, Green, Purple, Wildcard }

    public enum ToolMode { Conveyor, Sorter, Delay }

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

        // Also used to find a Sorter's second output arm: always 90 deg
        // clockwise of its first arm.
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

    public static class LabelColors
    {
        public static Color Get(PackageLabel label)
        {
            switch (label)
            {
                case PackageLabel.Blue: return new Color(0.25f, 0.5f, 0.95f);
                case PackageLabel.Orange: return new Color(0.95f, 0.55f, 0.15f);
                case PackageLabel.Green: return new Color(0.35f, 0.75f, 0.35f);
                case PackageLabel.Purple: return new Color(0.6f, 0.35f, 0.85f);
                case PackageLabel.Wildcard: return new Color(0.8f, 0.8f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
