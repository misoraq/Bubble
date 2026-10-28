
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SharkShopTransition : MonoBehaviour
{
    [Header("サメ（Canvas直下のUI Image）")]
    [SerializeField] private RectTransform sharkImage;
    [SerializeField] private Button shopButton;

    [Header("ショップ画面（ショップ全体の親）")]
    [SerializeField] private GameObject shopPanel;

    [Tooltip("既存のショップを開く処理が必要な場合に登録")]
    [SerializeField] private UnityEvent onOpenShop;

    [Header("サメの動き")]
    [Min(0.1f)]
    [SerializeField] private float moveDuration = 1.1f;

    [SerializeField] private float sharkY = 0f;

    [Min(0.1f)]
    [SerializeField] private float sharkScale = 1.5f;

    [Min(0f)]
    [SerializeField] private float bobAmplitude = 25f;

    [Min(0f)]
    [SerializeField] private float bobFrequency = 2f;

    [Range(0f, 30f)]
    [SerializeField] private float tiltAngle = 6f;

    [Min(0f)]
    [SerializeField] private float extraMargin = 40f;

    [Header("泡の演出")]
    [SerializeField] private Sprite bubbleSprite;

    [Min(0)]
    [SerializeField] private int bubbleCount = 12;

    [Min(1f)]
    [SerializeField] private float bubbleSize = 30f;

    [SerializeField] private float bubbleStartTime = 0.8f;
    [SerializeField] private float bubbleEndTime = 1.2f;

    [Min(0f)]
    [SerializeField] private float bubbleRise = 95f;

    [Min(0f)]
    [SerializeField] private float bubbleSpread = 80f;

    [Header("ショップ表示演出")]
    [Min(0f)]
    [SerializeField] private float shopAppearTime = 1.1f;

    [Min(0.05f)]
    [SerializeField] private float shopFadeDuration = 0.4f;

    [Range(0.1f, 1f)]
    [SerializeField] private float shopStartScale = 0.88f;

    private bool isPlaying;

    private Vector3 originalSharkScale;
    private Quaternion originalSharkRotation;
    private Vector3 originalShopScale;

    private RectTransform shopRect;
    private CanvasGroup shopCanvasGroup;

    private readonly List<GameObject> spawnedBubbles =
        new List<GameObject>();

    private void Awake()
    {
        if (sharkImage != null)
        {
            originalSharkScale = sharkImage.localScale;
            originalSharkRotation = sharkImage.localRotation;

            sharkImage.gameObject.SetActive(false);
        }

        if (shopPanel != null)
        {
            shopRect = shopPanel.GetComponent<RectTransform>();

            if (shopRect != null)
            {
                originalShopScale = shopRect.localScale;
            }

            shopCanvasGroup = shopPanel.GetComponent<CanvasGroup>();

            if (shopCanvasGroup == null)
            {
                shopCanvasGroup = shopPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    public void OpenShopWithShark()
    {
        if (isPlaying)
        {
            return;
        }

        if (sharkImage == null || shopPanel == null || shopRect == null)
        {
            Debug.LogError(
                "SharkShopTransition: Shark ImageとShop Panelを設定してください。"
            );

            return;
        }

        StartCoroutine(PlaySharkTransition());
    }

    private IEnumerator PlaySharkTransition()
    {
        isPlaying = true;

        if (shopButton != null)
        {
            shopButton.interactable = false;
        }

        RectTransform canvasRect =
            sharkImage.parent as RectTransform;

        if (canvasRect == null)
        {
            Debug.LogError(
                "SharkShopTransition: Shark ImageはCanvasの子にしてください。"
            );

            FinishTransition();
            yield break;
        }

        // 既存のショップ初期化処理
        onOpenShop?.Invoke();

        shopPanel.SetActive(true);

        // ショップを透明・縮小状態にする
        shopRect.localScale =
            originalShopScale * shopStartScale;

        shopCanvasGroup.alpha = 0f;
        shopCanvasGroup.interactable = false;
        shopCanvasGroup.blocksRaycasts = false;

        // サメを表示
        sharkImage.localScale =
            originalSharkScale * sharkScale;

        sharkImage.localRotation =
            originalSharkRotation;

        sharkImage.gameObject.SetActive(true);
        sharkImage.SetAsLastSibling();

        // 画面外から画面外までの移動距離
        float halfWidth =
            sharkImage.rect.width *
            Mathf.Abs(sharkImage.localScale.x) * 0.5f;

        float startX =
            canvasRect.rect.xMin - halfWidth - extraMargin;

        float endX =
            canvasRect.rect.xMax + halfWidth + extraMargin;

        float totalTime = Mathf.Max(
            moveDuration,
            bubbleEndTime,
            shopAppearTime + shopFadeDuration
        );

        float elapsed = 0f;
        int bubblesCreated = 0;

        while (elapsed < totalTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(elapsed / moveDuration);

            // サメの横移動・上下の揺れ・傾き
            if (elapsed <= moveDuration)
            {
                float phase =
                    t * Mathf.PI * 2f * bobFrequency;

                float y =
                    sharkY + Mathf.Sin(phase) * bobAmplitude;

                sharkImage.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(startX, endX, t),
                        y
                    );

                sharkImage.localRotation =
                    originalSharkRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        Mathf.Cos(phase) * tiltAngle
                    );
            }
            else if (sharkImage.gameObject.activeSelf)
            {
                sharkImage.gameObject.SetActive(false);
            }

            // 泡を発生させる
            if (bubbleSprite != null &&
                bubbleCount > 0 &&
                elapsed >= bubbleStartTime)
            {
                float bubbleProgress =
                    Mathf.Clamp01(
                        (elapsed - bubbleStartTime) /
                        Mathf.Max(
                            0.01f,
                            bubbleEndTime - bubbleStartTime
                        )
                    );

                int targetCount =
                    Mathf.Min(
                        bubbleCount,
                        Mathf.FloorToInt(
                            bubbleProgress * bubbleCount
                        ) + 1
                    );

                while (bubblesCreated < targetCount)
                {
                    SpawnBubble(
                        canvasRect,
                        startX,
                        endX,
                        elapsed
                    );

                    bubblesCreated++;
                }
            }

            UpdateBubbles(Time.unscaledDeltaTime);

            // ショップの拡大・フェードイン
            if (elapsed >= shopAppearTime)
            {
                float appear =
                    Mathf.Clamp01(
                        (elapsed - shopAppearTime) /
                        shopFadeDuration
                    );

                float smooth =
                    appear * appear * (3f - 2f * appear);

                shopCanvasGroup.alpha = smooth;

                shopRect.localScale =
                    Vector3.Lerp(
                        originalShopScale * shopStartScale,
                        originalShopScale,
                        smooth
                    );
            }

            yield return null;
        }

        // 演出終了
        sharkImage.gameObject.SetActive(false);

        sharkImage.localScale = originalSharkScale;
        sharkImage.localRotation = originalSharkRotation;

        shopRect.localScale = originalShopScale;

        shopCanvasGroup.alpha = 1f;
        shopCanvasGroup.interactable = true;
        shopCanvasGroup.blocksRaycasts = true;

        ClearBubbles();
        FinishTransition();
    }

    private void SpawnBubble(
        RectTransform parent,
        float startX,
        float endX,
        float elapsed
    )
    {
        float sharkT =
            Mathf.Clamp01(elapsed / moveDuration);

        float x =
            Mathf.Lerp(startX, endX, sharkT);

        float phase =
            sharkT * Mathf.PI * 2f * bobFrequency;

        float y =
            sharkY + Mathf.Sin(phase) * bobAmplitude;

        GameObject obj = new GameObject(
            "SharkBubble",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rt =
            obj.GetComponent<RectTransform>();

        rt.SetParent(parent, false);

        rt.anchorMin =
            rt.anchorMax = new Vector2(0.5f, 0.5f);

        rt.pivot = new Vector2(0.5f, 0.5f);

        float size =
            bubbleSize * Random.Range(0.5f, 1.3f);

        rt.sizeDelta = new Vector2(size, size);

        rt.anchoredPosition =
            new Vector2(
                x - sharkImage.rect.width *
                sharkScale * 0.45f +
                Random.Range(-25f, 25f),

                y + Random.Range(-25f, 25f)
            );

        Image img = obj.GetComponent<Image>();

        img.sprite = bubbleSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        img.color =
            new Color(1f, 1f, 1f, 0.85f);

        spawnedBubbles.Add(obj);
    }

    private void UpdateBubbles(float deltaTime)
    {
        foreach (GameObject obj in spawnedBubbles)
        {
            if (obj == null)
            {
                continue;
            }

            RectTransform rt =
                obj.GetComponent<RectTransform>();

            rt.anchoredPosition +=
                new Vector2(
                    bubbleSpread * 0.25f * deltaTime,
                    bubbleRise * deltaTime
                );

            Image img = obj.GetComponent<Image>();

            Color c = img.color;

            c.a =
                Mathf.Max(
                    0f,
                    c.a - deltaTime * 1.7f
                );

            img.color = c;
        }
    }

    private void ClearBubbles()
    {
        foreach (GameObject obj in spawnedBubbles)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawnedBubbles.Clear();
    }

    private void FinishTransition()
    {
        if (shopButton != null)
        {
            shopButton.interactable = true;
        }

        isPlaying = false;
    }
}
