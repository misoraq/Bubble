
using UnityEngine;
using UnityEngine.UI;

public class DangerFlash : MonoBehaviour
{
    [Header("赤点滅用Image")]
    [SerializeField] private Image flashImage;

    [Header("点滅速度")]
    [SerializeField] private float startFlashSpeed = 5f;
    [SerializeField] private float endFlashSpeed = 18f;

    [Header("赤色の濃さ")]
    [SerializeField] private float startAlpha = 0.15f;
    [SerializeField] private float endAlpha = 0.5f;

    private bool isFlashing = false;
    private float flashTimer = 0f;
    private float elapsedTime = 0f;
    private float totalDuration = 3f;

    private void Update()
    {
        if (!isFlashing || flashImage == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        // 0 = 開始直後、1 = 死亡直前
        float progress = Mathf.Clamp01(
            elapsedTime / totalDuration
        );

        // 時間経過で点滅速度を上げる
        float currentSpeed = Mathf.Lerp(
            startFlashSpeed,
            endFlashSpeed,
            progress
        );

        // 時間経過で赤色を濃くする
        float currentAlpha = Mathf.Lerp(
            startAlpha,
            endAlpha,
            progress
        );

        flashTimer += Time.deltaTime * currentSpeed;

        float alpha =
            (Mathf.Sin(flashTimer) + 1f)
            * 0.5f * currentAlpha;

        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }

    public void StartFlash()
    {
        StartFlash(3f);
    }

    public void StartFlash(float duration)
    {
        isFlashing = true;
        flashTimer = 0f;
        elapsedTime = 0f;
        totalDuration = Mathf.Max(duration, 0.01f);

        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
        }
    }

    public void StopFlash()
    {
        isFlashing = false;

        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
        }
    }
}
