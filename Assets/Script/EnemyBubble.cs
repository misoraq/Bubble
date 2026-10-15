
using UnityEngine;

public class EnemyBubble : MonoBehaviour
{
    [Header("泡の大きさ")]
    [SerializeField] private float startScale = 0.1f;
    [SerializeField] private float maxScale = 0.35f;
    [SerializeField] private float growSpeed = 0.2f;

    [Header("発射")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("泡が大きくなるときの移動距離")]
    [SerializeField] private float maxChargeOffset = 0.25f;

    private Vector3 startLocalPosition;
    private bool isCharging = true;
    private bool isShot = false;

    // 左右・斜め方向の両方に対応
    private Vector2 moveDirection;

    // この泡を発射した敵
    private Enemy ownerEnemy;

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;

        // チャージ開始時の位置を保存
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isCharging)
        {
            // チャージ中に敵が泡に捕まった場合はキャンセル
            if (ownerEnemy != null &&
     (ownerEnemy.IsBubbled || ownerEnemy.IsStunned))
            {
                Destroy(gameObject);
                return;
            }

            // 泡を徐々に大きくする
            float newScale =
                transform.localScale.x + growSpeed * Time.deltaTime;

            newScale = Mathf.Min(newScale, maxScale);

            transform.localScale =
                Vector3.one * newScale;

            // 泡の成長割合を0～1で取得
            float inflateAmount =
                Mathf.InverseLerp(
                    startScale,
                    maxScale,
                    newScale
                );

            // 泡が大きくなるにつれて前に移動
            float chargeOffset =
                Mathf.Lerp(
                    0f,
                    maxChargeOffset,
                    inflateAmount
                );

            transform.localPosition =
                startLocalPosition +
                Vector3.left * chargeOffset;

            // 敵の膨張アニメーション
            if (ownerEnemy != null)
            {
                ownerEnemy.SyncInflate(inflateAmount);
            }

            // 最大サイズになったら発射
            if (newScale >= maxScale)
            {
                Shoot();
            }
        }

        if (isShot)
        {
            // 指定された方向へ泡を移動
            transform.position +=
                (Vector3)moveDirection *
                moveSpeed *
                Time.deltaTime;
        }
    }

    private void Shoot()
    {
        isCharging = false;
        isShot = true;

        // 敵から切り離す
        transform.SetParent(null);

        if (ownerEnemy != null)
        {
            ownerEnemy.ResetInflateSmooth(0.6f);

            // 敵の攻撃終了
            ownerEnemy.EndAttack();
        }
    }

    // 新規：斜め方向の発射に対応
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // 既存の左右攻撃にも対応
    public void SetDirection(float direction)
    {
        moveDirection = new Vector2(direction, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 敵の泡同士が衝突した場合
        EnemyBubble otherBubble =
            other.GetComponentInParent<EnemyBubble>();

        if (otherBubble != null && otherBubble != this)
        {
            // 発射済みの泡同士なら両方消す
            if (isShot && otherBubble.isShot)
            {
                Destroy(otherBubble.gameObject);
                Destroy(gameObject);
            }

            return;
        }

        // プレイヤーに命中した場合

        if (other.CompareTag("Player"))
        {
            // チャージ中の泡はプレイヤーに影響しない
            if (isCharging || !isShot)
            {
                return;
            }

            Debug.Log("発射された敵の泡がPlayerに命中！");

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
