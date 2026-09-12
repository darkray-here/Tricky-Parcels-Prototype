using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrickyParcels
{
    // One row per possible label in the sorter popover. Build 4 of these in the
    // Editor (the max non-Wildcard labels any level uses); unused rows are hidden.
    [System.Serializable]
    public class LabelToggleRow
    {
        public GameObject root;
        public Text nameText;
        public Button toggleButton;
        public Text stateText;
    }

    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("Root")]
        public GameObject gameplayRoot;

        [Header("HUD")]
        public Text levelNameText;
        public Text quotaText;
        public Text timerText;
        public Button backToMenuButton;

        [Header("Toolbar")]
        public Button conveyorButton;
        public Button sorterButton;
        public Button delayButton;
        public Text conveyorCountText; // remaining conveyor budget
        public Text sorterCountText;   // remaining sorter budget
        public Text delayCountText;    // remaining delay budget

        [Header("Sorter Popover")]
        public GameObject sorterPopoverPanel;
        public Text sorterPopoverHeaderText;
        public LabelToggleRow[] labelRows; // size 4, one per possible label
        public Button sorterCloseButton;

        [Header("Tutorial")]
        public GameObject tutorialPanel;
        public Text tutorialHintText;
        public Button tutorialNextButton;

        [Header("Result")]
        public GameObject resultPanel;
        public Text resultText;
        public TMP_Text resultStarsText;
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

            for (int i = 0; i < labelRows.Length; i++)
            {
                int rowIndex = i; // capture for closure
                labelRows[i].toggleButton.onClick.AddListener(() => OnLabelToggleClicked(rowIndex));
            }
            sorterCloseButton.onClick.AddListener(() => sorterPopoverPanel.SetActive(false));
            if (sorterPopoverHeaderText != null)
                sorterPopoverHeaderText.text = "Tap a color to move it: Unassigned -> Arm 1 -> Arm 2";

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

            var grid = GridManager.Instance;
            var config = grid.CurrentConfig;

            if (gm.GraceRemaining > 0f)
            {
                quotaText.text = $"Delivered: 0 / {config.quota}";
                timerText.text = $"Starting in {Mathf.CeilToInt(gm.GraceRemaining)}...";
                UpdateTileCounter(config, grid);
                return;
            }

            quotaText.text = $"Delivered: {gm.Delivered} / {config.quota}";
            timerText.text = $"Time: {Mathf.CeilToInt(gm.TimeRemaining)}s";
            UpdateTileCounter(config, grid);
        }

        void UpdateTileCounter(LevelConfig config, GridManager grid)
        {
            if (conveyorCountText != null)
            {
                int left = config.conveyorBudget >= 999 ? 999 : config.conveyorBudget - grid.ConveyorPlaced;
                conveyorCountText.text = left.ToString();
            }
            if (sorterCountText != null)
            {
                int left = config.sorterBudget - grid.SorterPlaced;
                sorterCountText.text = left.ToString();
            }
            if (delayCountText != null)
            {
                int left = config.delayBudget - grid.DelayPlaced;
                delayCountText.text = left.ToString();
            }
        }

        public bool IsBlockingPanelOpen()
        {
            return sorterPopoverPanel.activeSelf || tutorialPanel.activeSelf || resultPanel.activeSelf;
        }

        public void ShowGameplay(LevelConfig config)
        {
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
            RefreshLabelRows();
        }

        void OnLabelToggleClicked(int rowIndex)
        {
            var labels = GridManager.Instance.CurrentConfig.labelPool.FindAll(l => l != PackageLabel.Wildcard);
            if (rowIndex >= labels.Count) return;
            GridManager.Instance.CycleLabelArmAssignment(_activeSorterCell, labels[rowIndex]);
            RefreshLabelRows();
        }

        void RefreshLabelRows()
        {
            var tile = GridManager.Instance.GetTile(_activeSorterCell);
            if (tile == null) return;
            var labels = GridManager.Instance.CurrentConfig.labelPool.FindAll(l => l != PackageLabel.Wildcard);

            for (int i = 0; i < labelRows.Length; i++)
            {
                if (labelRows[i] == null || labelRows[i].root == null)
                {
                    Debug.LogError($"UIController.labelRows[{i}] is not fully wired (root/nameText/toggleButton/stateText).");
                    continue;
                }

                if (i < labels.Count)
                {
                    var label = labels[i];
                    labelRows[i].root.SetActive(true);
                    labelRows[i].nameText.text = label.ToString();

                    string state = tile.armALabels.Contains(label) ? "Arm 1"
                                 : tile.armBLabels.Contains(label) ? "Arm 2"
                                 : "Unassigned";
                    labelRows[i].stateText.text = state;
                }
                else
                {
                    labelRows[i].root.SetActive(false);
                }
            }
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
            for (int i = 0; i < 3; i++) s += i < stars ? "<color=#9B59B6>*</color>" : "<color=#666666>-</color>";
            return s;
        }
    }
}