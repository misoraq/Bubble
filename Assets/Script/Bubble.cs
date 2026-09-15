using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("シャボン玉")]
    [SerializeField] private float startScale = 0.05f;
    [SerializeField] private float maxScale = 0.2f;
    [SerializeField] private float growSpeed = 0.1f;

    private float moveDirection;
    private bool isCharging = false;
    private bool isShot = false;

    private void Start()
    {
        // 最初は小さい状態
        transform.localScale = Vector3.one * startScale;
    }

    public void StartCharging()
    {
        isCharging = true;
        isShot = false;
    }

    public void Charge()
    {
        if (!isCharging)
        {
            return;
        }

        // 徐々に大きくする
        float newScale =
            transform.localScale.x + growSpeed * Time.deltaTime;

        // 最大サイズを超えないようにする
        newScale = Mathf.Min(newScale, maxScale);

        transform.localScale = Vector3.one * newScale;
    }

    public void SetDirection(float direction)
    {
        moveDirection = direction;
    }

    public void Shoot()
    {
        isCharging = false;
        isShot = true;
    }

    private void Update()
    {
        // 発射されるまでは動かさない
        if (!isShot)
        {
            return;
        }

        // 横方向へ移動
        transform.position +=
            Vector3.right * moveDirection * moveSpeed * Time.deltaTime;
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