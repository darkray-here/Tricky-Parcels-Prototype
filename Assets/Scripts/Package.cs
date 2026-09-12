using UnityEngine;

namespace TrickyParcels
{
    public class Package : MonoBehaviour
    {
        public PackageLabel label;
        public Vector2Int currentCell;

        public float moveInterval = 0.6f;
        public float jamWarningThreshold = 1.2f;

        private float _moveTimer;
        private float _stuckTimer;
        private SpriteRenderer _sr;
        private Vector3 _moveFrom;
        private Vector3 _moveTo;
        private float _moveLerp = 1f;

        void Awake()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteFactory.Square();
            _sr.sortingOrder = 3;
            transform.localScale = Vector3.one * 0.5f;
        }

        public void Init(Vector2Int startCell, PackageLabel packageLabel)
        {
            currentCell = startCell;
            label = packageLabel;
            transform.position = GridManager.Instance.CellToWorld(startCell);
            _sr.color = LabelColors.Get(label);
        }

        void Update()
        {
            if (_moveLerp < 1f)
            {
                _moveLerp += Time.deltaTime / moveInterval;
                transform.position = Vector3.Lerp(_moveFrom, _moveTo, Mathf.Clamp01(_moveLerp));
            }

            _moveTimer += Time.deltaTime;
            if (_moveTimer < moveInterval) return;

            TryAdvance();
        }

        void TryAdvance()
        {
            var grid = GridManager.Instance;
            var tile = grid.GetTile(currentCell);
            Direction moveDir;

            if (tile.type == TileType.Spawn)
            {
                moveDir = Direction.Right; // spawn sits on the left edge, only sensible way out
            }
            else if (tile.type == TileType.Conveyor || tile.type == TileType.Delay)
            {
                moveDir = tile.direction;
            }
            else if (tile.type == TileType.Sorter)
            {
                Direction armADir = tile.direction;
                Direction armBDir = tile.direction.RotateClockwise();

                if (label == PackageLabel.Wildcard)
                {
                    // Simplified heuristic: steer toward whichever arm's next cell is free.
                    moveDir = GameManager.Instance.IsCellOccupied(currentCell + armADir.ToOffset(), this) ? armBDir : armADir;
                }
                else if (tile.armALabels.Contains(label)) moveDir = armADir;
                else if (tile.armBLabels.Contains(label)) moveDir = armBDir;
                else { MarkStuck(); return; } // this sorter isn't configured for this label yet
            }
            else
            {
                MarkStuck();
                return;
            }

            Vector2Int nextCell = currentCell + moveDir.ToOffset();

            if (!grid.InBounds(nextCell))
            {
                MarkStuck();
                return;
            }

            if (GameManager.Instance.IsCellOccupied(nextCell, this))
            {
                MarkStuck();
                return;
            }

            var nextTile = grid.GetTile(nextCell);
            bool validDestination =
                nextTile.type == TileType.Conveyor ||
                nextTile.type == TileType.Sorter ||
                nextTile.type == TileType.Delay ||
                (nextTile.type == TileType.Chute &&
                 (nextTile.chuteLabel == label || label == PackageLabel.Wildcard) &&
                 GameManager.Instance.HasChuteCapacity(nextCell));

            if (!validDestination)
            {
                MarkStuck();
                return;
            }

            // Advance
            _stuckTimer = 0f;
            grid.SetJamWarning(currentCell, false);
            _moveFrom = transform.position;
            currentCell = nextCell;
            _moveTo = grid.CellToWorld(currentCell);
            _moveLerp = 0f;
            _moveTimer = 0f;

            if (nextTile.type == TileType.Delay)
            {
                _moveTimer = -nextTile.delayDuration; // pause before the next step
            }
            else if (nextTile.type == TileType.Chute)
            {
                GameManager.Instance.RecordChuteDelivery(nextCell);
                GameManager.Instance.OnPackageDelivered(this);
                Destroy(gameObject, moveInterval);
            }
        }

        void MarkStuck()
        {
            _moveTimer = 0f;
            _stuckTimer += moveInterval;
            if (_stuckTimer >= jamWarningThreshold)
            {
                GridManager.Instance.SetJamWarning(currentCell, true);
            }
        }
    }
}