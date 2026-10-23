
using UnityEngine;

public class ShopBubbleSpawner : MonoBehaviour
{
    [Header("ñAPrefab")]
    [SerializeField] private BubbleFloatUI bubblePrefab;

    [Header("ê∂ê¨îÕàÕ")]
    [SerializeField] private RectTransform spawnArea;

    [SerializeField] private float leftMargin = 15f;
    [SerializeField] private float rightMargin = 15f;

    [Header("ê∂ê¨ä‘äu")]
    [SerializeField] private float minSpawnInterval = 0.4f;
    [SerializeField] private float maxSpawnInterval = 0.9f;

    [Header("ñAÇÃà⁄ìÆë¨ìx")]
    [SerializeField] private float minRiseSpeed = 35f;
    [SerializeField] private float maxRiseSpeed = 65f;

    [Header("ñAÇÃÉTÉCÉYî{ó¶")]
    [SerializeField] private float minSize = 0.5f;
    [SerializeField] private float maxSize = 1.2f;

    [Header("ñAÇÃéıñΩ")]
    [SerializeField] private float minLifeTime = 2f;
    [SerializeField] private float maxLifeTime = 4f;

    private float spawnTimer;

    private void OnEnable()
    {
        ResetSpawnTimer();
    }

    private void Update()
    {
        if (bubblePrefab == null || spawnArea == null)
        {
            return;
        }

        spawnTimer -= Time.unscaledDeltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnBubble();
            ResetSpawnTimer();
        }
    }

    private void ResetSpawnTimer()
    {
        float min = Mathf.Max(0.05f, minSpawnInterval);
        float max = Mathf.Max(min, maxSpawnInterval);

        spawnTimer = Random.Range(min, max);
    }

    private void SpawnBubble()
    {
        Rect rect = spawnArea.rect;

        float minX = rect.xMin + leftMargin;
        float maxX = rect.xMax - rightMargin;

        if (minX > maxX)
        {
            return;
        }

        float x = Random.Range(minX, maxX);
        float y = rect.yMin;

        BubbleFloatUI bubble = Instantiate(
            bubblePrefab,
            spawnArea
        );

        RectTransform bubbleRect =
            bubble.GetComponent<RectTransform>();

        bubbleRect.anchorMin = new Vector2(0.5f, 0.5f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0.5f);

        bubbleRect.anchoredPosition =
            new Vector2(x, y);

        float speed = Random.Range(
            minRiseSpeed,
            maxRiseSpeed
        );

        float size = Random.Range(
            minSize,
            maxSize
        );

        float lifetime = Random.Range(
            minLifeTime,
            maxLifeTime
        );

        bubble.Initialize(speed, size, lifetime);
    }
}
