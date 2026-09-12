namespace TrickyParcels
{
    [System.Serializable]
    public class TileData
    {
        public TileType type = TileType.Empty;
        public Direction direction = Direction.Right;

        // Sorter only. Arm A points `direction`; Arm B always sits 90 deg
        // clockwise of Arm A. Swapping just swaps which label goes where.
        public PackageLabel armALabel = PackageLabel.Blue;
        public PackageLabel armBLabel = PackageLabel.Orange;
    }
}
