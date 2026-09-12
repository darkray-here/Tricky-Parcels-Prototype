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
            _sr.color = label == PackageLabel.Blue ? GridManager.Instance.blueColor : GridManager.Instance.orangeColor;
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
            else if (tile.type == TileType.Conveyor)
            {
                moveDir = tile.direction;
            }
            else if (tile.type == TileType.Sorter)
            {
                Direction armADir = tile.direction;
                Direction armBDir = tile.direction.RotateClockwise();
                moveDir = (tile.armALabel == label) ? armADir : armBDir;
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
                (nextTile.type == TileType.ChuteBlue && label == PackageLabel.Blue) ||
                (nextTile.type == TileType.ChuteOrange && label == PackageLabel.Orange);

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

            if (nextTile.type == TileType.ChuteBlue || nextTile.type == TileType.ChuteOrange)
            {
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
