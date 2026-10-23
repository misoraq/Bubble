
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class PhaseAnnouncementUI : MonoBehaviour
{
    [Header("基本UI")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private Image dimBackground;
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine;
    [SerializeField] private Image topLineImage;
    [SerializeField] private Image bottomLineImage;

    [Header("泡エフェクト")]
    [SerializeField] private RectTransform bubbleContainer;
    [SerializeField] private Sprite bubbleSprite;
    [SerializeField, Range(1, 80)] private int bubbleCount = 18;
    [SerializeField] private float bubbleSpeed = 100f;
    [SerializeField] private float bubbleMoveDistance = 180f;

    [Header("演出時間")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float lineDuration = 0.25f;
    [SerializeField] private float textDuration = 0.3f;
    [SerializeField] private float holdDuration = 0.7f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    [Header("文字の拡大")]
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float peakScale = 1.15f;

    [Header("背景色")]
    [SerializeField]
    private Color backgroundColor =
        new Color(0.01f, 0.08f, 0.15f, 0.75f);

    private CanvasGroup canvasGroup;
    private Vector3 originalTextScale;

    private readonly List<BubbleData> bubbles =
        new List<BubbleData>();

    private class BubbleData
    {
        public RectTransform rect;
        public Image image;
        public float speed;
        public float waveOffset;
        public float opacity;
        public float distanceMoved;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (phaseText != null)
            originalTextScale =
                phaseText.rectTransform.localScale;

        if (dimBackground != null)
        {
            dimBackground.color = backgroundColor;
            dimBackground.raycastTarget = false;
        }

        if (topLineImage != null)
            topLineImage.raycastTarget = false;

        if (bottomLineImage != null)
            bottomLineImage.raycastTarget = false;

        CreateBubbles();
        ResetVisuals();
    }

    public IEnumerator ShowPhase(int phase)
    {
        if (phaseText == null)
        {
            Debug.LogWarning(
                "PhaseAnnouncementUI: Phase Text が未設定です。");

            yield break;
        }

        // 先に透明度などを初期化してから泡を配置する
        ResetVisuals();
        SetupPhase(phase);
        ResetBubblePositions();

        yield return FadeIn();
        yield return ExpandLines();
        yield return ShowText();

        yield return AnimateFor(
            Mathf.Max(0f, holdDuration));

        yield return FadeOut();

        ResetVisuals();
    }

    private void SetupPhase(int phase)
    {
        Color accent;

        if (phase == 1)
        {
            phaseText.text = "PHASE 1";

            if (subtitleText != null)
                subtitleText.text = "READY? LET'S GO!";

            accent = new Color(0.38f, 0.86f, 1f);
        }
        else if (phase == 2)
        {
            phaseText.text = "PHASE 2";

            if (subtitleText != null)
                subtitleText.text = "DANGER LEVEL UP!";

            accent = new Color(1f, 0.81f, 0.38f);
        }
        else
        {
            phaseText.text = "FINAL PHASE";

            if (subtitleText != null)
                subtitleText.text = "FINAL WAVE INCOMING!";

            accent = new Color(1f, 0.48f, 0.60f);
        }

        phaseText.color = accent;

        if (subtitleText != null)
            subtitleText.color = Color.white;

        if (topLineImage != null)
            topLineImage.color = accent;

        if (bottomLineImage != null)
            bottomLineImage.color = accent;
    }

    private void ResetVisuals()
    {
        canvasGroup.alpha = 0f;

        if (topLine != null)
        {
            topLine.localScale =
                new Vector3(0f, 1f, 1f);
        }

        if (bottomLine != null)
        {
            bottomLine.localScale =
                new Vector3(0f, 1f, 1f);
        }

        if (phaseText != null)
        {
            phaseText.alpha = 0f;

            phaseText.rectTransform.localScale =
                originalTextScale * startScale;
        }

        if (subtitleText != null)
            subtitleText.alpha = 0f;

        foreach (BubbleData bubble in bubbles)
        {
            SetBubbleOpacity(bubble, 0f);
        }
    }

    private IEnumerator FadeIn()
    {
        float duration =
            Mathf.Max(0.01f, fadeInDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;

            elapsed += dt;

            canvasGroup.alpha =
                Mathf.Clamp01(elapsed / duration);

            MoveBubbles(dt);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator ExpandLines()
    {
        float duration =
            Mathf.Max(0.01f, lineDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;

            elapsed += dt;

            float t =
                Mathf.Clamp01(elapsed / duration);

            float eased =
                1f - Mathf.Pow(1f - t, 3f);

            Vector3 scale =
                new Vector3(eased, 1f, 1f);

            if (topLine != null)
                topLine.localScale = scale;

            if (bottomLine != null)
                bottomLine.localScale = scale;

            MoveBubbles(dt);

            yield return null;
        }

        if (topLine != null)
            topLine.localScale = Vector3.one;

        if (bottomLine != null)
            bottomLine.localScale = Vector3.one;
    }

    private IEnumerator ShowText()
    {
        float duration =
            Mathf.Max(0.01f, textDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;

            elapsed += dt;

            float t =
                Mathf.Clamp01(elapsed / duration);

            float scale;

            if (t < 0.65f)
            {
                float a = t / 0.65f;

                float eased =
                    1f - Mathf.Pow(1f - a, 3f);

                scale = Mathf.Lerp(
                    startScale,
                    peakScale,
                    eased);
            }
            else
            {
                float a =
                    (t - 0.65f) / 0.35f;

                scale = Mathf.Lerp(
                    peakScale,
                    1f,
                    a);
            }

            phaseText.alpha = t;

            phaseText.rectTransform.localScale =
                originalTextScale * scale;

            if (subtitleText != null)
                subtitleText.alpha = t;

            MoveBubbles(dt);

            yield return null;
        }

        phaseText.alpha = 1f;

        phaseText.rectTransform.localScale =
            originalTextScale;

        if (subtitleText != null)
            subtitleText.alpha = 1f;
    }

    private IEnumerator AnimateFor(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;

            elapsed += dt;

            MoveBubbles(dt);

            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float duration =
            Mathf.Max(0.01f, fadeOutDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;

            elapsed += dt;

            canvasGroup.alpha =
                1f - Mathf.Clamp01(elapsed / duration);

            MoveBubbles(dt);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void CreateBubbles()
    {
        if (bubbleContainer == null)
        {
            Debug.LogWarning(
                "PhaseAnnouncementUI: Bubble Container が未設定です。");

            return;
        }

        if (bubbleSprite == null)
        {
            Debug.LogWarning(
                "PhaseAnnouncementUI: Bubble Sprite が未設定です。");

            return;
        }

        for (int i = 0; i < bubbleCount; i++)
        {
            GameObject obj = new GameObject(
                "PhaseBubble_" + i,
                typeof(RectTransform),
                typeof(Image));

            obj.transform.SetParent(
                bubbleContainer,
                false);

            RectTransform rect =
                obj.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            float size =
                Random.Range(14f, 48f);

            rect.sizeDelta =
                new Vector2(size, size);

            Image image =
                obj.GetComponent<Image>();

            image.sprite = bubbleSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            BubbleData data = new BubbleData
            {
                rect = rect,
                image = image,

                speed = Random.Range(
                    bubbleSpeed * 0.5f,
                    bubbleSpeed * 1.5f),

                waveOffset = Random.Range(
                    0f,
                    Mathf.PI * 2f),

                opacity = Random.Range(
                    0.45f,
                    0.9f)
            };

            bubbles.Add(data);
        }
    }

    private void ResetBubblePositions()
    {
        if (bubbleContainer == null)
            return;

        float width =
            bubbleContainer.rect.width;

        float height =
            bubbleContainer.rect.height;

        if (width <= 0f || height <= 0f)
        {
            Debug.LogWarning(
                "PhaseAnnouncementUI: " +
                "Bubble Container の幅と高さを設定してください。");

            return;
        }

        foreach (BubbleData bubble in bubbles)
        {
            float x = Random.Range(
                -width * 0.45f,
                width * 0.45f);

            float y = Random.Range(
                -height * 0.4f,
                height * 0.25f);

            bubble.rect.anchoredPosition =
                new Vector2(x, y);

            bubble.distanceMoved = 0f;

            // 初期化後に透明度を戻す
            SetBubbleOpacity(
                bubble,
                bubble.opacity);
        }
    }

    private void MoveBubbles(float dt)
    {
        if (bubbleContainer == null)
            return;

        float top =
            bubbleContainer.rect.height * 0.5f;

        float bottom = -top;

        foreach (BubbleData bubble in bubbles)
        {
            Vector2 position =
                bubble.rect.anchoredPosition;

            float rise =
                bubble.speed * dt;

            position.y += rise;

            position.x +=
                Mathf.Sin(
                    Time.unscaledTime * 2f +
                    bubble.waveOffset)
                * 12f * dt;

            bubble.distanceMoved += rise;

            if (position.y > top ||
                (bubbleMoveDistance > 0f &&
                 bubble.distanceMoved > bubbleMoveDistance))
            {
                position.y = bottom;

                bubble.distanceMoved = 0f;
            }

            bubble.rect.anchoredPosition =
                position;
        }
    }

    private void SetBubbleOpacity(
        BubbleData bubble,
        float alpha)
    {
        if (bubble.image == null)
            return;

        Color color = Color.white;

        color.a = alpha;

        bubble.image.color = color;
    }
}
