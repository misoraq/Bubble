
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave設定")]
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float nextWaveDelay = 2f;

    [Header("出現位置")]
    [SerializeField] private float spawnOffsetX = 1f;
    [SerializeField] private float minSpawnY = -3f;
    [SerializeField] private float maxSpawnY = 3f;

    private int currentWave = 0;
    private bool waitingNextWave = false;

    // 現在のWaveで出現した敵を管理
    private List<Enemy> currentEnemies = new List<Enemy>();

    private void Start()
    {
        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            int totalEnemies = totalWaves * enemiesPerWave;

            stageManager.SetTotalEnemies(totalEnemies);
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

        Debug.Log("Wave " + currentWave + " / " + totalWaves);

        currentEnemies.Clear();

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Enemy enemy = SpawnRandomEnemy();

            if (enemy != null)
            {
                currentEnemies.Add(enemy);

                if (stageManager != null)
                {
                    stageManager.AddSpawnedEnemy();
                }
            }
        }
    }

    private Enemy SpawnRandomEnemy()
    {
        if (Camera.main == null || enemyPrefab == null)
        {
            return null;
        }

        // 左右どちらから出現するかランダム
        bool spawnFromLeft = Random.value < 0.5f;

        float spawnX;

        if (spawnFromLeft)
        {
            spawnX = Camera.main.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, 0f)
            ).x - spawnOffsetX;
        }
        else
        {
            spawnX = Camera.main.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, 0f)
            ).x + spawnOffsetX;
        }

        // 高さをランダムに設定
        float spawnY = Random.Range(minSpawnY, maxSpawnY);

        Vector3 spawnPosition =
            new Vector3(spawnX, spawnY, 0f);

        GameObject enemyObject = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        return enemyObject.GetComponent<Enemy>();
    }

    private void CheckWaveEnd()
    {
        if (waitingNextWave)
            return;

        // 現在のWaveの敵が1体でも残っていたら待機
        foreach (Enemy enemy in currentEnemies)
        {
            if (enemy != null)
                return;
        }

        // 最終Waveなら次を出現させない
        if (currentWave >= totalWaves)
            return;

        StartCoroutine(NextWaveCoroutine());
    }

    private IEnumerator NextWaveCoroutine()
    {
        waitingNextWave = true;

        yield return new WaitForSeconds(nextWaveDelay);

        waitingNextWave = false;

        SpawnEnemies();
    }
}
