
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SeaweedSwayUI : MonoBehaviour
{
    [Header("óhÇÍÇÃê›íË")]
    [SerializeField] private float swayAngle = 6f;
    [SerializeField] private float swaySpeed = 1.5f;

    [Header("å¬ëÃÇ≤Ç∆ÇÃÇ∏ÇÍ")]
    [SerializeField] private float phaseOffset = 0f;

    private RectTransform rectTransform;
    private Quaternion initialRotation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialRotation = rectTransform.localRotation;
    }

    private void Update()
    {
        float time = Time.unscaledTime * swaySpeed + phaseOffset;
        float angle = Mathf.Sin(time) * swayAngle;

        rectTransform.localRotation =
            initialRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
