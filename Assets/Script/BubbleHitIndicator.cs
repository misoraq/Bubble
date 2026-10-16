
using UnityEngine;

public class BubbleHitIndicator : MonoBehaviour
{
    [Header("表示用SpriteRenderer")]
    [SerializeField] private SpriteRenderer indicatorRenderer;

    [Header("命中回数ごとのSprite")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite oneHitSprite;
    [SerializeField] private Sprite twoHitSprite;
    [SerializeField] private Sprite threeHitSprite;

    private void Awake()
    {
        if (indicatorRenderer == null)
            indicatorRenderer = GetComponent<SpriteRenderer>();

        UpdateHits(0);
    }
    private void LateUpdate()
    {
        if (indicatorRenderer == null)
            return;

        float parentScaleX = transform.parent != null
            ? transform.parent.lossyScale.x
            : 1f;

        // ゲージのSpriteだけ左右反転を補正
        indicatorRenderer.flipX = parentScaleX < 0f;
    }
    public void UpdateHits(int currentHits)
    {
        if (indicatorRenderer == null)
            return;

        switch (currentHits)
        {
            case 0:
                indicatorRenderer.sprite = emptySprite;
                break;

            case 1:
                indicatorRenderer.sprite = oneHitSprite;
                break;

            case 2:
                indicatorRenderer.sprite = twoHitSprite;
                break;

            default:
                indicatorRenderer.sprite = threeHitSprite;
                break;
        }
    }
}
