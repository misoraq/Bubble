using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float startScale = 0.05f;
    [SerializeField] private float maxScale = 0.2f;
    [SerializeField] private float growSpeed = 0.1f;

    [Header("チャージ中の前進")]
    [SerializeField] private float maxChargeOffset = 0.15f;

    private float previousChargeOffset = 0f;

    private Vector2 moveDirection;

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
        previousChargeOffset = 0f;

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


        // =========================
        // チャージ量に応じて少し前へ出す
        // =========================

        float chargeAmount =
            Mathf.InverseLerp(
                startScale,
                maxScale,
                newScale
            );

        float chargeOffset =
            Mathf.Lerp(
                0f,
                maxChargeOffset,
                chargeAmount
            );

        float offsetDelta =
            chargeOffset -
            previousChargeOffset;

        transform.position +=
      (Vector3)moveDirection *
      offsetDelta;

        previousChargeOffset = chargeOffset;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
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
      (Vector3)moveDirection *
      moveSpeed *
      Time.deltaTime;
    }




    private void OnTriggerEnter2D(Collider2D other)
    {
        // ボスの弱点と胴体を通常敵より先に判定する。
        // 弱点は専用のTrigger Collider2Dを使用する。
        GiantPufferWeakPoint weakPoint = other.GetComponent<GiantPufferWeakPoint>();
        if (weakPoint != null)
        {
            weakPoint.TryHit();
            Destroy(gameObject);
            return;
        }

        GiantPufferBoss boss = other.GetComponentInParent<GiantPufferBoss>();
        if (boss != null)
        {
            // 胴体は無敵。弾だけ消す。
            Destroy(gameObject);
            return;
        }

        // 敵がチャージしている攻撃用の泡は無視
        EnemyBubble enemyBubble =
            other.GetComponentInParent<EnemyBubble>();

        BubbleFinish bubbleFinish =
            other.GetComponentInParent<BubbleFinish>();

        if (bubbleFinish != null && bubbleFinish.IsCaptured())
        {
            // 捕獲済みの敵には通常弾・フルチャージ弾を
            // 区別せず、追加ヒットとして数える
            bubbleFinish.AddHit();

            Destroy(gameObject);
            return;
        }

        if (enemyBubble != null)
            return;

        bool isFullCharge = IsFullCharge();

        Enemy enemy =
            other.GetComponentInParent<Enemy>();

        JellyfishBubbleHit jellyfish =
            other.GetComponentInParent<JellyfishBubbleHit>();

        CrabBubbleHit crab =
            other.GetComponentInParent<CrabBubbleHit>();

        Transform hitTarget = null;

        if (enemy != null)
        {
            enemy.HitByBubble(isFullCharge);
            hitTarget = enemy.transform;
        }
        else if (jellyfish != null)
        {
            jellyfish.HitByBubble(isFullCharge);
            hitTarget = jellyfish.transform;
        }
        else if (crab != null)
        {
            crab.HitByBubble(isFullCharge);
            hitTarget = crab.transform;
        }

        if (hitTarget == null)
            return;

        // 敵への命中時にHIT音を再生
        GameSoundManager.Instance?.PlayEnemyHit();

        HitTextEffect hitTextEffect =
            FindObjectOfType<HitTextEffect>();

        if (hitTextEffect != null)
            hitTextEffect.ShowHit(hitTarget);

        Destroy(gameObject);
    }



    public bool IsFullCharge()
    {
        return transform.localScale.x >= maxScale * 0.99f;
    }
}