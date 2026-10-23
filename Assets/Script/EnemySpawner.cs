using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject jellyfishPrefab;
    [SerializeField] private GameObject crabPrefab;

    [Header("ウェーブ間隔")]
    [SerializeField] private float nextWaveDelay = 2f;
    [SerializeField] private float phaseChangeDelay = 3f;
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

    [Header("フェーズ表示")]
    [SerializeField] private PhaseAnnouncementUI phaseAnnouncement;

    [Header("クリア演出テスト")]
    [SerializeField] private bool quickClearTestMode = false;

    private class WaveData
    {
        public int phase;
        public readonly List<GameObject> prefabs;

        public WaveData(int phase, params GameObject[] prefabs)
        {
            this.phase = phase;
            this.prefabs = new List<GameObject>(prefabs);
        }
    }

    private readonly List<WaveData> waves = new List<WaveData>();
    private readonly List<GameObject> currentEnemies = new List<GameObject>();
    private readonly List<Vector3> reservedPositions = new List<Vector3>();

    private int currentWave;
    private bool isSpawning;
    private bool waitingNextWave;
    private bool initialized;

    public bool IsAllWavesCompleted
    {
        get
        {
            return initialized &&
                   currentWave >= waves.Count &&
                   !isSpawning &&
                   !waitingNextWave &&
                   GetRemainingEnemyCount() == 0;
        }
    }

    private void Start()
    {
        if (!BuildWaves())
        {
            enabled = false;
            return;
        }

        int totalEnemies = 0;
        foreach (WaveData wave in waves)
            totalEnemies += wave.prefabs.Count;

        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager != null)
            stageManager.SetTotalEnemies(totalEnemies);

        initialized = true;
        waitingNextWave = true; // PHASE 1表示中にUpdateが次のWaveを開始しないようにする
        StartCoroutine(BeginStageRoutine());
    }

    private IEnumerator BeginStageRoutine()
    {
        if (phaseAnnouncement != null)
            yield return StartCoroutine(phaseAnnouncement.ShowPhase(1));

        yield return StartCoroutine(SpawnWaveRoutine());
        waitingNextWave = false;
    }

    private void Update()
    {
        if (!initialized || isSpawning || waitingNextWave)
            return;

        if (currentWave >= waves.Count || GetRemainingEnemyCount() > 0)
            return;

        StartCoroutine(NextWaveRoutine());
    }

    private bool BuildWaves()
    {
        if (enemyPrefab == null || jellyfishPrefab == null || crabPrefab == null)
        {
            Debug.LogError("EnemySpawner: 3種類の敵Prefabを設定してください。");
            return false;
        }

        waves.Clear();

        if (quickClearTestMode)
        {
            waves.Add(new WaveData(1, enemyPrefab));
            return true;
        }

        // PHASE 1：フグ4・クラゲ2（6体）
        waves.Add(new WaveData(1, enemyPrefab, enemyPrefab));
        waves.Add(new WaveData(1, enemyPrefab, jellyfishPrefab));
        waves.Add(new WaveData(1, enemyPrefab, jellyfishPrefab));

        // PHASE 2：フグ3・クラゲ4・カニ3（10体）
        waves.Add(new WaveData(2, enemyPrefab, jellyfishPrefab, crabPrefab));
        waves.Add(new WaveData(2, jellyfishPrefab, jellyfishPrefab, crabPrefab));
        waves.Add(new WaveData(2, enemyPrefab, jellyfishPrefab, crabPrefab, enemyPrefab));

        // PHASE 3：フグ4・クラゲ4・カニ4（12体）
        waves.Add(new WaveData(3, enemyPrefab, jellyfishPrefab, crabPrefab));
        waves.Add(new WaveData(3, jellyfishPrefab, crabPrefab, enemyPrefab));
        waves.Add(new WaveData(3, crabPrefab, enemyPrefab, jellyfishPrefab));
        waves.Add(new WaveData(3, enemyPrefab, jellyfishPrefab, crabPrefab));

        return true;
    }

    private int GetRemainingEnemyCount()
    {
        currentEnemies.RemoveAll(enemy => enemy == null);
        return currentEnemies.Count;
    }

    private IEnumerator NextWaveRoutine()
    {
        waitingNextWave = true;

        int previousPhase = waves[currentWave - 1].phase;
        int nextPhase = waves[currentWave].phase;
        bool isPhaseChange = previousPhase != nextPhase;

        if (isPhaseChange)
        {
            Debug.Log("PHASE " + previousPhase + " CLEAR! → PHASE " + nextPhase);
            if (phaseAnnouncement != null)
                yield return StartCoroutine(phaseAnnouncement.ShowPhase(nextPhase));

            yield return new WaitForSeconds(phaseChangeDelay);
        }
        else
        {
            yield return new WaitForSeconds(nextWaveDelay);
        }

        yield return StartCoroutine(SpawnWaveRoutine());
        waitingNextWave = false;
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;

        WaveData wave = waves[currentWave];
        currentWave++;
        currentEnemies.Clear();
        reservedPositions.Clear();

        Debug.Log("PHASE " + wave.phase + " / 3  Wave " + currentWave + " / " + waves.Count);

        StageManager stageManager = FindObjectOfType<StageManager>();

        foreach (GameObject prefab in wave.prefabs)
        {
            Vector3 spawnPosition;
            int retryCount = 0;
            while (!TryFindSpawnPosition(prefab, out spawnPosition))
            {
                retryCount++;
                if (retryCount >= 20)
                {
                    // 同じウェーブで出現位置を確保できない場合も進行を止めない
                    if (!TryFindFallbackPosition(prefab, out spawnPosition))
                    {
                        Debug.LogError("EnemySpawner: 出現位置を取得できません。Main Cameraを確認してください。");
                        isSpawning = false;
                        yield break;
                    }
                    Debug.LogWarning("EnemySpawner: 出現位置の距離制限を緩和してスポーンします。");
                    break;
                }
                yield return new WaitForSeconds(0.25f);
            }

            yield return StartCoroutine(ShowWarning(spawnPosition));

            GameObject spawnedEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            currentEnemies.Add(spawnedEnemy);
            reservedPositions.Add(spawnPosition);

            if (stageManager != null)
                stageManager.AddSpawnedEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private bool TryFindSpawnPosition(GameObject prefab, out Vector3 position)
    {
        position = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        float cameraDistance = Mathf.Abs(cam.transform.position.z);
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDistance)).x;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDistance)).x;
        bool isCrab = prefab.GetComponent<CrabAttack>() != null;

        for (int i = 0; i < positionSearchAttempts; i++)
        {
            bool fromLeft = Random.value < 0.5f;
            float x;
            float y;

            if (isCrab)
            {
                x = fromLeft ? left + crabEdgeMargin : right - crabEdgeMargin;
                y = crabSpawnY;
            }
            else
            {
                x = fromLeft ? left - spawnOffsetX : right + spawnOffsetX;
                y = Random.Range(minSpawnY, maxSpawnY);
            }

            Vector3 candidate = new Vector3(x, y, 0f);
            if (!IsPositionAvailable(candidate))
                continue;

            position = candidate;
            return true;
        }

        return false;
    }

    private bool TryFindFallbackPosition(GameObject prefab, out Vector3 position)
    {
        position = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null) return false;

        float distance = Mathf.Abs(cam.transform.position.z);
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x;
        bool fromLeft = Random.value < 0.5f;
        bool isCrab = prefab.GetComponent<CrabAttack>() != null;
        float x = isCrab
            ? (fromLeft ? left + crabEdgeMargin : right - crabEdgeMargin)
            : (fromLeft ? left - spawnOffsetX : right + spawnOffsetX);
        float y = isCrab ? crabSpawnY : Random.Range(minSpawnY, maxSpawnY);
        position = new Vector3(x, y, 0f);
        return true;
    }

    private bool IsPositionAvailable(Vector3 position)
    {
        foreach (Vector3 reserved in reservedPositions)
        {
            if (Vector2.Distance(position, reserved) < minimumSpawnDistance)
                return false;
        }

        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy != null &&
                Vector2.Distance(position, enemy.transform.position) < minimumSpawnDistance)
                return false;
        }

        return true;
    }

    private IEnumerator ShowWarning(Vector3 spawnPosition)
    {
        if (warningPrefab == null || Camera.main == null)
            yield break;

        Camera cam = Camera.main;
        float distance = Mathf.Abs(spawnPosition.z - cam.transform.position.z);
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x;
        float centerX = (left + right) * 0.5f;
        float warningX = spawnPosition.x < centerX
            ? left + warningEdgeMargin
            : right - warningEdgeMargin;

        Vector3 warningPosition = new Vector3(warningX, spawnPosition.y, 0f);
        GameObject warning = Instantiate(warningPrefab, warningPosition, Quaternion.identity);

        EnemyWarning warningController = warning.GetComponent<EnemyWarning>();
        if (warningController != null)
            warningController.SetTargetPosition(spawnPosition);

        SpriteRenderer[] renderers = warning.GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;
        bool isVisible = true;
        float blinkInterval = Mathf.Max(0.01f, warningBlinkInterval);

        while (elapsed < warningDuration)
        {
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                    sr.enabled = isVisible;
            }

            isVisible = !isVisible;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        Destroy(warning);
    }
}
