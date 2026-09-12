using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Colors (plain shapes only, no art)")]
        public Color emptyColor = new Color(0.85f, 0.85f, 0.85f);
        public Color conveyorColor = new Color(0.35f, 0.75f, 0.4f);
        public Color sorterColor = new Color(0.9f, 0.75f, 0.2f);
        public Color delayColor = new Color(0.5f, 0.55f, 0.85f);
        public Color spawnColor = new Color(0.3f, 0.3f, 0.3f);
        public Color jamWarningColor = Color.red;

        public ToolMode currentTool = ToolMode.Conveyor;

        // UIController subscribes to know when to open the sorter popover.
        public static event Action<Vector2Int> OnSorterTileClicked;

        public LevelConfig CurrentConfig { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2Int SpawnCell { get; private set; }
        public bool AnyJamOccurred { get; private set; }

        private TileData[,] _grid;
        private SpriteRenderer[,] _visuals;
        private Transform[,] _indicators;
        private readonly Dictionary<Vector2Int, Transform[]> _sorterArmVisuals = new Dictionary<Vector2Int, Transform[]>();

        private int _sorterPlaced, _delayPlaced, _conveyorPlaced;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (CurrentConfig == null) return;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            if (UIController.Instance != null && UIController.Instance.IsBlockingPanelOpen()) return;
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0)) HandleClick(false);
            if (Input.GetMouseButtonDown(1)) HandleClick(true);
        }

        public void LoadLevel(LevelConfig config)
        {
            CurrentConfig = config;
            Width = config.width;
            Height = config.height;
            SpawnCell = config.spawnCell;
            AnyJamOccurred = false;
            _sorterPlaced = 0;
            _delayPlaced = 0;
            _conveyorPlaced = 0;
            _sorterArmVisuals.Clear();

            foreach (Transform child in transform) Destroy(child.gameObject);

            _grid = new TileData[Width, Height];
            _visuals = new SpriteRenderer[Width, Height];
            _indicators = new Transform[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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

                    if (cell == config.spawnCell) _grid[x, y].type = TileType.Spawn;
                }
            }

            foreach (var chute in config.chutes)
            {
                var t = _grid[chute.cell.x, chute.cell.y];
                t.type = TileType.Chute;
                t.chuteLabel = chute.label;
            }

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    RefreshVisual(new Vector2Int(x, y));
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
                if (tile.type == TileType.Conveyor) { _conveyorPlaced--; tile.type = TileType.Empty; RefreshVisual(cell); }
                else if (tile.type == TileType.Sorter) { _sorterPlaced--; tile.type = TileType.Empty; RefreshVisual(cell); }
                else if (tile.type == TileType.Delay) { _delayPlaced--; tile.type = TileType.Empty; RefreshVisual(cell); }
                return;
            }

            if (tile.type == TileType.Spawn || tile.type == TileType.Chute) return;

            if (tile.type == TileType.Sorter)
            {
                OnSorterTileClicked?.Invoke(cell);
                return;
            }

            switch (currentTool)
            {
                case ToolMode.Conveyor:
                    if (tile.type == TileType.Empty && _conveyorPlaced < CurrentConfig.conveyorBudget)
                    {
                        tile.type = TileType.Conveyor;
                        tile.direction = Direction.Right;
                        _conveyorPlaced++;
                    }
                    else if (tile.type == TileType.Conveyor)
                    {
                        tile.direction = tile.direction.RotateClockwise();
                    }
                    RefreshVisual(cell);
                    break;

                case ToolMode.Sorter:
                    if (tile.type == TileType.Empty && _sorterPlaced < CurrentConfig.sorterBudget)
                    {
                        tile.type = TileType.Sorter;
                        tile.direction = Direction.Right;
                        var labels = CurrentConfig.labelPool.FindAll(l => l != PackageLabel.Wildcard);
                        tile.armALabel = labels.Count > 0 ? labels[0] : PackageLabel.Blue;
                        tile.armBLabel = labels.Count > 1 ? labels[1] : tile.armALabel;
                        _sorterPlaced++;
                        RefreshVisual(cell);
                    }
                    break;

                case ToolMode.Delay:
                    if (tile.type == TileType.Empty && _delayPlaced < CurrentConfig.delayBudget)
                    {
                        tile.type = TileType.Delay;
                        tile.direction = Direction.Right;
                        tile.delayDuration = CurrentConfig.delayDuration;
                        _delayPlaced++;
                    }
                    else if (tile.type == TileType.Delay)
                    {
                        tile.direction = tile.direction.RotateClockwise();
                    }
                    RefreshVisual(cell);
                    break;
            }
        }

        // Cycles one sorter arm to the next non-Wildcard label in the level's pool.
        public void CycleSorterArm(Vector2Int cell, bool armA)
        {
            var tile = GetTile(cell);
            if (tile == null || tile.type != TileType.Sorter) return;
            var labels = CurrentConfig.labelPool.FindAll(l => l != PackageLabel.Wildcard);
            if (labels.Count == 0) return;

            PackageLabel current = armA ? tile.armALabel : tile.armBLabel;
            int idx = labels.IndexOf(current);
            PackageLabel next = labels[(idx + 1) % labels.Count];
            if (armA) tile.armALabel = next; else tile.armBLabel = next;
            RefreshVisual(cell);
        }

        void RefreshVisual(Vector2Int cell)
        {
            var tile = _grid[cell.x, cell.y];
            var sr = _visuals[cell.x, cell.y];
            var indicator = _indicators[cell.x, cell.y];

            sr.color = GetBaseColor(tile);

            if (tile.type == TileType.Conveyor || tile.type == TileType.Delay)
            {
                indicator.GetComponent<SpriteRenderer>().enabled = true;
                indicator.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.15f, 0.1f);
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
            arms[armIndex].GetComponent<SpriteRenderer>().color = LabelColors.Get(label);
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
                AnyJamOccurred = true;
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                sr.color = Color.Lerp(GetBaseColor(tile), jamWarningColor, t);
            }
            else
            {
                sr.color = GetBaseColor(tile);
            }
        }

        Color GetBaseColor(TileData tile)
        {
            switch (tile.type)
            {
                case TileType.Empty: return emptyColor;
                case TileType.Conveyor: return conveyorColor;
                case TileType.Sorter: return sorterColor;
                case TileType.Delay: return delayColor;
                case TileType.Spawn: return spawnColor;
                case TileType.Chute: return LabelColors.Get(tile.chuteLabel);
                default: return Color.white;
            }
        }

        public int CountPlacedTiles() => _conveyorPlaced + _sorterPlaced + _delayPlaced;

        public TileData GetTile(Vector2Int cell) => InBounds(cell) ? _grid[cell.x, cell.y] : null;

        public bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

        public Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x * 1.2f, cell.y * 1.2f, 0);

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / 1.2f);
            int y = Mathf.RoundToInt(worldPos.y / 1.2f);
            return new Vector2Int(x, y);
        }
    }
}