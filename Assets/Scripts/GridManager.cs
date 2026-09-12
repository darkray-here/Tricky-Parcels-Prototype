using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings (Level 1: First Sort)")]
        public int width = 5;
        public int height = 5;
        public float cellSize = 1.2f;

        [Header("Fixed Cells")]
        public Vector2Int spawnCell = new Vector2Int(0, 2);
        public Vector2Int chuteBlueCell = new Vector2Int(4, 0);
        public Vector2Int chuteOrangeCell = new Vector2Int(4, 4);

        [Header("Colors (plain shapes only, no art)")]
        public Color emptyColor = new Color(0.85f, 0.85f, 0.85f);
        public Color conveyorColor = new Color(0.35f, 0.75f, 0.4f);
        public Color sorterColor = new Color(0.9f, 0.75f, 0.2f);
        public Color spawnColor = new Color(0.3f, 0.3f, 0.3f);
        public Color blueColor = new Color(0.25f, 0.5f, 0.95f);
        public Color orangeColor = new Color(0.95f, 0.55f, 0.15f);
        public Color jamWarningColor = Color.red;

        public ToolMode currentTool = ToolMode.Conveyor;

        // UIController subscribes to this to know when to open the sorter popover.
        public static event Action<Vector2Int> OnSorterTileClicked;

        private TileData[,] _grid;
        private SpriteRenderer[,] _visuals;
        private Transform[,] _indicators;
        private readonly Dictionary<Vector2Int, Transform[]> _sorterArmVisuals = new Dictionary<Vector2Int, Transform[]>();
        private bool _sorterPlaced = false;

        void Awake()
        {
            Instance = this;
            BuildGrid();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) HandleClick(false);
            if (Input.GetMouseButtonDown(1)) HandleClick(true);
        }

        void BuildGrid()
        {
            _grid = new TileData[width, height];
            _visuals = new SpriteRenderer[width, height];
            _indicators = new Transform[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    _grid[x, y] = new TileData();

                    var go = new GameObject($"Cell_{x}_{y}");
                    go.transform.SetParent(transform);
                    go.transform.position = CellToWorld(cell);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.Square();
                    sr.sortingOrder = 0;
                    _visuals[x, y] = sr;

                    var indicatorGO = new GameObject("Indicator");
                    indicatorGO.transform.SetParent(go.transform);
                    indicatorGO.transform.localPosition = Vector3.zero;
                    var indSr = indicatorGO.AddComponent<SpriteRenderer>();
                    indSr.sprite = SpriteFactory.Square();
                    indSr.sortingOrder = 1;
                    indSr.enabled = false;
                    _indicators[x, y] = indicatorGO.transform;

                    if (cell == spawnCell) _grid[x, y].type = TileType.Spawn;
                    else if (cell == chuteBlueCell) _grid[x, y].type = TileType.ChuteBlue;
                    else if (cell == chuteOrangeCell) _grid[x, y].type = TileType.ChuteOrange;

                    RefreshVisual(cell);
                }
            }
        }

        void HandleClick(bool isRightClick)
        {
            if (Camera.main == null) return;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector2Int cell = WorldToCell(worldPos);
            if (!InBounds(cell)) return;

            var tile = _grid[cell.x, cell.y];

            if (isRightClick)
            {
                if (tile.type == TileType.Conveyor || tile.type == TileType.Sorter)
                {
                    if (tile.type == TileType.Sorter) _sorterPlaced = false;
                    tile.type = TileType.Empty;
                    RefreshVisual(cell);
                }
                return;
            }

            if (tile.type == TileType.Spawn || tile.type == TileType.ChuteBlue || tile.type == TileType.ChuteOrange)
                return;

            if (tile.type == TileType.Sorter)
            {
                OnSorterTileClicked?.Invoke(cell);
                return;
            }

            if (currentTool == ToolMode.Conveyor)
            {
                if (tile.type == TileType.Empty)
                {
                    tile.type = TileType.Conveyor;
                    tile.direction = Direction.Right;
                }
                else if (tile.type == TileType.Conveyor)
                {
                    tile.direction = tile.direction.RotateClockwise();
                }
                RefreshVisual(cell);
            }
            else // ToolMode.Sorter
            {
                if (tile.type == TileType.Empty && !_sorterPlaced)
                {
                    tile.type = TileType.Sorter;
                    tile.direction = Direction.Right;
                    _sorterPlaced = true;
                    RefreshVisual(cell);
                }
            }
        }

        public void SwapSorterArms(Vector2Int cell)
        {
            var tile = GetTile(cell);
            if (tile == null || tile.type != TileType.Sorter) return;
            (tile.armALabel, tile.armBLabel) = (tile.armBLabel, tile.armALabel);
            RefreshVisual(cell);
        }

        void RefreshVisual(Vector2Int cell)
        {
            var tile = _grid[cell.x, cell.y];
            var sr = _visuals[cell.x, cell.y];
            var indicator = _indicators[cell.x, cell.y];

            sr.color = GetBaseColor(tile.type);

            if (tile.type == TileType.Conveyor)
            {
                indicator.GetComponent<SpriteRenderer>().enabled = true;
                indicator.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.3f, 0.1f);
                indicator.localScale = new Vector3(0.5f, 0.15f, 1f);
                indicator.localRotation = Quaternion.Euler(0, 0, tile.direction.ToZRotationDegrees());
                indicator.localPosition = (Vector3)(Vector2)tile.direction.ToOffset() * 0.3f;
                ClearSorterArms(cell);
            }
            else if (tile.type == TileType.Sorter)
            {
                indicator.GetComponent<SpriteRenderer>().enabled = false;
                DrawSorterArm(cell, tile.direction, tile.armALabel, 0);
                DrawSorterArm(cell, tile.direction.RotateClockwise(), tile.armBLabel, 1);
            }
            else
            {
                indicator.GetComponent<SpriteRenderer>().enabled = false;
                ClearSorterArms(cell);
            }
        }

        void DrawSorterArm(Vector2Int cell, Direction dir, PackageLabel label, int armIndex)
        {
            if (!_sorterArmVisuals.TryGetValue(cell, out var arms))
            {
                arms = new Transform[2];
                _sorterArmVisuals[cell] = arms;
            }
            if (arms[armIndex] == null)
            {
                var go = new GameObject($"Arm_{armIndex}");
                go.transform.SetParent(_visuals[cell.x, cell.y].transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Square();
                sr.sortingOrder = 2;
                go.transform.localScale = Vector3.one * 0.25f;
                arms[armIndex] = go.transform;
            }
            arms[armIndex].localPosition = (Vector3)(Vector2)dir.ToOffset() * 0.35f;
            arms[armIndex].GetComponent<SpriteRenderer>().color = label == PackageLabel.Blue ? blueColor : orangeColor;
        }

        void ClearSorterArms(Vector2Int cell)
        {
            if (_sorterArmVisuals.TryGetValue(cell, out var arms))
            {
                foreach (var a in arms) if (a != null) Destroy(a.gameObject);
                _sorterArmVisuals.Remove(cell);
            }
        }

        // Blinks a cell red (the jam telegraph) without permanently overwriting its base color.
        public void SetJamWarning(Vector2Int cell, bool warning)
        {
            if (!InBounds(cell)) return;
            var sr = _visuals[cell.x, cell.y];
            var tile = _grid[cell.x, cell.y];
            if (warning)
            {
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                sr.color = Color.Lerp(GetBaseColor(tile.type), jamWarningColor, t);
            }
            else
            {
                sr.color = GetBaseColor(tile.type);
            }
        }

        Color GetBaseColor(TileType type)
        {
            switch (type)
            {
                case TileType.Empty: return emptyColor;
                case TileType.Conveyor: return conveyorColor;
                case TileType.Sorter: return sorterColor;
                case TileType.Spawn: return spawnColor;
                case TileType.ChuteBlue: return blueColor;
                case TileType.ChuteOrange: return orangeColor;
                default: return Color.white;
            }
        }

        public TileData GetTile(Vector2Int cell) => InBounds(cell) ? _grid[cell.x, cell.y] : null;

        public bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;

        public Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x * cellSize, cell.y * cellSize, 0);

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.y / cellSize);
            return new Vector2Int(x, y);
        }
    }
}
