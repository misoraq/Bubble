using UnityEngine;

public static class EnemyKillRecords
{
    private const string Prefix = "Bubble.TotalKills.";

    public static int GetKills(string enemyType)
    {
        if (!IsSupportedType(enemyType)) return 0;
        return PlayerPrefs.GetInt(Prefix + enemyType, 0);
    }

    public static void RecordKill(string enemyType)
    {
        if (!IsSupportedType(enemyType))
        {
            Debug.LogWarning("Unknown enemy type for kill record: " + enemyType);
            return;
        }

        int total = GetKills(enemyType) + 1;
        PlayerPrefs.SetInt(Prefix + enemyType, total);
        PlayerPrefs.Save();
        Debug.Log("Cumulative " + enemyType + " kills: " + total);
    }

    // スキンの初期化時に使用する。その他のPlayerPrefsは消さない。
    public static void ResetAllKills()
    {
        PlayerPrefs.DeleteKey(Prefix + "Pufferfish");
        PlayerPrefs.DeleteKey(Prefix + "Jellyfish");
        PlayerPrefs.DeleteKey(Prefix + "Crab");
    }

    private static bool IsSupportedType(string enemyType)
    {
        return enemyType == "Pufferfish" ||
               enemyType == "Jellyfish" ||
               enemyType == "Crab";
    }
}
