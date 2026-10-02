using UnityEngine;
using UnityEngine.UI;

public class WaterGaugeUI : MonoBehaviour
{
    [Header("水鉄砲")]
    [SerializeField] private WaterGun waterGun;

    [Header("水のImage")]
    [SerializeField] private Image waterImage;

    [Header("残量スプライト 1～10")]
    [SerializeField] private Sprite[] waterSprites;

    private void Update()
    {
        if (waterGun == null || waterImage == null)
        {
            return;
        }

        int currentWater = waterGun.GetCurrentWater();

        // 残量0なら水を非表示
        if (currentWater <= 0)
        {
            waterImage.enabled = false;
            return;
        }

        waterImage.enabled = true;

        int spriteIndex =
            Mathf.Clamp(
                currentWater - 1,
                0,
                waterSprites.Length - 1
            );

        waterImage.sprite = waterSprites[spriteIndex];
    }
}