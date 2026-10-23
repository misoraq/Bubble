
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BubbleFloatUI : MonoBehaviour
{
    [Header("ˆÚ“®")]
    [SerializeField] private float riseSpeed = 70f;
    [SerializeField] private float swayAmount = 12f;
    [SerializeField] private float swaySpeed = 2f;

    [Header("Žõ–½")]
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private RectTransform rectTransform;
    private Image bubbleImage;

    private Vector2 startPosition;
    private float elapsedTime;
    private float phaseOffset;

    private Color initialColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bubbleImage = GetComponent<Image>();

        if (bubbleImage != null)
        {
            bubbleImage.raycastTarget = false;
            initialColor = bubbleImage.color;
        }

        startPosition = rectTransform.anchoredPosition;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public void Initialize(
        float speed,
        float size,
        float lifetime)
    {
        riseSpeed = speed;
        lifeTime = Mathf.Max(0.1f, lifetime);

        rectTransform.localScale = Vector3.one * size;
        startPosition = rectTransform.anchoredPosition;
        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime += Time.unscaledDeltaTime;

        float x = Mathf.Sin(
            elapsedTime * swaySpeed + phaseOffset
        ) * swayAmount;

        float y = elapsedTime * riseSpeed;

        rectTransform.anchoredPosition =
            startPosition + new Vector2(x, y);

        if (bubbleImage != null)
        {
            float remaining = lifeTime - elapsedTime;

            float alpha = fadeDuration > 0f
                ? Mathf.Clamp01(remaining / fadeDuration)
                : 1f;

            Color color = initialColor;
            color.a = initialColor.a * alpha;
            bubbleImage.color = color;
        }

        if (elapsedTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
