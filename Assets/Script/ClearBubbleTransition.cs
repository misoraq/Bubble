
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PixelBattleText;
public class ClearBubbleTransition : MonoBehaviour
{
    [Header("泡のUI")]
    [SerializeField] private RectTransform bubbleImage;

    [Header("リザルト画面")]
    [SerializeField] private GameObject clearPanel;

    [Header("スローモーション")]
    [SerializeField] private float slowMotionScale = 0.15f;
    [SerializeField] private float slowMotionDuration = 0.8f;
    [SerializeField] private float slowMotionHold = 0.4f;

    [Header("フェードイン")]
    [SerializeField] private float startDelay = 0.2f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float holdDuration = 0.5f;

    [Header("泡の揺れ")]
    [SerializeField] private float swayAmount = 15f;
    [SerializeField] private float swaySpeed = 3f;
    [SerializeField] private float breathingAmount = 0.025f;

    [Header("泡の破裂")]
    [SerializeField] private float popDuration = 0.35f;
    [SerializeField] private float popScaleMultiplier = 1.08f;

    private bool isPlaying = false;

    public void PlayTransition()
    {
        if (isPlaying)
            return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isPlaying = true;
        // クリア演出中は新しい文字を生成させない
        PixelBattleTextController.SuppressTextDisplay = true;
        // クリア演出開始時にHIT・PERFECTの文字を消去
        PixelBattleTextController.ClearAllTexts();

        // ① ゲーム全体を徐々にスローモーションにする
        yield return StartCoroutine(SlowMotionRoutine());

        // ② 泡の演出が始まるまで待つ
        yield return new WaitForSecondsRealtime(startDelay);

        if (bubbleImage == null)
        {
            ShowResultImmediately();
            Time.timeScale = 1f;
            isPlaying = false;
            yield break;
        }

        Image image = bubbleImage.GetComponent<Image>();

        // Canvasの大きさを取得
        RectTransform canvasRect =
            bubbleImage.GetComponentInParent<Canvas>()
                .GetComponent<RectTransform>();

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float bubbleSize = Mathf.Min(
            bubbleImage.rect.width,
            bubbleImage.rect.height
        );

        if (bubbleSize <= 0f)
            bubbleSize = 1f;

        // 画面全体を覆う泡の大きさ
        float targetScale =
            Mathf.Sqrt(
                canvasWidth * canvasWidth +
                canvasHeight * canvasHeight
            ) / bubbleSize * 1.35f;

        // 泡を最初から画面全体の大きさにする
        bubbleImage.gameObject.SetActive(true);
        bubbleImage.anchoredPosition = Vector2.zero;
        bubbleImage.localScale = Vector3.one * targetScale;

        if (image != null)
        {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }

        // ③ 泡をフェードイン
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (image != null)
            {
                Color color = image.color;
                color.a = smoothT;
                image.color = color;
            }

            AnimateBubble(timer, targetScale);

            yield return null;
        }

        if (image != null)
        {
            Color color = image.color;
            color.a = 1f;
            image.color = color;
        }

        // ④ 泡に包まれた状態で少し待つ
        timer = 0f;

        while (timer < holdDuration)
        {
            timer += Time.unscaledDeltaTime;

            AnimateBubble(
                fadeDuration + timer,
                targetScale
            );

            yield return null;
        }

        // ⑤ 泡の奥にリザルト画面を準備
        ClearResultAnimation resultAnimation = null;

        if (clearPanel != null)
        {
            resultAnimation =
                clearPanel.GetComponent<ClearResultAnimation>();

            if (resultAnimation != null)
                resultAnimation.PrepareAnimation();

            clearPanel.SetActive(true);
        }

        // ⑥ 泡が割れるように消える
        Vector2 popStartPosition = bubbleImage.anchoredPosition;
        Vector3 popStartScale = bubbleImage.localScale;

        timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / popDuration);
            float eased = t * t;

            bubbleImage.localScale = Vector3.Lerp(
                popStartScale,
                Vector3.one * targetScale * popScaleMultiplier,
                eased
            );

            bubbleImage.anchoredPosition =
                Vector2.Lerp(popStartPosition, Vector2.zero, t);

            if (image != null)
            {
                Color color = image.color;
                color.a = 1f - t;
                image.color = color;
            }

            yield return null;
        }

        // 泡を完全に消す
        bubbleImage.gameObject.SetActive(false);

        // ⑦ リザルトアニメーション開始
        if (resultAnimation != null)
            resultAnimation.PlayAnimation();

        // ゲームの時間を通常に戻す
        Time.timeScale = 1f;

        isPlaying = false;
    }

    // ゲーム全体を徐々にスローモーションにする
    private IEnumerator SlowMotionRoutine()
    {
        float timer = 0f;
        float startScale = Time.timeScale;

        while (timer < slowMotionDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / slowMotionDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            Time.timeScale = Mathf.Lerp(
                startScale,
                slowMotionScale,
                t
            );

            yield return null;
        }

        Time.timeScale = slowMotionScale;

        yield return new WaitForSecondsRealtime(
            slowMotionHold
        );
    }

    // 泡をゆらゆら動かす
    private void AnimateBubble(float time, float baseScale)
    {
        float swayX =
            Mathf.Sin(time * swaySpeed) * swayAmount;

        float swayY =
            Mathf.Cos(time * swaySpeed * 0.7f) *
            swayAmount * 0.5f;

        bubbleImage.anchoredPosition =
            new Vector2(swayX, swayY);

        float breathe =
            1f + Mathf.Sin(time * swaySpeed * 1.3f) *
            breathingAmount;

        bubbleImage.localScale =
            Vector3.one * baseScale * breathe;
    }

    // 泡が設定されていない場合の処理
    private void ShowResultImmediately()
    {
        if (clearPanel == null)
            return;

        ClearResultAnimation animation =
            clearPanel.GetComponent<ClearResultAnimation>();

        if (animation != null)
            animation.PrepareAnimation();

        clearPanel.SetActive(true);

        if (animation != null)
            animation.PlayAnimation();
    }

    private void OnDisable()
    {
        // 演出途中で無効化された場合も時間を戻す
        if (isPlaying)
        {
            Time.timeScale = 1f;
            isPlaying = false;
        }
    }
}
