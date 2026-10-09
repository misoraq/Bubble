
using System.Collections;
using UnityEngine;

public class GameOverDropUI : MonoBehaviour
{
    [Header("GAME OVER画像")]
    [SerializeField] private RectTransform gameOverImage;

    [Header("落下設定")]
    [SerializeField] private float startOffsetY = 900f;
    [SerializeField] private float dropDuration = 0.6f;

    [Header("バウンド設定")]
    [SerializeField] private float bounceHeight = 80f;
    [SerializeField] private float bounceDuration = 0.35f;

    [Header("リトライボタン")]
    [SerializeField] private CanvasGroup retryButtonGroup;

    [Header("リトライボタン表示設定")]
    [SerializeField] private float retryWaitDuration = 2f;
    [SerializeField] private float retryFadeDuration = 0.8f;
    private Vector2 targetPosition;

    private void Awake()
    {
        if (gameOverImage == null)
            return;

        targetPosition = gameOverImage.anchoredPosition;

        // 最初から画面上部に待機させる
        gameOverImage.anchoredPosition =
            targetPosition + Vector2.up * startOffsetY;
        if (retryButtonGroup != null)
        {
            retryButtonGroup.alpha = 0f;
            retryButtonGroup.interactable = false;
            retryButtonGroup.blocksRaycasts = false;
        }
    }

    public void PlayDrop()
    {
        if (gameOverImage == null)
        {
            Debug.LogWarning("GameOverImageが設定されていません！");
            return;
        }

        Debug.Log("GAME OVER落下演出開始");

        StopAllCoroutines();
        StartCoroutine(DropSequence());
    }

    private IEnumerator DropSequence()
    {
        Vector2 startPosition =
            targetPosition + Vector2.up * startOffsetY;

        gameOverImage.anchoredPosition = startPosition;

        // 上から落下
        float timer = 0f;

        while (timer < dropDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(dropDuration, 0.01f)
            );

            // 加速しながら落下
            t = t * t;

            gameOverImage.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        // 着地後に一度跳ねる
        timer = 0f;

        while (timer < bounceDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(bounceDuration, 0.01f)
            );

            float height =
                Mathf.Sin(t * Mathf.PI) * bounceHeight;

            gameOverImage.anchoredPosition =
                targetPosition + Vector2.up * height;

            yield return null;
        }

        gameOverImage.anchoredPosition = targetPosition;
        // バウンド終了後、指定秒数待機
        yield return new WaitForSecondsRealtime(retryWaitDuration);

        // リトライボタンをフェードイン
        if (retryButtonGroup != null)
        {
            float fadeTimer = 0f;

            while (fadeTimer < retryFadeDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    fadeTimer / Mathf.Max(retryFadeDuration, 0.01f)
                );

                // ゆっくり自然に表示
                t = Mathf.SmoothStep(0f, 1f, t);

                retryButtonGroup.alpha = t;

                yield return null;
            }

            retryButtonGroup.alpha = 1f;
            retryButtonGroup.interactable = true;
            retryButtonGroup.blocksRaycasts = true;
        }
    }
}
