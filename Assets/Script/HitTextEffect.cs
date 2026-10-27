
using UnityEngine;
using PixelBattleText;

public class HitTextEffect : MonoBehaviour
{
    [Header("HIT文字のアニメーション")]
    [SerializeField] private TextAnimation hitAnimation;

    [Header("PERFECT文字のアニメーション")]
    [SerializeField] private TextAnimation perfectAnimation;

    [Header("通常の表示位置")]
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 0.5f, 0f);

    [Header("追加ヒットの位置調整")]
    [SerializeField] private float extraHitOffsetX = 0.8f;

    [Header("PERFECTの位置調整")]
    [SerializeField] private float perfectOffsetY = 0.6f;

    public void ShowHit(Transform enemyTransform)
    {
        if (hitAnimation == null || enemyTransform == null)
            return;

        PixelBattleTextController.DisplayFollowText(
            "HIT!",
            hitAnimation,
            enemyTransform,
            offset
        );
    }

    public void ShowExtraHit(
        Transform enemyTransform,
        int hitCount)
    {
        if (hitAnimation == null || enemyTransform == null)
            return;

        Vector3 hitOffset = offset;

        // 1発目は左、2発目は右
        if (hitCount == 1)
        {
            hitOffset.x -= extraHitOffsetX;
        }
        else
        {
            hitOffset.x += extraHitOffsetX;
        }

        PixelBattleTextController.DisplayFollowText(
            "HIT!",
            hitAnimation,
            enemyTransform,
            hitOffset
        );
    }

    public void ShowPerfect(Transform enemyTransform)
    {
        if (enemyTransform == null || Camera.main == null)
            return;

        TextAnimation animation =
            perfectAnimation != null
            ? perfectAnimation
            : hitAnimation;

        if (animation == null)
            return;

        Vector3 worldPosition =
            enemyTransform.position + offset;

        worldPosition.y += perfectOffsetY;

        Vector3 viewportPosition =
            Camera.main.WorldToViewportPoint(worldPosition);

        GameSoundManager.Instance?.PlayPerfectText();

        PixelBattleTextController.DisplayText(
            "PERFECT!",
            animation,
            new Vector2(
                viewportPosition.x,
                viewportPosition.y
            )
        );
    }
}
