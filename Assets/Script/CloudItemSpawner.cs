
using System.Collections;
using UnityEngine;

public class CloudItemSpawner : MonoBehaviour
{
    [Header("雲")]
    [SerializeField] private SpriteRenderer cloudSprite;

    [Header("チャージ表示")]
    [SerializeField] private SpriteRenderer chargeFill;
    [SerializeField] private SpriteMask chargeMask;
    [SerializeField]
    private Color cooldownColor =
        new Color(0.5f, 0.55f, 0.6f, 1f);

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

    [Header("雷の距離")]
    [SerializeField] private float lightningDistance = 1.5f;

    [Header("プレイヤー")]
    [SerializeField] private Transform player;

    private Color originalColor;
    private Vector3 maskOriginalScale;
    private Vector3 maskOriginalPosition;
    private float maskHeight;

    private void Start()
    {
        if (cloudSprite == null || spawnPoint == null)
        {
            Debug.LogWarning(
                "雲またはSpawnPointが未設定です"
            );

            enabled = false;
            return;
        }

        originalColor = cloudSprite.color;

        if (chargeFill != null && chargeMask != null)
        {
            chargeFill.sprite = cloudSprite.sprite;
            chargeFill.color = originalColor;

            chargeFill.maskInteraction =
                SpriteMaskInteraction.VisibleInsideMask;

            maskOriginalScale = chargeMask.transform.localScale;
            maskOriginalPosition = chargeMask.transform.localPosition;

            // マスクの初期サイズを保存
            maskHeight = chargeMask.bounds.size.y;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float waitTime = randomInterval
                ? Random.Range(minInterval, maxInterval)
                : spawnInterval;

            waitTime = Mathf.Max(0.1f, waitTime);

            // クールタイム開始
            cloudSprite.color = cooldownColor;

            if (chargeFill != null)
                chargeFill.enabled = true;

            SetChargeProgress(0f);

            // 時間に合わせて下からたまる
            float timer = 0f;

            while (timer < waitTime)
            {
                timer += Time.deltaTime;

                float progress =
                    Mathf.Clamp01(timer / waitTime);

                SetChargeProgress(progress);

                yield return null;
            }

            // チャージ完了
            SetChargeProgress(1f);

            // 点滅中は元の雲だけ表示
            if (chargeFill != null)
                chargeFill.enabled = false;

            cloudSprite.color = originalColor;

            // 黄色く点滅
            for (int i = 0; i < flashCount; i++)
            {
                cloudSprite.color = flashColor;

                yield return new WaitForSeconds(
                    flashInterval
                );

                cloudSprite.color = originalColor;

                yield return new WaitForSeconds(
                    flashInterval
                );
            }

            // 雷の方向を決定
            Vector3 startPosition = cloudSprite.bounds.center;

            Vector2 direction = player != null
                ? ((Vector2)(player.position - startPosition)).normalized
                : Vector2.down;

            float angle =
                Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;

            Vector3 endPosition =
                startPosition +
                (Vector3)(direction * lightningDistance);

            float rotationZ = angle - 90f;

            // 青い雷を生成
            if (blueLightningPrefab != null)
            {
                GameObject lightning = Instantiate(
                    blueLightningPrefab,
                    startPosition,
                    Quaternion.Euler(0f, 0f, rotationZ)
                );

                lightning.transform.position =
                    (startPosition + endPosition) * 0.5f;

                Destroy(lightning, lightningDuration);
            }

            yield return new WaitForSeconds(
                lightningDuration
            );

            // 回復アイテムを生成
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
                    recoveryItem.SetDirection(direction);
            }
        }
    }

    private void SetChargeProgress(float progress)
    {
        if (chargeMask == null)
            return;

        progress = Mathf.Clamp01(progress);

        Transform maskTransform = chargeMask.transform;

        // マスクの高さを下から上へ伸ばす
        Vector3 scale = maskOriginalScale;

        scale.y = maskOriginalScale.y *
                  Mathf.Max(0.001f, progress);

        maskTransform.localScale = scale;

        // 下端を固定するため、中心を下に移動する
        Vector3 position = maskOriginalPosition;

        float fullHeight =
            chargeMask.sprite.bounds.size.y *
            Mathf.Abs(maskOriginalScale.y);

        position.y -= fullHeight * (1f - progress) * 0.5f;

        maskTransform.localPosition = position;
    }

    private void OnDisable()
    {
        if (cloudSprite != null)
            cloudSprite.color = originalColor;

        if (chargeFill != null)
            chargeFill.enabled = false;
    }
}
