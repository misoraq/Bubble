
using UnityEngine;

public class ElectricStunVisual : MonoBehaviour
{
    [Header("水鉄砲の見た目")]
    [SerializeField] private SpriteRenderer gunRenderer;

    [Header("電気エフェクト")]
    [SerializeField] private GameObject electricEffect;

    [Header("感電中の色")]
    [SerializeField]
    private Color stunnedColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);


    [Header("水の残量ゲージ")]
    [SerializeField] private SpriteRenderer waterGaugeRenderer;

    private Color originalGaugeColor;


    [Header("点滅速度")]
    [SerializeField] private float blinkSpeed = 12f;

    private Color originalColor;
    private bool isStunned;

    private void Awake()
    {
        if (waterGaugeRenderer != null)
        {
            originalGaugeColor = waterGaugeRenderer.color;
        }
        if (gunRenderer != null)
        {
            originalColor = gunRenderer.color;
        }

        if (electricEffect != null)
        {
            electricEffect.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isStunned || electricEffect == null)
        {
            return;
        }

        // 電気エフェクトをパチパチ点滅させる
        float blink = Mathf.Sin(Time.time * blinkSpeed);

        electricEffect.SetActive(blink > 0f);
    }

    public void StartStunVisual()
    {
        isStunned = true;

        if (gunRenderer != null)
        {
            gunRenderer.color = stunnedColor;
        }

        if (electricEffect != null)
        {
            electricEffect.SetActive(true);
        }
        // 感電中は残量ゲージも灰色にする
        if (waterGaugeRenderer != null)
        {
            waterGaugeRenderer.color = stunnedColor;
        }
    }

    public void EndStunVisual()
    {
        isStunned = false;

        if (gunRenderer != null)
        {
            gunRenderer.color = originalColor;
        }

        if (electricEffect != null)
        {
            electricEffect.SetActive(false);
        }
        // 感電終了時にゲージの色を戻す
        if (waterGaugeRenderer != null)
        {
            waterGaugeRenderer.color = originalGaugeColor;
        }
    }
}
