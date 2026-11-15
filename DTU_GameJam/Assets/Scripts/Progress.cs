using UnityEngine;

public static class Progress
{
    const string Key = "HighestUnlockedLevel";

    public static int HighestUnlocked => PlayerPrefs.GetInt(Key, 1);   // level 1 is always open

    public static void CompleteLevel(int level)
    {
        int unlocked = Mathf.Max(HighestUnlocked, level + 1);
        PlayerPrefs.SetInt(Key, unlocked);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Key);
    }
}