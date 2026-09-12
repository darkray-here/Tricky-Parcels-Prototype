using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    [System.Serializable]
    public class ChuteSpec
    {
        public Vector2Int cell;
        public PackageLabel label; // Wildcard packages can enter regardless of this

        public ChuteSpec(Vector2Int cell, PackageLabel label)
        {
            this.cell = cell;
            this.label = label;
        }
    }

    [System.Serializable]
    public class BurstEvent
    {
        public float atTime;
        public PackageLabel[] labels;

        public BurstEvent(float atTime, PackageLabel[] labels)
        {
            this.atTime = atTime;
            this.labels = labels;
        }
    }

    // One config drives GridManager + GameManager for a whole level, so
    // "adding a level" is just adding an entry to LevelDatabase.
    [System.Serializable]
    public class LevelConfig
    {
        public string levelName;
        public bool isTutorial;
        public string[] tutorialHints;

        public int width;
        public int height;
        public Vector2Int spawnCell;
        public List<ChuteSpec> chutes;
        public List<PackageLabel> labelPool;

        public int quota;
        public float spawnInterval;
        public float timeCap;

        public int sorterBudget;
        public int delayBudget;
        public int conveyorBudget; // int.MaxValue = unlimited
        public float delayDuration = 2f; // fixed pause length, simplified from the GDD's short/medium/long cycle

        public int chuteCapacityMax = 999;
        public float chuteCapacityWindow = 1f;

        public List<BurstEvent> bursts = new List<BurstEvent>();

        public int parTileCount;
    }
}
