using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float startScale = 0.05f;
    [SerializeField] private float maxScale = 0.2f;
    [SerializeField] private float growSpeed = 0.1f;

    private float moveDirection;

    private bool isCharging = false;
    private bool isShot = false;

    private Collider2D bubbleCollider;

    private void Awake()
    {
        bubbleCollider = GetComponent<Collider2D>();

        // チャージ中は敵に当たらない
        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = false;
        }
    }

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
    }

    public void StartCharging()
    {
        isCharging = true;
        isShot = false;

        // チャージ中はCollider OFF
        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = false;
        }
    }

    public void Charge()
    {
        if (!isCharging)
        {
            return;
        }

        float newScale =
            transform.localScale.x +
            growSpeed * Time.deltaTime;

        newScale = Mathf.Min(newScale, maxScale);

        transform.localScale =
            Vector3.one * newScale;
    }

    public void SetDirection(float direction)
    {
        moveDirection = direction;
    }

    public void Shoot()
    {
        isCharging = false;
        isShot = true;

        // 発射した瞬間にCollider ON
        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = true;
        }
    }

    private void Update()
    {
        if (!isShot)
        {
            return;
        }

        transform.position +=
            Vector3.right *
            moveDirection *
            moveSpeed *
            Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            bool isFullCharge =
                transform.localScale.x >= maxScale * 0.99f;

            enemy.HitByBubble(isFullCharge);

            Destroy(gameObject);
        }
    }
}