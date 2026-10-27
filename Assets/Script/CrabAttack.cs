using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CrabAttack : MonoBehaviour
{
    [Header("通常時の巡回")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float screenEdgeMargin = 0.6f;

    [Header("プレイヤー発見")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float groundHeightTolerance = 1.2f;

    [Header("突進")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private float preparationTime = 0.5f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("参照")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer crabRenderer;

    [Header("突進ヒット時の水ダメージ")]
    [SerializeField] private int waterDamage = 3;

    private Rigidbody2D rb;
    private Transform player;
    private Camera mainCamera;

    private bool isAttacking;
    private bool isCharging;
    private bool hasHitPlayer;
    private bool isPaused;

    private float attackTimer;
    private float patrolDirection = 1f;
    private Vector2 chargeDirection;

    private Coroutine attackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (crabRenderer == null)
            crabRenderer = GetComponent<SpriteRenderer>();

        mainCamera = Camera.main;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        PlayAnimation("Crab_Walk");
    }

    private void Update()
    {
        if (isPaused || player == null)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (isAttacking || attackTimer > 0f)
            return;

        if (CanAttackPlayer())
        {
            attackCoroutine = StartCoroutine(ChargeRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (isPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isAttacking)
        {
            rb.velocity = isCharging
                ? chargeDirection * chargeSpeed
                : Vector2.zero;

            return;
        }

        Patrol();
    }

    private void Patrol()
    {
        if (mainCamera == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float distanceFromCamera =
            Mathf.Abs(transform.position.z -
                      mainCamera.transform.position.z);

        float leftEdge = mainCamera
            .ViewportToWorldPoint(
                new Vector3(0f, 0.5f, distanceFromCamera)
            ).x + screenEdgeMargin;

        float rightEdge = mainCamera
            .ViewportToWorldPoint(
                new Vector3(1f, 0.5f, distanceFromCamera)
            ).x - screenEdgeMargin;

        if (rb.position.x <= leftEdge)
        {
            patrolDirection = 1f;
        }
        else if (rb.position.x >= rightEdge)
        {
            patrolDirection = -1f;
        }

        // 端を越えた場合は画面内へ戻す
        float clampedX =
            Mathf.Clamp(rb.position.x, leftEdge, rightEdge);

        if (!Mathf.Approximately(rb.position.x, clampedX))
        {
            rb.position = new Vector2(clampedX, rb.position.y);
        }

        rb.velocity = new Vector2(
            patrolDirection * patrolSpeed,
            0f
        );

        UpdateFacing(patrolDirection);
    }

    private bool CanAttackPlayer()
    {
        float distanceX =
            Mathf.Abs(player.position.x - transform.position.x);

        float distanceY =
            Mathf.Abs(player.position.y - transform.position.y);

        // 横方向が近く、かつプレイヤーが地面付近にいる
        return distanceX <= detectionRange &&
               distanceY <= groundHeightTolerance;
    }

    private IEnumerator ChargeRoutine()
    {
        isAttacking = true;
        isCharging = false;
        hasHitPlayer = false;

        // 準備開始時のプレイヤー位置を基準に突進方向を決定
        chargeDirection =
            player.position.x >= transform.position.x
            ? Vector2.right
            : Vector2.left;

        UpdateFacing(chargeDirection.x);

        rb.velocity = Vector2.zero;
        PlayAnimation("Crab_Idle");

        yield return new WaitForSeconds(preparationTime);

        isCharging = true;
        PlayAnimation("Crab_Charge");

        yield return new WaitForSeconds(chargeDuration);

        isCharging = false;
        rb.velocity = Vector2.zero;

        // 突進後の巡回方向を合わせる
        patrolDirection = chargeDirection.x;

        PlayAnimation("Crab_Walk");

        attackTimer = attackCooldown;
        isAttacking = false;
        attackCoroutine = null;
    }

    private void UpdateFacing(float direction)
    {
        if (crabRenderer == null)
            return;

        // スプライトの初期状態が右向きの場合
        crabRenderer.flipX = direction < 0f;
    }

    private void PlayAnimation(string stateName)
    {
        if (animator != null)
            animator.Play(stateName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void TryHitPlayer(Collider2D other)
    {
        // 突進中以外は接触しても何も起こらない
        if (!isCharging || hasHitPlayer || isPaused)
            return;

        PlayerMove playerMove =
            other.GetComponentInParent<PlayerMove>();

        if (playerMove == null)
            return;

        // フェーズ演出中は攻撃を成立させない
        if (PhaseAnnouncementUI.IsPlaying)
            return;

        hasHitPlayer = true;
        playerMove.HitByCrabCharge(chargeDirection);

        // 1回の突進につき1度だけ水を減らす
        WaterGun waterGun = playerMove.GetComponentInChildren<WaterGun>();
        if (waterGun != null && waterDamage > 0)
            waterGun.TakeDamage(waterDamage);
    }

    // 通常泡による停止・捕獲処理と連携するためのメソッド
    public void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;

        isPaused = paused;

        if (isPaused)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            isAttacking = false;
            isCharging = false;
            rb.velocity = Vector2.zero;

            PlayAnimation("Crab_Idle");
        }
        else
        {
            attackTimer = attackCooldown;
            PlayAnimation("Crab_Walk");
        }
    }
}
