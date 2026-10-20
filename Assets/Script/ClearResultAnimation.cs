
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

    [Header("星評価")]
    [SerializeField] private TMP_Text starText;

    [Header("タイトルボタン")]
    [SerializeField] private GameObject returnTitleButton;

    [Header("タイトルの浮上演出")]
    [SerializeField] private float titleDuration = 0.8f;
    [SerializeField] private float titleStartOffset = -100f;

    [Header("その他の演出")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float interval = 0.2f;
    [SerializeField] private float starInterval = 0.25f;

    private Coroutine animationCoroutine;
    private string savedStars = "";

    private Vector2 titleOriginalPosition;
    private Image titleImage;

    private void Awake()
    {
        if (clearText != null)
        {
            titleOriginalPosition = clearText.anchoredPosition;
            titleImage = clearText.GetComponent<Image>();
        }
    }

    // 泡が割れる前に呼び出す
    public void PrepareAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // STAGE CLEAR! を隠す
        if (clearText != null)
        {
            clearText.localScale = Vector3.one * 0.7f;

            clearText.anchoredPosition =
                titleOriginalPosition +
                new Vector2(0f, titleStartOffset);
        }

        if (titleImage != null)
        {
            Color color = titleImage.color;
            color.a = 0f;
            titleImage.color = color;
        }

        HideGroup(pufferfishGroup);
        HideGroup(jellyfishGroup);
        HideGroup(crabGroup);

        if (starText != null)
        {
            savedStars = starText.text;
            starText.text = "";
        }

        if (returnTitleButton != null)
            returnTitleButton.SetActive(false);
    }

    public void PlayAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        // ① STAGE CLEAR! が浮かび上がる
        yield return StartCoroutine(FloatTitle());

        yield return new WaitForSecondsRealtime(interval);

        // ② 敵アイコンが順番に登場
        yield return StartCoroutine(Pop(pufferfishGroup));
        yield return new WaitForSecondsRealtime(interval);

        yield return StartCoroutine(Pop(jellyfishGroup));
        yield return new WaitForSecondsRealtime(interval);

        yield return StartCoroutine(Pop(crabGroup));
        yield return new WaitForSecondsRealtime(interval);

        // ③ 星評価
        if (starText != null)
        {
            foreach (char star in savedStars)
            {
                starText.text += star;

                yield return new WaitForSecondsRealtime(
                    starInterval
                );
            }
        }

        yield return new WaitForSecondsRealtime(interval);

        // ④ タイトルボタン
        if (returnTitleButton != null)
            returnTitleButton.SetActive(true);

        animationCoroutine = null;
    }

    // STAGE CLEAR! の浮上アニメーション
    private IEnumerator FloatTitle()
    {
        if (clearText == null)
            yield break;

        Vector2 startPosition =
            titleOriginalPosition +
            new Vector2(0f, titleStartOffset);

        float timer = 0f;

        while (timer < titleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / titleDuration
            );

            // ゆっくり減速しながら浮上
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            clearText.anchoredPosition = Vector2.Lerp(
                startPosition,
                titleOriginalPosition,
                eased
            );

            // 小さい状態から少し大きくなる
            float scale = Mathf.LerpUnclamped(
                0.7f,
                1f,
                EaseOutBack(t)
            );

            clearText.localScale =
                Vector3.one * scale;

            // 透明から徐々に表示
            if (titleImage != null)
            {
                Color color = titleImage.color;
                color.a = Mathf.SmoothStep(0f, 1f, t);
                titleImage.color = color;
            }

            yield return null;
        }

        clearText.anchoredPosition =
            titleOriginalPosition;

        clearText.localScale = Vector3.one;

        if (titleImage != null)
        {
            Color color = titleImage.color;
            color.a = 1f;
            titleImage.color = color;
        }
    }

    private void HideGroup(RectTransform target)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(true);
        target.localScale = Vector3.zero;
    }

    private IEnumerator Pop(RectTransform target)
    {
        if (target == null)
            yield break;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / popDuration);

            float scale = Mathf.LerpUnclamped(
                0f,
                1f,
                EaseOutBack(t)
            );

            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f)
                  + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
