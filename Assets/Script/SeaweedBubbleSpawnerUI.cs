
using System.Collections;
using UnityEngine;

public class SeaweedBubbleSpawnerUI : MonoBehaviour
{
    [Header("ñAÇÃPrefabÅiUI ImageÅj")]
    [SerializeField] private GameObject bubblePrefab;

    [Header("ñAÇï\é¶Ç∑ÇÈêe")]
    [SerializeField] private RectTransform bubbleParent;

    [Header("ñAÇÃî≠ê∂à íu")]
    [SerializeField] private RectTransform leftSpawnPoint;
    [SerializeField] private RectTransform rightSpawnPoint;

    [Header("î≠ê∂ä‘äu")]
    [SerializeField] private float minSpawnInterval = 0.4f;
    [SerializeField] private float maxSpawnInterval = 1.2f;

    [Header("ñAÇÃà⁄ìÆ")]
    [SerializeField] private float minRiseSpeed = 25f;
    [SerializeField] private float maxRiseSpeed = 55f;
    [SerializeField] private float lifeTime = 2.5f;
    [SerializeField] private float swayAmount = 12f;
    [SerializeField] private float swaySpeed = 2f;

    [Header("ñAÇÃÉTÉCÉY")]
    [SerializeField] private float minBubbleSize = 8f;
    [SerializeField] private float maxBubbleSize = 18f;

    [Header("î≠ê∂à íuÇÃÇŒÇÁÇ¬Ç´")]
    [SerializeField] private float spawnOffsetX = 15f;
    [SerializeField] private float spawnOffsetY = 8f;

    private Coroutine spawnCoroutine;

    private void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float interval = Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );

            yield return new WaitForSeconds(interval);

            RectTransform spawnPoint =
                Random.value < 0.5f
                    ? leftSpawnPoint
                    : rightSpawnPoint;

            if (spawnPoint != null)
            {
                SpawnBubble(spawnPoint);
            }
        }
    }

    private void SpawnBubble(RectTransform spawnPoint)
    {
        if (bubblePrefab == null || bubbleParent == null)
            return;

        GameObject bubble = Instantiate(
            bubblePrefab,
            bubbleParent,
            false
        );

        RectTransform bubbleRect =
            bubble.GetComponent<RectTransform>();

        if (bubbleRect == null)
        {
            Destroy(bubble);
            return;
        }

        Vector2 localPosition =
            bubbleParent.InverseTransformPoint(
                spawnPoint.position
            );

        localPosition += new Vector2(
            Random.Range(-spawnOffsetX, spawnOffsetX),
            Random.Range(-spawnOffsetY, spawnOffsetY)
        );

        bubbleRect.anchorMin = new Vector2(0.5f, 0.5f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0.5f);
        bubbleRect.anchoredPosition = localPosition;

        float size = Random.Range(
            minBubbleSize,
            maxBubbleSize
        );

        bubbleRect.sizeDelta = new Vector2(size, size);

        float speed = Random.Range(
            minRiseSpeed,
            maxRiseSpeed
        );

        StartCoroutine(
            MoveBubble(bubbleRect, speed)
        );
    }

    private IEnumerator MoveBubble(
        RectTransform bubbleRect,
        float riseSpeed
    )
    {
        Vector2 startPosition = bubbleRect.anchoredPosition;

        CanvasGroup canvasGroup =
            bubbleRect.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                bubbleRect.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        float elapsed = 0f;
        float randomPhase = Random.Range(0f, Mathf.PI * 2f);

        while (elapsed < lifeTime && bubbleRect != null)
        {
            elapsed += Time.deltaTime;

            float x = Mathf.Sin(
                elapsed * swaySpeed + randomPhase
            ) * swayAmount;

            float y = elapsed * riseSpeed;

            bubbleRect.anchoredPosition =
                startPosition + new Vector2(x, y);

            float progress = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, lifeTime)
            );

            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        if (bubbleRect != null)
        {
            Destroy(bubbleRect.gameObject);
        }
    }
}
