
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

    [Header("星評価")]
    [SerializeField] private int threeStarKills = 8;
    [SerializeField] private int twoStarKills = 5;

    private bool stageCleared = false;
    private EnemySpawner enemySpawner;
    [Header("敵の種類別撃破数")]
    [SerializeField] private int pufferfishKills = 0;
    [SerializeField] private int jellyfishKills = 0;
    [SerializeField] private int crabKills = 0;
    [Header("敵別撃破数のUI")]
    [SerializeField] private TMP_Text pufferfishCountText;
    [SerializeField] private TMP_Text jellyfishCountText;
    [SerializeField] private TMP_Text crabCountText;
    [Header("クリア画面遷移")]
    [SerializeField] private ClearBubbleTransition clearBubbleTransition;
    private void Start()
    {
        enemySpawner =
            FindObjectOfType<EnemySpawner>();

        if (clearPanel != null)
            clearPanel.SetActive(false);
    }

    private void Update()
    {
        CheckStageClear();
    }

    private void CheckStageClear()
    {
        if (stageCleared)
            return;

        if (enemySpawner == null)
            return;

        // 全Waveのスポーンと撃破が終わるまで待つ
        if (!enemySpawner.IsAllWavesCompleted)
            return;

        // 予定した敵が全員スポーンしたか
        if (spawnedEnemies < totalEnemies)
            return;

        // ハリセンボン
        if (FindObjectsOfType<Enemy>().Length > 0)
            return;

        // クラゲ
        if (FindObjectsOfType<JellyfishBubbleHit>().Length > 0)
            return;

        // カニ
        if (FindObjectsOfType<CrabBubbleHit>().Length > 0)
            return;

        StageClear();
    }

    private void StageClear()
    {
        FreezePlayer();
        stageCleared = true;

        Debug.Log("ステージクリア！");
        Debug.Log("最終撃破数 : " + defeatedEnemies);

        

        int stars = 1;

        if (defeatedEnemies >= threeStarKills)
        {
            stars = 3;
        }
        else if (defeatedEnemies >= twoStarKills)
        {
            stars = 2;
        }

        if (starText != null)
            starText.text = new string('★', stars);

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
        // 敵別撃破数をリザルトUIに反映
        UpdateResultUI();

        // 泡の演出を開始
        if (clearBubbleTransition != null)
        {
            clearBubbleTransition.PlayTransition();
        }
        else if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
    }

    public void RetryStage()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void AddDefeatedEnemy()
    {
        defeatedEnemies++;
        Debug.Log("敵撃破数 : " + defeatedEnemies);
    }


    public void AddDefeatedEnemy(string enemyType)
    {
        // 今回のステージの撃破数
        AddDefeatedEnemy();

        switch (enemyType)
        {
            case "Pufferfish":
                pufferfishKills++;
                break;

            case "Jellyfish":
                jellyfishKills++;
                break;

            case "Crab":
                crabKills++;
                break;

            default:
                Debug.LogWarning(
                    "不明な敵の種類: " + enemyType
                );
                return;
        }

        // ステージをまたいだ累計撃破数を保存
        EnemyKillRecords.RecordKill(enemyType);

        Debug.Log(
            "今回の撃破数 - フグ: " + pufferfishKills +
            " / クラゲ: " + jellyfishKills +
            " / カニ: " + crabKills
        );
    }


    public void AddSpawnedEnemy()
    {
        spawnedEnemies++;

        Debug.Log(
            "敵出現数 : " +
            spawnedEnemies + " / " + totalEnemies
        );
    }

    public void SetTotalEnemies(int amount)
    {
        totalEnemies = amount;
        spawnedEnemies = 0;
        defeatedEnemies = 0;
        pufferfishKills = 0;
        jellyfishKills = 0;
        crabKills = 0;
        stageCleared = false;

        Debug.Log(
            "ステージ敵総数 : " + totalEnemies
        );
    }

    private void UpdateResultUI()
    {
        if (pufferfishCountText != null)
            pufferfishCountText.text = "× " + pufferfishKills;

        if (jellyfishCountText != null)
            jellyfishCountText.text = "× " + jellyfishKills;

        if (crabCountText != null)
            crabCountText.text = "× " + crabKills;
    }
    private void FreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        // プレイヤーの操作を停止
        PlayerMove controller =
            player.GetComponent<PlayerMove>();

        if (controller != null)
            controller.enabled = false;

        // プレイヤーの物理演算を停止
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
    }
}
