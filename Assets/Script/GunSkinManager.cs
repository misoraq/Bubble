
using UnityEngine;

public class GunSkinManager : MonoBehaviour
{
    public static GunSkinManager Instance { get; private set; }

    public const int NormalGun = 0;
    public const int SharkGun = 1;
    public const int BowGun = 2;

    private const string BestStarsKey = "BestClearStars";
    private const string EquippedSkinKey = "EquippedGunSkin";
    public const int PufferGun = 3;
    public const int JellyfishGun = 4;
    public const int CrabGun = 5;
    [ContextMenu("テスト用：スキン解放リセット")]
    private void ResetSkinUnlockTest()
    {
        PlayerPrefs.SetInt("BestClearStars", 0);
        PlayerPrefs.SetInt("EquippedGunSkin", 0);
        PlayerPrefs.Save();

        Debug.Log("スキン解放データをリセットしました");
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 過去最高の星評価を取得
    public int GetBestStars()
    {
        return PlayerPrefs.GetInt(BestStarsKey, 0);
    }

    // ステージクリア時に呼び出す
    public void RegisterClearStars(int stars)
    {
        stars = Mathf.Clamp(stars, 1, 3);

        if (stars > GetBestStars())
        {
            PlayerPrefs.SetInt(BestStarsKey, stars);
            PlayerPrefs.Save();
        }
    }

    // スキンが解放されているか
    public bool IsUnlocked(int skinId)
    {
        switch (skinId)
        {
            case NormalGun:
                return true;

            case SharkGun:
                return GetBestStars() >= 2;

            case BowGun:
                return GetBestStars() >= 3;

            case PufferGun:
                return EnemyKillRecords.GetKills("Pufferfish") >= 20;

            case JellyfishGun:
                return EnemyKillRecords.GetKills("Jellyfish") >= 20;

            case CrabGun:
                return EnemyKillRecords.GetKills("Crab") >= 15;

            default:
                return false;
        }
    }

    // スキンを装備
    public bool EquipSkin(int skinId)
    {
        // 未完成のスキン4〜6は装備禁止
        if (skinId == PufferGun ||
            skinId == JellyfishGun ||
            skinId == CrabGun)
        {
            return false;
        }

        if (!IsUnlocked(skinId))
            return false;

        PlayerPrefs.SetInt(EquippedSkinKey, skinId);
        PlayerPrefs.Save();

        return true;
    }

    // 現在装備しているスキン
    public int GetEquippedSkin()
    {
        int skinId = PlayerPrefs.GetInt(EquippedSkinKey, NormalGun);

        return IsUnlocked(skinId) ? skinId : NormalGun;
    }
}
