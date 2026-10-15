using UnityEngine;
using PixelBattleText;

public class HitTextEffect : MonoBehaviour
{
    [Header("HIT文字のアニメーション")]
    [SerializeField] private TextAnimation hitAnimation;

    [Header("表示位置の調整")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);


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
}