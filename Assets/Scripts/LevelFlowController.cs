namespace TrickyParcels
{
    using UnityEngine;

    public class LevelFlowController : MonoBehaviour
    {
        public static LevelFlowController Instance { get; private set; }

        public int CurrentLevelIndex { get; private set; } = -1;

        void Awake()
        {
            Instance = this;
        }

        public void PlayLevel(int index)
        {
            if (index < 0 || index >= LevelDatabase.Levels.Count) return;
            CurrentLevelIndex = index;
            var config = LevelDatabase.Levels[index];

            GridManager.Instance.LoadLevel(config);
            GameManager.Instance.LoadLevel(config);
            UIController.Instance.ShowGameplay(config);
        }

        public void RestartCurrentLevel() => PlayLevel(CurrentLevelIndex);

        public void GoToNextLevel()
        {
            int next = CurrentLevelIndex + 1;
            if (next < LevelDatabase.Levels.Count)
                PlayLevel(next);
            else
                GoToLevelSelect();
        }

        public void GoToLevelSelect()
        {
            UIController.Instance.HideGameplay();
            MenuController.Instance.ShowLevelSelect();
        }

        public void GoToMainMenu()
        {
            UIController.Instance.HideGameplay();
            MenuController.Instance.ShowMainMenu();
        }
    }
}
