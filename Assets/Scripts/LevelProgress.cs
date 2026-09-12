using UnityEngine;

namespace TrickyParcels
{
    public static class LevelProgress
    {
        public static int GetStars(int levelIndex) => PlayerPrefs.GetInt($"tp_stars_{levelIndex}", 0);

        public static void SetStars(int levelIndex, int stars)
        {
            if (stars > GetStars(levelIndex))
            {
                PlayerPrefs.SetInt($"tp_stars_{levelIndex}", stars);
                PlayerPrefs.Save();
            }
        }

        // Level 0 (Tutorial) is always unlocked; each further level unlocks
        // once the previous one has at least 1 star.
        public static bool IsUnlocked(int levelIndex)
        {
            if (levelIndex <= 0) return true;
            return GetStars(levelIndex - 1) > 0;
        }
    }
}
