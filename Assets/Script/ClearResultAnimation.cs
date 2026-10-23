using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClearResultAnimation : MonoBehaviour
{
    [Header("STAGE CLEAR! 画像")]
    [SerializeField] private RectTransform clearText;

    [Header("敵の撃破数")]
    [SerializeField] private RectTransform pufferfishGroup;
    [SerializeField] private RectTransform jellyfishGroup;
    [SerializeField] private RectTransform crabGroup;

    [Header("星評価（既存の評価文字列を読み取る）")]
    [SerializeField] private TMP_Text starText;

    [Header("独立した3つの星Image（左から順に登録）")]
    [SerializeField] private Image[] starImages = new Image[3];

    [Header("星のスプライト")]
    [SerializeField] private Sprite unlitStarSprite;
    [SerializeField] private Sprite litStarSprite;

    [Header("タイトルボタン")]
    [SerializeField] private GameObject returnTitleButton;

    [Header("タイトルの浮上演出")]
    [SerializeField] private float titleDuration = 0.8f;
    [SerializeField] private float titleStartOffset = -100f;

    [Header("その他の演出")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float interval = 0.2f;
    [SerializeField] private float starInterval = 0.25f;

    [Header("星が順番に光る演出")]
    [SerializeField] private float starLightDuration = 0.35f;
    [SerializeField] private float starPulseScale = 1.25f;

    private Coroutine animationCoroutine;
    private string savedStars = "";
    private Vector2 titleOriginalPosition;
    private Vector3 titleOriginalScale = Vector3.one;
    private Image titleImage;
    private Vector3[] starOriginalScales;

    private bool initialized = false;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    // 非アクティブなClearPanelでもPrepareAnimationから初期化できるようにする。
    private void InitializeIfNeeded()
    {
        if (initialized) return;

        if (clearText != null)
        {
            titleOriginalPosition = clearText.anchoredPosition;
            titleOriginalScale = clearText.localScale;
            titleImage = clearText.GetComponent<Image>();
        }

        if (starImages == null)
            starImages = new Image[0];

        starOriginalScales = new Vector3[starImages.Length];
        for (int i = 0; i < starImages.Length; i++)
        {
            starOriginalScales[i] = starImages[i] != null
                ? starImages[i].rectTransform.localScale
                : Vector3.one;
        }

        initialized = true;
    }

    public void PrepareAnimation()
    {
        InitializeIfNeeded();
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (clearText != null)
        {
            clearText.localScale = titleOriginalScale * 0.7f;
            clearText.anchoredPosition = titleOriginalPosition + new Vector2(0f, titleStartOffset);
        }

        if (titleImage != null)
        {
            Color c = titleImage.color;
            c.a = 0f;
            titleImage.color = c;
        }

        HideGroup(pufferfishGroup);
        HideGroup(jellyfishGroup);
        HideGroup(crabGroup);

        if (starText != null)
        {
            // 既存の星評価を保持。空文字の場合は前回値を上書きしない。
            if (!string.IsNullOrEmpty(starText.text))
                savedStars = starText.text;

            starText.text = "";
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            Image star = starImages[i];
            if (star == null) continue;

            star.gameObject.SetActive(true);
            star.enabled = false;
            star.sprite = unlitStarSprite;
            star.color = Color.white;
            star.rectTransform.localScale = starOriginalScales[i];
        }

        if (returnTitleButton != null)
            returnTitleButton.SetActive(false);
    }

    public void PlayAnimation()
    {
        InitializeIfNeeded();
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        yield return StartCoroutine(FloatTitle());
        yield return new WaitForSecondsRealtime(interval);

        yield return StartCoroutine(Pop(pufferfishGroup));
        yield return new WaitForSecondsRealtime(interval);

        yield return StartCoroutine(Pop(jellyfishGroup));
        yield return new WaitForSecondsRealtime(interval);

        yield return StartCoroutine(Pop(crabGroup));
        yield return new WaitForSecondsRealtime(interval);

        // 3つの星を未点灯のスプライトで表示する。
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            starImages[i].sprite = unlitStarSprite;
            starImages[i].color = Color.white;
            starImages[i].enabled = true;
        }

        int earned = CountEarnedStars(savedStars);
        int count = Mathf.Min(earned, starImages.Length);

        for (int i = 0; i < count; i++)
        {
            if (starImages[i] != null)
                yield return StartCoroutine(LightUpStar(i));

            yield return new WaitForSecondsRealtime(starInterval);
        }

        yield return new WaitForSecondsRealtime(interval);

        if (returnTitleButton != null)
            returnTitleButton.SetActive(true);

        animationCoroutine = null;
    }

    private IEnumerator FloatTitle()
    {
        if (clearText == null) yield break;

        Vector2 start = titleOriginalPosition + new Vector2(0f, titleStartOffset);
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, titleDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            clearText.anchoredPosition = Vector2.Lerp(start, titleOriginalPosition, eased);
            clearText.localScale = titleOriginalScale *
                Mathf.LerpUnclamped(0.7f, 1f, EaseOutBack(t));

            if (titleImage != null)
            {
                Color c = titleImage.color;
                c.a = Mathf.SmoothStep(0f, 1f, t);
                titleImage.color = c;
            }

            yield return null;
        }

        clearText.anchoredPosition = titleOriginalPosition;
        clearText.localScale = titleOriginalScale;

        if (titleImage != null)
        {
            Color c = titleImage.color;
            c.a = 1f;
            titleImage.color = c;
        }
    }

    private void HideGroup(RectTransform target)
    {
        if (target == null) return;

        target.gameObject.SetActive(true);
        target.localScale = Vector3.zero;
    }

    private IEnumerator Pop(RectTransform target)
    {
        if (target == null) yield break;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, popDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.one *
                Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t));
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private IEnumerator LightUpStar(int index)
    {
        Image star = starImages[index];
        RectTransform rect = star.rectTransform;
        Vector3 baseScale = starOriginalScales[index];

        // 色を変えるのではなく、黄色の星スプライトへ切り替える。
        star.sprite = litStarSprite;
        star.color = Color.white;

        float duration = Mathf.Max(0.01f, starLightDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = 1f + (starPulseScale - 1f) * Mathf.Sin(t * Mathf.PI);
            rect.localScale = baseScale * pulse;
            yield return null;
        }

        rect.localScale = baseScale;
    }

    private int CountEarnedStars(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        int count = 0;
        foreach (char c in value)
        {
            if (c == '★' || c == '⭐') count++;
        }

        return Mathf.Clamp(count, 0, 3);
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
