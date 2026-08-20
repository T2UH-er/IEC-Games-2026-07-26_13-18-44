using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quản lý dữ liệu người chơi (Player Data) sử dụng PlayerPrefs.
/// </summary>
public static class PlayerDataManager
{
    private const string KEY_HIGHEST_UNLOCKED_LEVEL = "HighestUnlockedLevel";

    /// <summary>
    /// Lấy màn chơi cao nhất đã mở khóa (mặc định là Level 1 nếu chưa lưu).
    /// </summary>
    public static int GetHighestUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KEY_HIGHEST_UNLOCKED_LEVEL, 1);
    }

    /// <summary>
    /// Lưu màn chơi cao nhất khi người chơi hoàn thành 1 level.
    /// Ví dụ: Thắng level 7 -> mở khóa và lưu level 8.
    /// Nếu quay lại chơi level 1 và thắng -> 2 <= 8 nên giữ nguyên 8.
    /// </summary>
    /// <param name="completedLevel">Level người chơi vừa chiến thắng</param>
    public static void SaveHighestUnlockedLevel(int completedLevel)
    {
        int nextLevel = completedLevel + 1;
        int currentHighest = GetHighestUnlockedLevel();

        if (nextLevel > currentHighest)
        {
            PlayerPrefs.SetInt(KEY_HIGHEST_UNLOCKED_LEVEL, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerDataManager] Chúc mừng! Đã mở khóa và lưu màn chơi mới nhất: Level {nextLevel}");
        }
        else
        {
            Debug.Log($"[PlayerDataManager] Hoàn thành Level {completedLevel}. Màn cao nhất hiện tại vẫn là Level {currentHighest} (không đổi).");
        }
    }

    /// <summary>
    /// Xóa / Reset dữ liệu tiến trình màn chơi về mặc định (Level 1).
    /// </summary>
    public static void ResetData()
    {
        PlayerPrefs.DeleteKey(KEY_HIGHEST_UNLOCKED_LEVEL);
        PlayerPrefs.Save();
        Debug.Log("[PlayerDataManager] Đã reset tiến trình màn chơi về Level 1.");
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Player Data/Reset Highest Level Progress")]
    public static void ResetProgressMenu()
    {
        ResetData();
    }
#endif
}
