
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
    [SerializeField] private float nextWaveDelay = 1.3f;
    [SerializeField] private float phaseChangeDelay = 3f;
    [SerializeField] private float spawnInterval = 0.2f;

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

    private void Awake()
    {
        if (phaseAnnouncement == null)
            phaseAnnouncement = FindObjectOfType<PhaseAnnouncementUI>(true);

        if (phaseAnnouncement == null)
        {
            Debug.LogError(
                "EnemySpawner: PhaseAnnouncementUI が見つかりません。" +
                "Inspectorの Phase Announcement に設定してください。",
                this
            );
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
        waitingNextWave = true;

        StartCoroutine(BeginStageRoutine());
    }

    private IEnumerator BeginStageRoutine()
    {
        GameSoundManager.Instance?.PlayPhaseStart();

        if (phaseAnnouncement != null)
        {
            yield return StartCoroutine(
                phaseAnnouncement.ShowPhase(1)
            );
        }
        else
        {
            Debug.LogError(
                "EnemySpawner: PHASE 1 の演出を開始できません。",
                this
            );
        }

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
        if (enemyPrefab == null ||
            jellyfishPrefab == null ||
            crabPrefab == null)
        {
            Debug.LogError(
                "EnemySpawner: 3種類の敵Prefabを設定してください。"
            );
            return false;
        }

        waves.Clear();

        if (quickClearTestMode)
        {
            waves.Add(new WaveData(1, enemyPrefab));
            return true;
        }

        // PHASE 1：各ウェーブ4体
        waves.Add(new WaveData(
            1,
            enemyPrefab,
            enemyPrefab,
            jellyfishPrefab,
            enemyPrefab
        ));

        waves.Add(new WaveData(
            1,
            enemyPrefab,
            jellyfishPrefab,
            enemyPrefab,
            jellyfishPrefab
        ));

        waves.Add(new WaveData(
            1,
            enemyPrefab,
            jellyfishPrefab,
            enemyPrefab,
            enemyPrefab
        ));

        // PHASE 2：各ウェーブ5体
        waves.Add(new WaveData(
            2,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab,
            jellyfishPrefab
        ));

        waves.Add(new WaveData(
            2,
            jellyfishPrefab,
            enemyPrefab,
            crabPrefab,
            jellyfishPrefab,
            enemyPrefab
        ));

        waves.Add(new WaveData(
            2,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab,
            crabPrefab
        ));

        // PHASE 3：各ウェーブ6～7体
        waves.Add(new WaveData(
            3,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab,
            jellyfishPrefab,
            enemyPrefab
        ));

        waves.Add(new WaveData(
            3,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab
        ));

        waves.Add(new WaveData(
            3,
            crabPrefab,
            enemyPrefab,
            jellyfishPrefab,
            enemyPrefab,
            crabPrefab,
            jellyfishPrefab,
            enemyPrefab
        ));

        waves.Add(new WaveData(
            3,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab,
            jellyfishPrefab,
            crabPrefab,
            enemyPrefab
        ));

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
            GameSoundManager.Instance?.PlayPhaseStart();

            Debug.Log(
                "PHASE " + previousPhase +
                " CLEAR! → PHASE " + nextPhase
            );

            if (phaseAnnouncement != null)
            {
                yield return StartCoroutine(
                    phaseAnnouncement.ShowPhase(nextPhase)
                );
            }
            else
            {
                Debug.LogError(
                    "EnemySpawner: フェーズ演出が未設定です。",
                    this
                );
            }

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

        Debug.Log(
            "PHASE " + wave.phase +
            " / 3  Wave " + currentWave +
            " / " + waves.Count
        );

        StageManager stageManager = FindObjectOfType<StageManager>();

        List<Vector3> plannedPositions = new List<Vector3>();
        List<GameObject> plannedPrefabs = new List<GameObject>();

        // 敵全員の出現位置を先に決定する
        foreach (GameObject prefab in wave.prefabs)
        {
            Vector3 spawnPosition;
            int retryCount = 0;

            while (!TryFindSpawnPosition(prefab, out spawnPosition))
            {
                retryCount++;

                if (retryCount >= 20)
                {
                    if (!TryFindFallbackPosition(prefab, out spawnPosition))
                    {
                        Debug.LogError(
                            "EnemySpawner: 出現位置を取得できません。" +
                            "Main Cameraを確認してください。"
                        );

                        isSpawning = false;
                        yield break;
                    }

                    Debug.LogWarning(
                        "EnemySpawner: 距離制限を緩和して出現させます。"
                    );

                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            plannedPrefabs.Add(prefab);
            plannedPositions.Add(spawnPosition);

            // 同じウェーブ内で位置が重ならないようにする
            reservedPositions.Add(spawnPosition);
        }

        // 全敵の警告を同時に開始する
        for (int i = 0; i < plannedPositions.Count; i++)
        {
            StartCoroutine(
                ShowWarning(plannedPositions[i])
            );
        }

        // 警告が終了するまで待機する
        if (warningPrefab != null && Camera.main != null)
        {
            yield return new WaitForSeconds(
                Mathf.Max(0f, warningDuration)
            );
        }

        // 警告終了後、敵を短い間隔で出現させる
        for (int i = 0; i < plannedPrefabs.Count; i++)
        {
            GameObject spawnedEnemy = Instantiate(
                plannedPrefabs[i],
                plannedPositions[i],
                Quaternion.identity
            );

            currentEnemies.Add(spawnedEnemy);

            if (stageManager != null)
                stageManager.AddSpawnedEnemy();

            if (i < plannedPrefabs.Count - 1)
            {
                yield return new WaitForSeconds(
                    Mathf.Max(0f, spawnInterval)
                );
            }
        }

        isSpawning = false;
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
                x = fromLeft
                    ? left + crabEdgeMargin
                    : right - crabEdgeMargin;

                y = crabSpawnY;
            }
            else
            {
                x = fromLeft
                    ? left - spawnOffsetX
                    : right + spawnOffsetX;

                y = Random.Range(minSpawnY, maxSpawnY);
            }

            Vector3 candidate =
                new Vector3(x, y, 0f);

            if (!IsPositionAvailable(candidate))
                continue;

            position = candidate;
            return true;
        }

        return false;
    }

    private bool TryFindFallbackPosition(
        GameObject prefab,
        out Vector3 position)
    {
        position = Vector3.zero;

        Camera cam = Camera.main;

        if (cam == null)
            return false;

        float distance =
            Mathf.Abs(cam.transform.position.z);

        float left = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distance)
        ).x;

        float right = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distance)
        ).x;

        bool fromLeft = Random.value < 0.5f;

        bool isCrab =
            prefab.GetComponent<CrabAttack>() != null;

        float x = isCrab
            ? (fromLeft
                ? left + crabEdgeMargin
                : right - crabEdgeMargin)
            : (fromLeft
                ? left - spawnOffsetX
                : right + spawnOffsetX);

        float y = isCrab
            ? crabSpawnY
            : Random.Range(minSpawnY, maxSpawnY);

        position = new Vector3(x, y, 0f);

        return true;
    }

    private bool IsPositionAvailable(Vector3 position)
    {
        foreach (Vector3 reserved in reservedPositions)
        {
            if (Vector2.Distance(
                position,
                reserved
            ) < minimumSpawnDistance)
            {
                return false;
            }
        }

        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy != null &&
                Vector2.Distance(
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

        float blinkInterval = Mathf.Max(
            0.01f,
            warningBlinkInterval
        );

        while (elapsed < warningDuration)
        {
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                    sr.enabled = isVisible;
            }

            isVisible = !isVisible;

            yield return new WaitForSeconds(
                blinkInterval
            );

            elapsed += blinkInterval;
        }

        Destroy(warning);
    }
}
