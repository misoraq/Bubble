
using UnityEngine;

public class WaterGaugeUI : MonoBehaviour
{
    [Header("水鉄砲")]
    [SerializeField] private WaterGun waterGun;

    [Header("水のSpriteRenderer")]
    [SerializeField] private SpriteRenderer waterSprite;

    [Header("残量スプライト 1～10")]
    [SerializeField] private Sprite[] waterSprites;

    private void Update()
    {
        if (waterGun == null || waterSprite == null)
        {
            return;
        }

        int currentWater = waterGun.GetCurrentWater();

        // 残量0なら水だけ非表示
        if (currentWater <= 0)
        {
            waterSprite.enabled = false;
            return;
        }

        if (waterSprites == null || waterSprites.Length == 0)
        {
            return;
        }

        waterSprite.enabled = true;

        int spriteIndex = Mathf.Clamp(
            currentWater - 1,
            0,
            waterSprites.Length - 1
        );

        waterSprite.sprite = waterSprites[spriteIndex];
    }
}
