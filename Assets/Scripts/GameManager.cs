using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrickyParcels
{
    public enum GameState { Playing, Won, Lost }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level 1: First Sort parameters")]
        public int quota = 10;
        public float spawnInterval = 3f;
        public float softTimeCap = 90f;
        public float spawnJamFailDelay = 3f; // jam telegraph before spawn-blocked = fail

        public GameState State { get; private set; } = GameState.Playing;
        public int Delivered { get; private set; } = 0;
        public float TimeRemaining { get; private set; }

        private float _spawnTimer;
        private int _spawned = 0;
        private float _spawnBlockedTimer = 0f;
        private readonly List<Package> _activePackages = new List<Package>();

        void Awake()
        {
            Instance = this;
            TimeRemaining = softTimeCap;
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                EndGame(false, "Time ran out before the quota was met.");
                return;
            }

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval && _spawned < quota)
            {
                TrySpawnPackage();
            }
        }

        void TrySpawnPackage()
        {
            var grid = GridManager.Instance;

            if (IsCellOccupied(grid.spawnCell, null))
            {
                _spawnBlockedTimer += Time.deltaTime;
                grid.SetJamWarning(grid.spawnCell, true);
                if (_spawnBlockedTimer >= spawnJamFailDelay)
                {
                    EndGame(false, "The line jammed all the way back to the spawn point.");
                }
                return;
            }

            _spawnBlockedTimer = 0f;
            grid.SetJamWarning(grid.spawnCell, false);
            _spawnTimer = 0f;
            _spawned++;

            var label = Random.value < 0.5f ? PackageLabel.Blue : PackageLabel.Orange;
            var go = new GameObject($"Package_{_spawned}");
            var pkg = go.AddComponent<Package>();
            pkg.Init(grid.spawnCell, label);
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

        public void OnPackageDelivered(Package pkg)
        {
            _activePackages.Remove(pkg);
            Delivered++;
            if (Delivered >= quota)
            {
                EndGame(true, "Quota met. Nice routing.");
            }
        }

        void EndGame(bool won, string message)
        {
            if (State != GameState.Playing) return;
            State = won ? GameState.Won : GameState.Lost;
            if (UIController.Instance != null) UIController.Instance.ShowResult(won, message);
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
