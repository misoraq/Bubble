
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject jellyfishPrefab;
    [SerializeField] private GameObject crabPrefab;

    [Header("Wave設定")]
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float nextWaveDelay = 2f;
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("空中の敵の出現位置")]
    [SerializeField] private float spawnOffsetX = 1f;
    [SerializeField] private float minSpawnY = -1f;
    [SerializeField] private float maxSpawnY = 3f;

    [Header("カニの出現位置")]
    [SerializeField] private float crabSpawnY = -2.5f;
    [SerializeField] private float crabEdgeMargin = 1f;

    [Header("敵出現警告")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float warningBlinkInterval = 0.15f;
    [SerializeField] private float warningEdgeMargin = 0.8f;
    [Header("重なり防止")]
    [SerializeField] private float minimumSpawnDistance = 2.5f;
    [SerializeField] private int positionSearchAttempts = 30;

    private int currentWave;
    private bool isSpawning;
    private bool waitingNextWave;

    private readonly List<GameObject> currentEnemies =
        new List<GameObject>();

    private readonly List<Vector3> reservedPositions =
        new List<Vector3>();

    public bool IsAllWavesCompleted
    {
        get
        {
            return currentWave >= totalWaves &&
                   !isSpawning &&
                   !waitingNextWave &&
                   GetRemainingEnemyCount() == 0;
        }
    }

    private void Start()
    {
        totalWaves = Mathf.Max(1, totalWaves);
        enemiesPerWave = Mathf.Max(1, enemiesPerWave);

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            stageManager.SetTotalEnemies(
                totalWaves * enemiesPerWave
            );
        }

        StartCoroutine(SpawnWaveRoutine());
    }

    private void Update()
    {
        if (isSpawning || waitingNextWave)
            return;

        if (GetRemainingEnemyCount() > 0)
            return;

        if (currentWave >= totalWaves)
            return;

        StartCoroutine(NextWaveRoutine());
    }

    private int GetRemainingEnemyCount()
    {
        currentEnemies.RemoveAll(enemy => enemy == null);
        return currentEnemies.Count;
    }

    private IEnumerator NextWaveRoutine()
    {
        waitingNextWave = true;

        yield return new WaitForSeconds(nextWaveDelay);

        waitingNextWave = false;
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;
        currentWave++;

        currentEnemies.Clear();
        reservedPositions.Clear();

        Debug.Log(
            "Wave " + currentWave + " / " + totalWaves
        );

        List<GameObject> prefabs = GetAvailablePrefabs();

        if (prefabs.Count == 0)
        {
            Debug.LogError(
                "EnemySpawner: 敵Prefabが未設定です。"
            );

            isSpawning = false;
            yield break;
        }

        // 各種類をなるべく均等に出す
        List<GameObject> spawnOrder =
            new List<GameObject>();

        while (spawnOrder.Count < enemiesPerWave)
        {
            List<GameObject> shuffled =
                new List<GameObject>(prefabs);

            Shuffle(shuffled);

            foreach (GameObject prefab in shuffled)
            {
                if (spawnOrder.Count >= enemiesPerWave)
                    break;

                spawnOrder.Add(prefab);
            }
        }

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        foreach (GameObject prefab in spawnOrder)
        {
            Vector3 spawnPosition;

            // 安全な出現位置が見つかるまで待つ
            while (!TryFindSpawnPosition(
                prefab, out spawnPosition))
            {
                yield return new WaitForSeconds(0.25f);
            }
            yield return StartCoroutine(
    ShowWarning(spawnPosition)
);
            GameObject spawnedEnemy = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            currentEnemies.Add(spawnedEnemy);
            reservedPositions.Add(spawnPosition);

            if (stageManager != null)
                stageManager.AddSpawnedEnemy();

            yield return new WaitForSeconds(
                spawnInterval
            );
        }

        isSpawning = false;
    }

    private List<GameObject> GetAvailablePrefabs()
    {
        List<GameObject> result =
            new List<GameObject>();

        if (enemyPrefab != null)
            result.Add(enemyPrefab);

        if (jellyfishPrefab != null)
            result.Add(jellyfishPrefab);

        if (crabPrefab != null)
            result.Add(crabPrefab);

        return result;
    }

    private void Shuffle(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            GameObject temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private bool TryFindSpawnPosition(
        GameObject prefab,
        out Vector3 position)
    {
        position = Vector3.zero;

        Camera cam = Camera.main;

        if (cam == null)
            return false;

        float cameraDistance =
            Mathf.Abs(cam.transform.position.z);

        float left = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, cameraDistance)
        ).x;

        float right = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, cameraDistance)
        ).x;

        bool isCrab =
            prefab.GetComponent<CrabAttack>() != null;

        for (int i = 0; i < positionSearchAttempts; i++)
        {
            bool fromLeft = Random.value < 0.5f;

            float x;
            float y;

            if (isCrab)
            {
                // カニは画面内の左右端付近
                x = fromLeft
                    ? left + crabEdgeMargin
                    : right - crabEdgeMargin;

                y = crabSpawnY;
            }
            else
            {
                // 空中の敵は画面外から登場
                x = fromLeft
                    ? left - spawnOffsetX
                    : right + spawnOffsetX;

                y = Random.Range(
                    minSpawnY,
                    maxSpawnY
                );
            }

            Vector3 candidate = new Vector3(
                x, y, 0f
            );

            if (!IsPositionAvailable(candidate))
                continue;

            position = candidate;
            return true;
        }

        return false;
    }

    private bool IsPositionAvailable(Vector3 position)
    {
        // 今回のWaveで使用した位置
        foreach (Vector3 reserved in reservedPositions)
        {
            if (Vector2.Distance(
                position, reserved
            ) < minimumSpawnDistance)
            {
                return false;
            }
        }

        // 現在生存している敵との距離
        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy == null)
                continue;

            if (Vector2.Distance(
                position,
                enemy.transform.position
            ) < minimumSpawnDistance)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator ShowWarning(Vector3 spawnPosition)
    {
        if (warningPrefab == null || Camera.main == null)
            yield break;

        Camera cam = Camera.main;

        float distance = Mathf.Abs(
            spawnPosition.z - cam.transform.position.z
        );

        float left = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distance)
        ).x;

        float right = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distance)
        ).x;

        float centerX = (left + right) * 0.5f;

        // 敵が登場する側の画面端に表示
        float warningX = spawnPosition.x < centerX
            ? left + warningEdgeMargin
            : right - warningEdgeMargin;

        Vector3 warningPosition = new Vector3(
            warningX,
            spawnPosition.y,
            0f
        );

        GameObject warning = Instantiate(
            warningPrefab,
            warningPosition,
            Quaternion.identity
        );
        EnemyWarning warningController =
    warning.GetComponent<EnemyWarning>();

        if (warningController != null)
        {
            warningController.SetTargetPosition(
                spawnPosition
            );
        }

        SpriteRenderer[] renderers =
     warning.GetComponentsInChildren<SpriteRenderer>();

        float elapsed = 0f;

        bool isVisible = true;

        while (elapsed < warningDuration)
        {
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                {
                    sr.enabled = isVisible;
                }
            }

            isVisible = !isVisible;

            yield return new WaitForSeconds(
                warningBlinkInterval
            );

            elapsed += warningBlinkInterval;
        }

        Destroy(warning);

    }

}
