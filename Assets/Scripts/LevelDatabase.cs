using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    // Levels are pre-populated so they show up in the Inspector.
    // Edit any field directly in the Inspector — no code changes needed.
    public class LevelDatabase : MonoBehaviour
    {
        public static LevelDatabase Instance { get; private set; }

        public List<LevelConfig> levels = new List<LevelConfig>
        {
            // 0: Tutorial
            new LevelConfig
            {
                levelName = "Tutorial",
                isTutorial = true,
                tutorialHints = new[]
                {
                    "Welcome! Packages will pop out from the dark tile. Your job? Build a path so they reach the colored chutes!",
                    "Left-click an empty tile to place a conveyor belt. Click it again to rotate which way it points.",
                    "See those colored squares at the edges? Those are chutes - each one only accepts packages of its matching color.",
                    "Packages come in different colors. You need a SORTER to sort them! Switch to the Sorter tool (middle button) and place one.",
                    "Middle-click a placed sorter to open its popup. Tap a color to assign it: Unassigned -> Arm 1 -> Arm 2.",
                    "The sorter has two arms pointing in different directions. Use the Sorter tool and click the sorter to rotate which way the arms face.",
                    "Right-click any tile to remove it. Deliver 3 packages to pass the tutorial. Good luck!"
                },
                width = 4,
                height = 4,
                spawnCell = new Vector2Int(0, 2),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec { cell = new Vector2Int(3, 0), label = PackageLabel.Blue },
                    new ChuteSpec { cell = new Vector2Int(3, 3), label = PackageLabel.Orange },
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange },
                quota = 3,
                spawnInterval = 4f,
                timeCap = 180f,
                sorterBudget = 1,
                delayBudget = 0,
                conveyorBudget = 8,
                chuteCapacityMax = 999,
                startGracePeriod = 2f,
                parTileCount = 3,
            },

            // 1: "First Sort"
            new LevelConfig
            {
                levelName = "First Sort",
                startGracePeriod = 3f,
                width = 5,
                height = 5,
                spawnCell = new Vector2Int(0, 2),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec { cell = new Vector2Int(4, 0), label = PackageLabel.Blue },
                    new ChuteSpec { cell = new Vector2Int(4, 4), label = PackageLabel.Orange },
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange },
                quota = 5,
                spawnInterval = 2.5f,
                timeCap = 60f,
                sorterBudget = 1,
                delayBudget = 0,
                conveyorBudget = 10,
                chuteCapacityMax = 5,
                parTileCount = 4,
            },

            // 2: "Rush Hour"
            new LevelConfig
            {
                levelName = "Rush Hour",
                startGracePeriod = 2f,
                width = 6,
                height = 6,
                spawnCell = new Vector2Int(0, 3),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec { cell = new Vector2Int(5, 0), label = PackageLabel.Blue },
                    new ChuteSpec { cell = new Vector2Int(5, 3), label = PackageLabel.Orange },
                    new ChuteSpec { cell = new Vector2Int(5, 5), label = PackageLabel.Cyan },
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange, PackageLabel.Cyan },
                quota = 10,
                spawnInterval = 2.5f,
                timeCap = 90f,
                sorterBudget = 2,
                delayBudget = 1,
                conveyorBudget = 12,
                delayDuration = 2.5f,
                chuteCapacityMax = 3,
                chuteCapacityWindow = 5f,
                bursts = new List<BurstEvent>
                {
                    new BurstEvent { atTime = 30f, labels = new[] { PackageLabel.Blue, PackageLabel.Blue, PackageLabel.Blue } },
                    new BurstEvent { atTime = 60f, labels = new[] { PackageLabel.Cyan, PackageLabel.Cyan, PackageLabel.Cyan } },
                },
                parTileCount = 8,
            },

            // 3: "Peak Season"
            new LevelConfig
            {
                levelName = "Peak Season",
                startGracePeriod = 8f,
                width = 7,
                height = 7,
                spawnCell = new Vector2Int(0, 3),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec { cell = new Vector2Int(6, 0), label = PackageLabel.Blue },
                    new ChuteSpec { cell = new Vector2Int(6, 2), label = PackageLabel.Orange },
                    new ChuteSpec { cell = new Vector2Int(6, 4), label = PackageLabel.Cyan },
                    new ChuteSpec { cell = new Vector2Int(6, 6), label = PackageLabel.Purple },
                },
                labelPool = new List<PackageLabel>
                {
                    PackageLabel.Blue, PackageLabel.Orange, PackageLabel.Cyan, PackageLabel.Purple, PackageLabel.Wildcard
                },
                quota = 15,
                spawnInterval = 2f,
                timeCap = 120f,
                sorterBudget = 3,
                delayBudget = 2,
                conveyorBudget = 16,
                delayDuration = 2f,
                chuteCapacityMax = 2,
                chuteCapacityWindow = 5f,
                bursts = new List<BurstEvent>
                {
                    new BurstEvent { atTime = 30f, labels = new[] { PackageLabel.Blue, PackageLabel.Orange } },
                    new BurstEvent { atTime = 60f, labels = new[] { PackageLabel.Cyan, PackageLabel.Purple } },
                    new BurstEvent { atTime = 90f, labels = new[] { PackageLabel.Wildcard, PackageLabel.Wildcard } },
                },
                parTileCount = 15,
            },
        };

        void Awake()
        {
            Instance = this;
        }
    }
}