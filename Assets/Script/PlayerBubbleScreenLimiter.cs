
using UnityEngine;

public class PlayerBubbleScreenLimiter : MonoBehaviour
{
    [Header("画面外判定")]
    [SerializeField] private float screenMargin = 0f;

    [Header("発射直後の判定猶予")]
    [SerializeField] private float spawnGraceTime = 0.05f;

    private Camera mainCamera;
    private float aliveTime;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        aliveTime += Time.deltaTime;

        if (aliveTime < spawnGraceTime)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        Vector3 viewportPosition =
            mainCamera.WorldToViewportPoint(transform.position);

        bool isOutsideScreen =
            viewportPosition.z <= 0f ||
            viewportPosition.x < -screenMargin ||
            viewportPosition.x > 1f + screenMargin ||
            viewportPosition.y < -screenMargin ||
            viewportPosition.y > 1f + screenMargin;

        if (isOutsideScreen)
        {
            Destroy(gameObject);
        }
    }
}
