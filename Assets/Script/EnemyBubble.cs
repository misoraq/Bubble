using UnityEngine;

public class EnemyBubble : MonoBehaviour
{
    [Header("–A‚Ì‘å‚«‚³")]
    [SerializeField] private float startScale = 0.1f;
    [SerializeField] private float maxScale = 0.35f;
    [SerializeField] private float growSpeed = 0.2f;

    [Header("”­Ë")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("–A‚ªŒû‚©‚ç—£‚ê‚é‹——£")]
    [SerializeField] private float maxChargeOffset = 0.25f;

    private Vector3 startLocalPosition;
    private bool isCharging = true;
    private bool isShot = false;

    private float moveDirection;

    // ‚±‚Ì–A‚ğì‚Á‚½“G
    private Enemy ownerEnemy;

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;

        // Œû‚©‚ç‚ÌÅ‰‚ÌˆÊ’u‚ğ•Û‘¶
        startLocalPosition = transform.localPosition;
    }
    private void Update()
    {
        if (isCharging)
        {
             // “G‚ªƒvƒŒƒCƒ„[‚Ì–A‚Å•ï‚Ü‚ê‚½‚çUŒ‚ƒ`ƒƒ[ƒW‚ğƒLƒƒƒ“ƒZƒ‹
        if (ownerEnemy != null && ownerEnemy.IsBubbled)
        {
                ownerEnemy.ResetInflateSmooth(0.2f);
                ownerEnemy.EndAttack();

            Destroy(gameObject);
            return;
        }
            float newScale =
                transform.localScale.x + growSpeed * Time.deltaTime;

            newScale = Mathf.Min(newScale, maxScale);

            transform.localScale =
                Vector3.one * newScale;

            // –A‚Ì–c’£—¦‚ğ0`1‚É•ÏŠ·
            float inflateAmount =
    Mathf.InverseLerp(
        startScale,
        maxScale,
        newScale
    );

            // –A‚ª‘å‚«‚­‚È‚é‚Ù‚ÇŒû‚©‚ç—£‚·
            float chargeOffset =
                Mathf.Lerp(
                    0f,
                    maxChargeOffset,
                    inflateAmount
                );

            transform.localPosition =
                startLocalPosition +
                Vector3.left * chargeOffset;

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

        // “G‚©‚ç–A‚ğØ‚è—£‚·
        transform.SetParent(null);

        if (ownerEnemy != null)
        {
            ownerEnemy.ResetInflateSmooth(0.6f);

            // “G‚ÌUŒ‚I—¹
            ownerEnemy.EndAttack();
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
            Debug.Log("“G‚Ì–A‚ªPlayer‚É–½’†I");
            WaterGun waterGun =
                FindObjectOfType<WaterGun>();

            if (waterGun != null)
            {
                waterGun.TakeDamage(1);
            }

            Destroy(gameObject);
        }
    }
    public void SetOwner(Enemy enemy)
    {
        ownerEnemy = enemy;
    }
}