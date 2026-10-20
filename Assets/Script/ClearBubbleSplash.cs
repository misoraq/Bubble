
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClearBubbleSplash : MonoBehaviour
{
    [Header("水滴Prefab")]
    [SerializeField] private Image dropletPrefab;

    [Header("水滴の設定")]
    [SerializeField] private int dropletsPerCorner = 6;
    [SerializeField] private float minDistance = 100f;
    [SerializeField] private float maxDistance = 350f;
    [SerializeField] private float duration = 0.6f;

    [Header("四隅からの位置調整")]
    [SerializeField] private float cornerOffset = 30f;

    public void PlaySplash()
    {
        if (dropletPrefab == null)
        {
            Debug.LogWarning("水滴Prefabが設定されていません");
            return;
        }

        RectTransform parentRect = GetComponent<RectTransform>();

        if (parentRect == null)
            return;

        float halfWidth = parentRect.rect.width / 2f;
        float halfHeight = parentRect.rect.height / 2f;

        // 左上
        SpawnCorner(
            new Vector2(-halfWidth + cornerOffset,
                         halfHeight - cornerOffset),
            135f
        );

        // 右上
        SpawnCorner(
            new Vector2(halfWidth - cornerOffset,
                         halfHeight - cornerOffset),
            45f
        );

        // 左下
        SpawnCorner(
            new Vector2(-halfWidth + cornerOffset,
                         -halfHeight + cornerOffset),
            225f
        );

        // 右下
        SpawnCorner(
            new Vector2(halfWidth - cornerOffset,
                         -halfHeight + cornerOffset),
            315f
        );
    }

    private void SpawnCorner(Vector2 startPosition, float baseAngle)
    {
        for (int i = 0; i < dropletsPerCorner; i++)
        {
            Image droplet = Instantiate(dropletPrefab, transform);
            droplet.gameObject.SetActive(true);

            RectTransform rect = droplet.rectTransform;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = startPosition;

            // 四隅から外側に向かう角度
            float angle = baseAngle + Random.Range(-40f, 40f);

            float distance = Random.Range(
                minDistance,
                maxDistance
            );

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            rect.localRotation =
                Quaternion.Euler(0f, 0f, angle - 90f);

            StartCoroutine(
                MoveDroplet(
                    droplet,
                    startPosition,
                    startPosition + direction * distance
                )
            );
        }
    }

    private IEnumerator MoveDroplet(
        Image droplet,
        Vector2 startPosition,
        Vector2 targetPosition
    )
    {
        RectTransform rect = droplet.rectTransform;

        Color originalColor = droplet.color;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            // 最初は勢いよく、後半はゆっくり
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                eased
            );

            Color color = originalColor;
            color.a = originalColor.a * (1f - t);
            droplet.color = color;

            yield return null;
        }

        Destroy(droplet.gameObject);
    }
}
