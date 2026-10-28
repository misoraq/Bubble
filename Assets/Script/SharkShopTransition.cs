using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SharkShopTransition : MonoBehaviour
{
    [Header("サメの画像（Canvas直下のUI Image）")]
    [SerializeField] private RectTransform sharkImage;

    [Header("ショップを開くボタン")]
    [SerializeField] private Button shopButton;

    [Header("ショップ画面")]
    [SerializeField] private GameObject shopPanel;

    [Header("横切る時間（秒）")]
    [Min(0.1f)]
    [SerializeField] private float moveDuration = 1.6f;

    [Header("泳ぐ高さ（Canvas中央を0とする）")]
    [SerializeField] private float sharkY = 0f;

    [Header("サメの大きさ（元画像に対する倍率）")]
    [Min(0.1f)]
    [SerializeField] private float sharkScale = 1.5f;

    [Header("上下に泳ぐ動き")]
    [Min(0f)]
    [SerializeField] private float bobAmplitude = 25f;
    [Min(0f)]
    [SerializeField] private float bobFrequency = 2f;

    [Header("体を傾ける角度")]
    [Range(0f, 30f)]
    [SerializeField] private float tiltAngle = 6f;

    [Header("画面外の余白")]
    [Min(0f)]
    [SerializeField] private float extraMargin = 40f;

    private bool isPlaying;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Awake()
    {
        if (sharkImage != null)
        {
            originalScale = sharkImage.localScale;
            originalRotation = sharkImage.localRotation;
            sharkImage.gameObject.SetActive(false);
        }
    }

    public void OpenShopWithShark()
    {
        if (isPlaying || sharkImage == null) return;
        StartCoroutine(PlaySharkTransition());
    }

    private IEnumerator PlaySharkTransition()
    {
        isPlaying = true;

        if (shopButton != null)
            shopButton.interactable = false;

        // sharkImage は Canvas の直下に置き、Anchor / Pivot を中央にする。
        RectTransform parentRect = sharkImage.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogError("SharkShopTransition: サメ画像の親に RectTransform が必要です。");
            if (shopButton != null) shopButton.interactable = true;
            isPlaying = false;
            yield break;
        }

        sharkImage.localScale = originalScale * sharkScale;
        sharkImage.localRotation = originalRotation;
        sharkImage.gameObject.SetActive(true);
        sharkImage.SetAsLastSibling();

        // 画像の横幅を考慮し、左右とも画面の外まで完全に移動させる。
        float halfSharkWidth = sharkImage.rect.width * Mathf.Abs(sharkImage.localScale.x) * 0.5f;
        float startX = parentRect.rect.xMin - halfSharkWidth - extraMargin;
        float endX = parentRect.rect.xMax + halfSharkWidth + extraMargin;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float phase = t * Mathf.PI * 2f * bobFrequency;
            float y = sharkY + Mathf.Sin(phase) * bobAmplitude;

            sharkImage.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), y);
            sharkImage.localRotation = originalRotation *
                Quaternion.Euler(0f, 0f, Mathf.Cos(phase) * tiltAngle);

            yield return null;
        }

        sharkImage.gameObject.SetActive(false);
        sharkImage.localRotation = originalRotation;
        sharkImage.localScale = originalScale;

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (shopButton != null)
            shopButton.interactable = true;

        isPlaying = false;
    }
}
