using UnityEngine;

public class EnemyBubble : MonoBehaviour
{
    [Header("ñAÇÃëÂÇ´Ç≥")]
    [SerializeField] private float startScale = 0.1f;
    [SerializeField] private float maxScale = 0.35f;
    [SerializeField] private float growSpeed = 0.2f;

    [Header("î≠éÀ")]
    [SerializeField] private float moveSpeed = 4f;

    private bool isCharging = true;
    private bool isShot = false;

    private float moveDirection;

    // Ç±ÇÃñAÇçÏÇ¡ÇΩìG
    private Enemy ownerEnemy;

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        if (isCharging)
        {
            float newScale =
                transform.localScale.x + growSpeed * Time.deltaTime;

            newScale = Mathf.Min(newScale, maxScale);

            transform.localScale =
                Vector3.one * newScale;

            // ñAÇÃñcí£ó¶Ç0Å`1Ç…ïœä∑
            float inflateAmount =
                Mathf.InverseLerp(
                    startScale,
                    maxScale,
                    newScale
                );

            // ìGñ{ëÃÇ‡ìØÇ∂äÑçáÇ≈ñcÇÁÇ‹ÇπÇÈ
            if (ownerEnemy != null)
            {
                ownerEnemy.SyncInflate(inflateAmount);
            }

            if (newScale >= maxScale)
            {
                Shoot();
            }
        }

        if (isShot)
        {
            transform.position +=
                Vector3.right * moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    private void Shoot()
    {
        isCharging = false;
        isShot = true;

        // ìGÇ©ÇÁêÿÇËó£Ç∑
        transform.SetParent(null);

        if (ownerEnemy != null)
        {
            ownerEnemy.ResetInflate();
        }
    }

    public void SetDirection(float direction)
    {
        moveDirection = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ìGÇÃñAÇ™PlayerÇ…ñΩíÜÅI");

            PlayerBubbleUI bubbleUI =
                FindObjectOfType<PlayerBubbleUI>();

            if (bubbleUI != null)
            {
                bubbleUI.TakeDamage();
            }

            Destroy(gameObject);
        }
    }
    public void SetOwner(Enemy enemy)
    {
        ownerEnemy = enemy;
    }
}