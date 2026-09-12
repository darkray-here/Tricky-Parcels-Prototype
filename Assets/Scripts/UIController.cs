using UnityEngine;
using UnityEngine.UI;

namespace TrickyParcels
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("HUD")]
        public Text quotaText;
        public Text timerText;

        [Header("Toolbar")]
        public Button conveyorButton;
        public Button sorterButton;

        [Header("Sorter Popover")]
        public GameObject sorterPopoverPanel;
        public Text sorterPopoverLabel;
        public Button sorterSwapButton;
        public Button sorterCloseButton;

        [Header("Result")]
        public GameObject resultPanel;
        public Text resultText;
        public Button restartButton;

        private Vector2Int _activeSorterCell;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            conveyorButton.onClick.AddListener(() => SetTool(ToolMode.Conveyor));
            sorterButton.onClick.AddListener(() => SetTool(ToolMode.Sorter));
            sorterSwapButton.onClick.AddListener(SwapActiveSorter);
            sorterCloseButton.onClick.AddListener(() => sorterPopoverPanel.SetActive(false));
            restartButton.onClick.AddListener(() => GameManager.Instance.RestartLevel());

            sorterPopoverPanel.SetActive(false);
            resultPanel.SetActive(false);

            GridManager.OnSorterTileClicked += OpenSorterPopover;
            SetTool(ToolMode.Conveyor);
        }

        void OnDestroy()
        {
            GridManager.OnSorterTileClicked -= OpenSorterPopover;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            quotaText.text = $"Delivered: {gm.Delivered} / {gm.quota}";
            timerText.text = $"Time: {Mathf.CeilToInt(Mathf.Max(0, gm.TimeRemaining))}s";
        }

        void SetTool(ToolMode mode)
        {
            GridManager.Instance.currentTool = mode;
            conveyorButton.interactable = mode != ToolMode.Conveyor;
            sorterButton.interactable = mode != ToolMode.Sorter;
        }

        void OpenSorterPopover(Vector2Int cell)
        {
            _activeSorterCell = cell;
            sorterPopoverPanel.SetActive(true);
            RefreshPopoverLabel();
        }

        void SwapActiveSorter()
        {
            GridManager.Instance.SwapSorterArms(_activeSorterCell);
            RefreshPopoverLabel();
        }

        void RefreshPopoverLabel()
        {
            var tile = GridManager.Instance.GetTile(_activeSorterCell);
            if (tile == null) return;
            sorterPopoverLabel.text = $"Arm 1: {tile.armALabel}   Arm 2: {tile.armBLabel}";
        }

        public void ShowResult(bool won, string message)
        {
            resultPanel.SetActive(true);
            resultText.text = (won ? "LEVEL CLEARED\n" : "LEVEL FAILED\n") + message;
        }
    }
}
