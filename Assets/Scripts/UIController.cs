using UnityEngine;
using UnityEngine.UI;

namespace TrickyParcels
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("Root")]
        public GameObject gameplayRoot; // parent of HUD + toolbar + grid camera view

        [Header("Level Select Panel")]
        public GameObject levelSelectPanel;

        [Header("HUD")]
        public Text levelNameText;
        public Text quotaText;
        public Text timerText;
        public Button backToMenuButton;

        [Header("Toolbar")]
        public Button conveyorButton;
        public Button sorterButton;
        public Button delayButton;

        [Header("Sorter Popover")]
        public GameObject sorterPopoverPanel;
        public Text sorterPopoverLabel;
        public Button sorterArmACycleButton;
        public Button sorterArmBCycleButton;
        public Button sorterCloseButton;

        [Header("Tutorial")]
        public GameObject tutorialPanel;
        public Text tutorialHintText;
        public Button tutorialNextButton;

        [Header("Result")]
        public GameObject resultPanel;
        public Text resultText;
        public Text resultStarsText;
        public Button nextLevelButton;
        public Button retryButton;
        public Button levelSelectButton;

        private Vector2Int _activeSorterCell;
        private string[] _tutorialHints;
        private int _tutorialIndex;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            conveyorButton.onClick.AddListener(() => SetTool(ToolMode.Conveyor));
            sorterButton.onClick.AddListener(() => SetTool(ToolMode.Sorter));
            delayButton.onClick.AddListener(() => SetTool(ToolMode.Delay));

            sorterArmACycleButton.onClick.AddListener(() => { GridManager.Instance.CycleSorterArm(_activeSorterCell, true); RefreshPopoverLabel(); });
            sorterArmBCycleButton.onClick.AddListener(() => { GridManager.Instance.CycleSorterArm(_activeSorterCell, false); RefreshPopoverLabel(); });
            sorterCloseButton.onClick.AddListener(() => sorterPopoverPanel.SetActive(false));

            tutorialNextButton.onClick.AddListener(AdvanceTutorial);

            retryButton.onClick.AddListener(() => LevelFlowController.Instance.RestartCurrentLevel());
            nextLevelButton.onClick.AddListener(() => LevelFlowController.Instance.GoToNextLevel());
            levelSelectButton.onClick.AddListener(() => LevelFlowController.Instance.GoToLevelSelect());
            backToMenuButton.onClick.AddListener(() => LevelFlowController.Instance.GoToLevelSelect());

            sorterPopoverPanel.SetActive(false);
            resultPanel.SetActive(false);
            tutorialPanel.SetActive(false);
            gameplayRoot.SetActive(false);

            GridManager.OnSorterTileClicked += OpenSorterPopover;
        }

        void OnDestroy()
        {
            GridManager.OnSorterTileClicked -= OpenSorterPopover;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            quotaText.text = $"Delivered: {gm.Delivered} / {GridManager.Instance.CurrentConfig.quota}";
            timerText.text = $"Time: {Mathf.CeilToInt(gm.TimeRemaining)}s";
        }

        // GridManager checks this every frame before accepting a click, so no
        // panel has to manually lock/unlock input on open and close.
        public bool IsBlockingPanelOpen()
        {
            return sorterPopoverPanel.activeSelf || tutorialPanel.activeSelf || resultPanel.activeSelf;
        }

        public void ShowGameplay(LevelConfig config)
        {
            levelSelectPanel.SetActive(false);
            gameplayRoot.SetActive(true);
            resultPanel.SetActive(false);
            levelNameText.text = config.levelName;
            SetTool(ToolMode.Conveyor);
            delayButton.gameObject.SetActive(config.delayBudget > 0);

            if (config.isTutorial && config.tutorialHints != null && config.tutorialHints.Length > 0)
            {
                _tutorialHints = config.tutorialHints;
                _tutorialIndex = 0;
                tutorialPanel.SetActive(true);
                tutorialHintText.text = _tutorialHints[0];
            }
            else
            {
                tutorialPanel.SetActive(false);
            }
        }

        public void HideGameplay()
        {
            gameplayRoot.SetActive(false);
            resultPanel.SetActive(false);
            tutorialPanel.SetActive(false);
        }

        void AdvanceTutorial()
        {
            _tutorialIndex++;
            if (_tutorialHints == null || _tutorialIndex >= _tutorialHints.Length)
            {
                tutorialPanel.SetActive(false);
            }
            else
            {
                tutorialHintText.text = _tutorialHints[_tutorialIndex];
            }
        }

        void SetTool(ToolMode mode)
        {
            GridManager.Instance.currentTool = mode;
            conveyorButton.interactable = mode != ToolMode.Conveyor;
            sorterButton.interactable = mode != ToolMode.Sorter;
            delayButton.interactable = mode != ToolMode.Delay;
        }

        void OpenSorterPopover(Vector2Int cell)
        {
            _activeSorterCell = cell;
            sorterPopoverPanel.SetActive(true);
            RefreshPopoverLabel();
        }

        void RefreshPopoverLabel()
        {
            var tile = GridManager.Instance.GetTile(_activeSorterCell);
            if (tile == null) return;
            sorterPopoverLabel.text = $"Arm 1: {tile.armALabel}   Arm 2: {tile.armBLabel}\n(tap a cycle button to change)";
        }

        public void ShowResult(bool won, string message, int stars)
        {
            resultPanel.SetActive(true);
            tutorialPanel.SetActive(false);
            resultText.text = (won ? "LEVEL CLEARED\n" : "LEVEL FAILED\n") + message;
            resultStarsText.text = won ? BuildStarString(stars) : "";
            nextLevelButton.gameObject.SetActive(won);
        }

        string BuildStarString(int stars)
        {
            string s = "";
            for (int i = 0; i < 3; i++) s += i < stars ? "\u2605" : "\u2606"; // filled/empty star
            return s;
        }
    }
}