using System.Collections.Generic;

namespace TrickyParcels
{
    [System.Serializable]
    public class TileData
    {
        public TileType type = TileType.Empty;
        public Direction direction = Direction.Right;

        // Sorter only. Each arm now holds a SET of labels (e.g. Blue+Cyan -> Arm A,
        // Orange -> Arm B), not just one. Wildcard is never placed in either set;
        // wildcard packages route dynamically instead (see Package.cs).
        public List<PackageLabel> armALabels = new List<PackageLabel>();
        public List<PackageLabel> armBLabels = new List<PackageLabel>();

        // Chute only: which label this specific chute accepts.
        public PackageLabel chuteLabel = PackageLabel.Blue;

        // Delay only: fixed pause length (simplified from the GDD's
        // short/medium/long cycle to one fixed value per level).
        public float delayDuration = 2f;
    }
}