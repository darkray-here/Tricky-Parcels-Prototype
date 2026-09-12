using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    // GDD numbers are used where the doc gives an exact figure (grid size, quota,
    // labels, sorter/delay/conveyor budgets, par tile count). Where the doc
    // describes a mechanic without a literal number (capacity window length,
    // exact burst timing), reasonable values are chosen and called out in the
    // setup guide's "assumptions" section.
    public static class LevelDatabase
    {
        public static readonly List<LevelConfig> Levels = new List<LevelConfig>
        {
            // 0: Tutorial
            new LevelConfig
            {
                levelName = "Tutorial",
                isTutorial = true,
                tutorialHints = new[]
                {
                    "Click an empty tile to place a conveyor.",
                    "Click a placed conveyor again to rotate it 90 degrees.",
                    "Route packages from the dark spawn tile toward the sorter.",
                    "Click the placed sorter to assign which label goes to each arm.",
                    "Right-click any tile to clear it. Deliver 3 packages to pass!"
                },
                width = 4,
                height = 4,
                spawnCell = new Vector2Int(0, 2),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec(new Vector2Int(3, 0), PackageLabel.Blue),
                    new ChuteSpec(new Vector2Int(3, 3), PackageLabel.Orange),
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange },
                quota = 3,
                spawnInterval = 4f,
                timeCap = 180f,
                sorterBudget = 1,
                delayBudget = 0,
                conveyorBudget = int.MaxValue,
                chuteCapacityMax = 999,
                parTileCount = 3,
            },

            // 1: "First Sort" (Introductory)
            new LevelConfig
            {
                levelName = "First Sort",
                width = 5,
                height = 5,
                spawnCell = new Vector2Int(0, 2),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec(new Vector2Int(4, 0), PackageLabel.Blue),
                    new ChuteSpec(new Vector2Int(4, 4), PackageLabel.Orange),
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange },
                quota = 10,
                spawnInterval = 3f,
                timeCap = 90f,
                sorterBudget = 1,
                delayBudget = 0,
                conveyorBudget = int.MaxValue,
                chuteCapacityMax = 5, // "never a real constraint" per the GDD
                parTileCount = 4,
            },

            // 2: "Rush Hour" (Intermediate)
            new LevelConfig
            {
                levelName = "Rush Hour",
                width = 6,
                height = 6,
                spawnCell = new Vector2Int(0, 3),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec(new Vector2Int(5, 0), PackageLabel.Blue),
                    new ChuteSpec(new Vector2Int(5, 3), PackageLabel.Orange),
                    new ChuteSpec(new Vector2Int(5, 5), PackageLabel.Green),
                },
                labelPool = new List<PackageLabel> { PackageLabel.Blue, PackageLabel.Orange, PackageLabel.Green },
                quota = 18,
                spawnInterval = 3f,
                timeCap = 120f,
                sorterBudget = 2,
                delayBudget = 1,
                conveyorBudget = 8,
                delayDuration = 2.5f,
                chuteCapacityMax = 3,
                chuteCapacityWindow = 5f,
                bursts = new List<BurstEvent>
                {
                    new BurstEvent(40f, new[] { PackageLabel.Blue, PackageLabel.Blue, PackageLabel.Blue }),
                    new BurstEvent(80f, new[] { PackageLabel.Green, PackageLabel.Green, PackageLabel.Green }),
                },
                parTileCount = 10,
            },

            // 3: "Peak Season" (Advanced)
            new LevelConfig
            {
                levelName = "Peak Season",
                width = 7,
                height = 7,
                spawnCell = new Vector2Int(0, 3),
                chutes = new List<ChuteSpec>
                {
                    new ChuteSpec(new Vector2Int(6, 0), PackageLabel.Blue),
                    new ChuteSpec(new Vector2Int(6, 2), PackageLabel.Orange),
                    new ChuteSpec(new Vector2Int(6, 4), PackageLabel.Green),
                    new ChuteSpec(new Vector2Int(6, 6), PackageLabel.Purple),
                },
                labelPool = new List<PackageLabel>
                {
                    PackageLabel.Blue, PackageLabel.Orange, PackageLabel.Green, PackageLabel.Purple, PackageLabel.Wildcard
                },
                quota = 28,
                spawnInterval = 3f,
                timeCap = 150f,
                sorterBudget = 3,
                delayBudget = 2,
                conveyorBudget = 12,
                delayDuration = 2f,
                chuteCapacityMax = 2,
                chuteCapacityWindow = 5f,
                bursts = new List<BurstEvent>
                {
                    new BurstEvent(50f, new[] { PackageLabel.Blue, PackageLabel.Orange }),
                    new BurstEvent(100f, new[] { PackageLabel.Green, PackageLabel.Purple }),
                },
                parTileCount = 15,
            },
        };
    }
}
