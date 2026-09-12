using UnityEngine;
using UnityEngine.UI;

namespace TrickyParcels
{
    // One entry per level slot in the Level Select panel. Wire exactly
    // LevelDatabase.Levels.Count of these in the Inspector, in order.
    [System.Serializable]
    public class LevelSelectSlot
    {
        public Button button;
        public Text nameText;
        public Text starsText;
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
            for (int i = 0; i < levelSlots.Length && i < LevelDatabase.Levels.Count; i++)
            {
                var slot = levelSlots[i];
                var config = LevelDatabase.Levels[i];
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
            for (int i = 0; i < 3; i++) s += i < stars ? "\u2605" : "\u2606";
            return s;
        }
    }
}
