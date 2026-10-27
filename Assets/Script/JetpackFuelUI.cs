
using UnityEngine;
using UnityEngine.UI;

public class JetpackFuelUI : MonoBehaviour
{
    [Header("燃料Slider")]
    [SerializeField] private Slider fuelSlider;

    [Header("オレンジ色の燃料画像")]
    [SerializeField] private Image fuelFillImage;

    private void Awake()
    {
        if (fuelFillImage != null)
        {
            fuelFillImage.type = Image.Type.Filled;
            fuelFillImage.fillMethod = Image.FillMethod.Horizontal;
            fuelFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    private void LateUpdate()
    {
        if (fuelSlider == null || fuelFillImage == null)
            return;

        // Sliderの値を0～1に変換
        float fuelRatio = fuelSlider.normalizedValue;

        // 画像自体の横幅を変えず、表示範囲だけ変更
        fuelFillImage.fillAmount = fuelRatio;
    }
}
