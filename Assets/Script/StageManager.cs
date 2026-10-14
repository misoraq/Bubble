using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TMP_Text starText;

    [Header("ステージの敵")]
    [SerializeField] private int totalEnemies = 0;

    [Header("敵の撃破数")]
    [SerializeField] private int defeatedEnemies = 0;
    private int spawnedEnemies = 0;

    [Header("★評価")]
    [SerializeField] private int threeStarKills = 8;
    [SerializeField] private int twoStarKills = 5;

    private bool stageCleared = false;

    private void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    private void Update()
    {
        CheckStageClear();
    }

    private void CheckStageClear()
    {
        if (stageCleared)
        {
            return;
        }

        // まだ予定数の敵を出していない
        if (spawnedEnemies < totalEnemies)
        {
            return;
        }

        // まだEnemyが残っている
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (enemies.Length > 0)
        {
            return;
        }

        StageClear();
    }


    private void StageClear()
    {
        stageCleared = true;

        Debug.Log("ステージクリア！");
        Debug.Log("最終撃破数 : " + defeatedEnemies);

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        // 撃破数から星評価を決定
        int stars = 1;

        if (defeatedEnemies >= threeStarKills)
        {
            stars = 3;
        }
        else if (defeatedEnemies >= twoStarKills)
        {
            stars = 2;
        }

        // クリア画面に星を表示
        if (starText != null)
        {
            starText.text = new string('★', stars);
        }

        // 星評価を保存して銃スキンを解放
        if (GunSkinManager.Instance != null)
        {
            GunSkinManager.Instance.RegisterClearStars(stars);

            Debug.Log(
                "最高星評価：" +
                GunSkinManager.Instance.GetBestStars()
            );

            Debug.Log(
                "サメ銃 解放：" +
                GunSkinManager.Instance.IsUnlocked(
                    GunSkinManager.SharkGun
                )
            );

            Debug.Log(
                "弓銃 解放：" +
                GunSkinManager.Instance.IsUnlocked(
                    GunSkinManager.BowGun
                )
            );
        }
        else
        {
            Debug.LogWarning(
                "GunSkinManagerが見つかりません"
            );
        }
    }

    public void RetryStage()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
    public void ReturnToTitle()
    {
        // 時間を通常に戻す
        Time.timeScale = 1f;

        // タイトルシーンへ移動
        SceneManager.LoadScene("TitleScene");
    }

    public void AddDefeatedEnemy()
    {
        defeatedEnemies++;

        Debug.Log("敵撃破数 : " + defeatedEnemies);
    }
    public void AddSpawnedEnemy()
    {
        spawnedEnemies++;

        Debug.Log(
            "敵出現数 : " +
            spawnedEnemies +
            " / " +
            totalEnemies
        );
    }
    public void SetTotalEnemies(int amount)
{
    totalEnemies = amount;

    Debug.Log(
        "ステージ敵総数 : " +
        totalEnemies
    );
}
}