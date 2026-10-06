using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave設定")]
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private float nextWaveDelay = 2f;

    [Header("出現位置")]
    [SerializeField] private float spawnOffsetX = 1f;
    [SerializeField] private float minSpawnY = -3f;
    [SerializeField] private float maxSpawnY = 3f;

    private int currentWave = 0;
    private bool waitingNextWave = false;

    private Enemy enemy1;
    private Enemy enemy2;

    private void Start()
    {
        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            int totalEnemies =
                totalWaves * 2;

            stageManager.SetTotalEnemies(
                totalEnemies
            );
        }

        SpawnEnemies();
    }

    private void Update()
    {
        CheckWaveEnd();
    }

    private void SpawnEnemies()
    {
        currentWave++;

        Debug.Log(
            "Wave " +
            currentWave +
            " / " +
            totalWaves
        );

        enemy1 = SpawnRandomEnemy();
        enemy2 = SpawnRandomEnemy();

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            stageManager.AddSpawnedEnemy();
            stageManager.AddSpawnedEnemy();
        }
    }

    private Enemy SpawnRandomEnemy()
    {
        if (Camera.main == null)
        {
            return null;
        }

        // 左右どちらから出るかランダム
        bool spawnFromLeft =
            Random.value < 0.5f;

        float spawnX;

        if (spawnFromLeft)
        {
            spawnX =
                Camera.main.ViewportToWorldPoint(
                    new Vector3(0f, 0.5f, 0f)
                ).x - spawnOffsetX;
        }
        else
        {
            spawnX =
                Camera.main.ViewportToWorldPoint(
                    new Vector3(1f, 0.5f, 0f)
                ).x + spawnOffsetX;
        }

        // 高さをランダム
        float spawnY =
            Random.Range(
                minSpawnY,
                maxSpawnY
            );

        Vector3 spawnPosition =
            new Vector3(
                spawnX,
                spawnY,
                0f
            );

        GameObject enemyObject =
            Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

        return enemyObject.GetComponent<Enemy>();
    }

    private void CheckWaveEnd()
    {
        if (waitingNextWave)
        {
            return;
        }

        // 今のWaveの2体が残っている
        if (enemy1 != null || enemy2 != null)
        {
            return;
        }

        // 全Wave終了
        if (currentWave >= totalWaves)
        {
            return;
        }

        StartCoroutine(
            NextWaveCoroutine()
        );
    }

    private System.Collections.IEnumerator NextWaveCoroutine()
    {
        waitingNextWave = true;

        yield return new WaitForSeconds(
            nextWaveDelay
        );

        waitingNextWave = false;

        SpawnEnemies();
    }
}