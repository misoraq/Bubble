
using System.Collections;
using UnityEngine;

public class CloudItemSpawner : MonoBehaviour
{
    [Header("雲")]
    [SerializeField] private SpriteRenderer cloudSprite;

    [Header("出現位置")]
    [SerializeField] private Transform spawnPoint;

    [Header("回復アイテム")]
    [SerializeField] private GameObject recoveryPrefab;

    [Header("青い雷エフェクト")]
    [SerializeField] private GameObject blueLightningPrefab;

    [Header("出現間隔")]
    [SerializeField] private bool randomInterval = false;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 15f;

    [Header("点滅設定")]
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float flashInterval = 0.15f;

    [Header("雷の表示時間")]
    [SerializeField] private float lightningDuration = 0.3f;
    [Header("雷のランダム方向")]
    [SerializeField] private float lightningDistance = 1.5f;
    [Header("プレイヤー")]
    [SerializeField] private Transform player;
    private Color originalColor;

    private void Start()
    {
        if (cloudSprite == null || spawnPoint == null)
        {
            Debug.LogWarning("雲またはSpawnPointが未設定です");
            enabled = false;
            return;
        }

        originalColor = cloudSprite.color;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float waitTime = randomInterval
                ? Random.Range(minInterval, maxInterval)
                : spawnInterval;

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, waitTime)
            );

            // 黄色く点滅
            for (int i = 0; i < flashCount; i++)
            {
                cloudSprite.color = flashColor;
                yield return new WaitForSeconds(flashInterval);

                cloudSprite.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
            }

            // 360度からランダムな角度を選ぶ

            Vector3 startPosition = cloudSprite.bounds.center;

            // 現在のプレイヤーの位置に向ける
            Vector2 direction = player != null
                ? ((Vector2)(player.position - startPosition)).normalized
                : Vector2.down;

            // プレイヤー方向の角度を計算
            float angle = Mathf.Atan2(direction.y, direction.x)
                          * Mathf.Rad2Deg;


            Vector3 endPosition =
                startPosition + (Vector3)(direction * lightningDistance);

            // 雷の角度
            float rotationZ = angle - 90f;

            // 青い雷を表示
            if (blueLightningPrefab != null)
            {
                GameObject lightning = Instantiate(
                    blueLightningPrefab,
                    startPosition,
                    Quaternion.Euler(0f, 0f, rotationZ)
                );

                lightning.transform.position =
                    (startPosition + endPosition) * 0.5f;

                // 指定秒数後に雷を削除
                Destroy(lightning, lightningDuration);
            }

            yield return new WaitForSeconds(lightningDuration);


            // 泡エフェクトの先端から回復アイテムを出現
            if (recoveryPrefab != null)
            {
                GameObject item = Instantiate(
                    recoveryPrefab,
                    endPosition,
                    Quaternion.identity
                );

                RecoveryItem recoveryItem =
                    item.GetComponent<RecoveryItem>();

                if (recoveryItem != null)
                {
                    // 泡エフェクトと同じ方向に飛ばす
                    recoveryItem.SetDirection(direction);
                }
            }

        }
    }

    private void OnDisable()
    {
        if (cloudSprite != null)
            cloudSprite.color = originalColor;
    }
}
