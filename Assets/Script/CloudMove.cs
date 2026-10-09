
using UnityEngine;

public class CloudMove : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 0.5f;

    [Header("画面端からの余白")]
    [SerializeField] private float edgeOffset = 0.5f;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (mainCamera == null || spriteRenderer == null)
            return;

        // 左から右へ移動
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        float cameraDistance =
            Mathf.Abs(transform.position.z - mainCamera.transform.position.z);

        float rightEdge = mainCamera.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, cameraDistance)
        ).x;

        float leftEdge = mainCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, cameraDistance)
        ).x;

        float halfWidth = spriteRenderer.bounds.extents.x;

        // 雲が画面右端から完全に出たら左端に戻す
        if (transform.position.x - halfWidth > rightEdge + edgeOffset)
        {
            Vector3 position = transform.position;
            position.x = leftEdge - halfWidth - edgeOffset;
            transform.position = position;
        }
    }
}
