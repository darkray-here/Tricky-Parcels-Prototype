using System.Collections.Generic;
using UnityEngine;

namespace TrickyParcels
{
    public enum GameState { Idle, Playing, Won, Lost }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Idle;
        public int Delivered { get; private set; }
        public float TimeRemaining { get; private set; }
        public float GraceRemaining => Mathf.Max(0f, _graceRemaining);

        public GameObject levelSelector;
        public GameObject tutorialPause;
        public GameObject timerText;

        private LevelConfig _config;
        private float _levelTime;
        private float _graceRemaining;
        private float _spawnTimer;
        private float _spawnBlockedTimer;
        private int _totalCreated;
        private readonly Queue<PackageLabel> _spawnQueue = new Queue<PackageLabel>();
        private readonly List<BurstEvent> _pendingBursts = new List<BurstEvent>();
        private readonly List<Package> _activePackages = new List<Package>();
        private readonly Dictionary<Vector2Int, List<float>> _chuteDeliveryLog = new Dictionary<Vector2Int, List<float>>();

        void Awake()
        {
            Instance = this;
        }

        public void LoadLevel(LevelConfig config)
        {
            _config = config;
            State = GameState.Playing;
            levelSelector.SetActive(false);
            Delivered = 0;
            TimeRemaining = config.timeCap;
            _levelTime = 0f;
            _graceRemaining = config.startGracePeriod;
            _spawnTimer = 0f;
            _spawnBlockedTimer = 0f;
            _totalCreated = 0;
            _spawnQueue.Clear();
            _chuteDeliveryLog.Clear();

            foreach (var p in _activePackages) if (p != null) Destroy(p.gameObject);
            _activePackages.Clear();

            _pendingBursts.Clear();
            _pendingBursts.AddRange(config.bursts);
        }

        void Update()
        {
            if (State != GameState.Playing) return;
            if (tutorialPause.activeSelf)
            {
                Time.timeScale = 0f;
                timerText.SetActive(false);
            }
            else
            {
                Time.timeScale = 1f;
                timerText.SetActive(true);
            }

            if (_graceRemaining > 0f)
            {
                _graceRemaining -= Time.deltaTime;
                return; // no spawns, no bursts, timer holds at full during grace
            }

            _levelTime += Time.deltaTime;
            TimeRemaining = Mathf.Max(0f, _config.timeCap - _levelTime);
            if (TimeRemaining <= 0f)
            {
                EndGame(false, "Time ran out before the quota was met.");
                return;
            }

            for (int i = _pendingBursts.Count - 1; i >= 0; i--)
            {
                if (_levelTime >= _pendingBursts[i].atTime)
                {
                    foreach (var label in _pendingBursts[i].labels)
                    {
                        _spawnQueue.Enqueue(label);
                        _totalCreated++;
                    }
                    _pendingBursts.RemoveAt(i);
                }
            }

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _config.spawnInterval && _totalCreated < _config.quota)
            {
                _spawnTimer = 0f;
                _spawnQueue.Enqueue(RandomLabel());
                _totalCreated++;
            }

            TryDrainSpawnQueue();
        }

        PackageLabel RandomLabel() => _config.labelPool[Random.Range(0, _config.labelPool.Count)];

        void TryDrainSpawnQueue()
        {
            if (_spawnQueue.Count == 0) return;
            var grid = GridManager.Instance;

            if (IsCellOccupied(grid.SpawnCell, null))
            {
                _spawnBlockedTimer += Time.deltaTime;
                grid.SetJamWarning(grid.SpawnCell, true);
                if (_spawnBlockedTimer >= 3f)
                {
                    EndGame(false, "The line jammed all the way back to the spawn point.");
                }
                return;
            }

            _spawnBlockedTimer = 0f;
            grid.SetJamWarning(grid.SpawnCell, false);

            var label = _spawnQueue.Dequeue();
            var go = new GameObject("Package");
            var pkg = go.AddComponent<Package>();
            pkg.Init(grid.SpawnCell, label);
            _activePackages.Add(pkg);
        }

        public bool IsCellOccupied(Vector2Int cell, Package requester)
        {
            foreach (var p in _activePackages)
            {
                if (p == null || p == requester) continue;
                if (p.currentCell == cell) return true;
            }
            return false;
        }

        public bool HasChuteCapacity(Vector2Int cell)
        {
            if (!_chuteDeliveryLog.TryGetValue(cell, out var log))
            {
                log = new List<float>();
                _chuteDeliveryLog[cell] = log;
            }
            log.RemoveAll(t => Time.time - t > _config.chuteCapacityWindow);
            return log.Count < _config.chuteCapacityMax;
        }

        public void RecordChuteDelivery(Vector2Int cell)
        {
            if (!_chuteDeliveryLog.TryGetValue(cell, out var log))
            {
                log = new List<float>();
                _chuteDeliveryLog[cell] = log;
            }
            log.Add(Time.time);
        }

        public void OnPackageDelivered(Package pkg)
        {
            _activePackages.Remove(pkg);
            Delivered++;
            if (Delivered >= _config.quota)
            {
                EndGame(true, "Quota met. Nice routing.");
            }
        }

        void EndGame(bool won, string message)
        {
            if (State != GameState.Playing) return;
            State = won ? GameState.Won : GameState.Lost;

            int stars = 0;
            if (won)
            {
                int tilesUsed = GridManager.Instance.CountPlacedTiles();
                stars = 1;
                if (tilesUsed <= Mathf.CeilToInt(_config.parTileCount * 1.5f)) stars = 2;
                if (tilesUsed <= _config.parTileCount && !GridManager.Instance.AnyJamOccurred) stars = 3;

                int levelIndex = LevelFlowController.Instance.CurrentLevelIndex;
                LevelProgress.SetStars(levelIndex, stars);
            }

            UIController.Instance?.ShowResult(won, message, stars);
        }
    }
}