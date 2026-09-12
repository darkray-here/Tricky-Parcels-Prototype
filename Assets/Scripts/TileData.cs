namespace TrickyParcels
{
    [System.Serializable]
    public class TileData
    {
        public TileType type = TileType.Empty;
        public Direction direction = Direction.Right;

        // Sorter only. Arm A points `direction`; Arm B always sits 90 deg
        // clockwise of Arm A. Each arm is assigned one concrete label
        // (never Wildcard) via the popover's cycle buttons.
        public PackageLabel armALabel = PackageLabel.Blue;
        public PackageLabel armBLabel = PackageLabel.Orange;

        // Chute only: which label this specific chute accepts.
        public PackageLabel chuteLabel = PackageLabel.Blue;

        // Delay only: fixed pause length (simplified from the GDD's
        // short/medium/long cycle to one fixed value per level).
        public float delayDuration = 2f;
    }
}
