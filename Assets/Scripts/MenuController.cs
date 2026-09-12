using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrickyParcels
{
    // One entry per level slot in the Level Select panel. Wire exactly
    // LevelDatabase.Levels.Count of these in the Inspector, in order.
    [System.Serializable]
    public class LevelSelectSlot
    {
        public Button button;
        public Text nameText;
        public TMP_Text starsText;
    }

    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject levelSelectPanel;

        [Header("Main Menu")]
        public Button playButton;

        [Header("Level Select (one slot per level, in order)")]
        public LevelSelectSlot[] levelSlots;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            playButton.onClick.AddListener(ShowLevelSelect);

            for (int i = 0; i < levelSlots.Length; i++)
            {
                int levelIndex = i; // capture for the closure
                levelSlots[i].button.onClick.AddListener(() => LevelFlowController.Instance.PlayLevel(levelIndex));
            }

            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            levelSelectPanel.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            mainMenuPanel.SetActive(false);
            levelSelectPanel.SetActive(true);
            RefreshLevelSlots();
        }

        public void HideLevelSelect()
        {
            levelSelectPanel.SetActive(false);
        }

        void RefreshLevelSlots()
        {
            var levels = LevelDatabase.Instance.levels;
            for (int i = 0; i < levelSlots.Length && i < levels.Count; i++)
            {
                var slot = levelSlots[i];
                var config = levels[i];
                bool unlocked = LevelProgress.IsUnlocked(i);

                slot.button.interactable = unlocked;
                slot.nameText.text = config.levelName;
                int stars = LevelProgress.GetStars(i);
                slot.starsText.text = unlocked ? BuildStarString(stars) : "LOCKED";
            }
        }

        string BuildStarString(int stars)
        {
            string s = "";
            for (int i = 0; i < 3; i++) s += i < stars ? "<color=#9B59B6>*</color>" : "<color=#666666>-</color>";
            return s;
        }
    }
}
