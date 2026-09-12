using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    [System.Serializable]
    public class ChuteSpec
    {
        public Vector2Int cell;
        public PackageLabel label;
    }

    [System.Serializable]
    public class BurstEvent
    {
        public float atTime;
        public PackageLabel[] labels;
    }

    [System.Serializable]
    public class LevelConfig
    {
        [Header("Info")]
        public string levelName;
        public bool isTutorial;
        [TextArea] public string[] tutorialHints;

        [Header("Grid")]
        public int width;
        public int height;
        public Vector2Int spawnCell;
        public Direction spawnDirection = Direction.Right;

        [Header("Chutes")]
        public List<ChuteSpec> chutes;

        [Header("Packages")]
        public List<PackageLabel> labelPool;
        public int quota;
        public float spawnInterval;
        public float timeCap;

        [Header("Budgets")]
        public int sorterBudget;
        public int delayBudget;
        public int conveyorBudget = 999;
        public float delayDuration = 2f;

        [Header("Chute Capacity")]
        public int chuteCapacityMax = 999;
        public float chuteCapacityWindow = 1f;

        [Header("Bursts")]
        public List<BurstEvent> bursts = new List<BurstEvent>();

        [Header("Star Rating")]
        public int parTileCount;

        [Header("Grace")]
        public float startGracePeriod = 3f;
    }
}