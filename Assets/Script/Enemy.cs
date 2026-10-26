using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("泡")]
    [SerializeField] private GameObject bubbleVisual;

    [Header("泡の大きさ")]
    [SerializeField] private float smallBubbleScale = 0.2f;
    [SerializeField] private float fullChargeBubbleScale = 0.35f;

    [Header("泡の制限時間")]
    [SerializeField] private float smallBubbleTime = 3f;
    [SerializeField] private float fullChargeBubbleTime = 7f;

    [SerializeField] private float floatSpeed = 1.5f;

    [Header("小さい泡")]
    [SerializeField] private int requiredHits = 3;
    [Header("ダッシュUI")]
    [SerializeField] private GameObject dashUI;
    private int currentHits = 0;

    [Header("通常弾による停止")]
    [SerializeField] private float stunDuration = 2f;
    private float stunTimer = 0f;
    private bool isBubbled = false;
    [Header("ダッシュ衝突")]
    [SerializeField] private float dashKnockbackSpeed = 4f;
    [SerializeField] private float dashKnockbackTime = 0.2f;

    [Header("Player追跡")]
    [SerializeField] private float chaseSpeed = 2f;

    [Header("敵の膨張")]
    [SerializeField] private float normalScale = 0.3f;
    [SerializeField] private float maxInflateScale = 0.6f;

    private float originalScaleX;
    private float originalScaleY;
    private float originalScaleZ;

    [Header("向き制御")]
    [SerializeField] private float turnThreshold = 0.8f;
    [SerializeField] private float turnCooldown = 0.2f;

    private float turnTimer = 0f;

    [Header("敵の泡攻撃")]
    [SerializeField] private GameObject enemyBubblePrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackCooldown = 3f;

    [Header("敵攻撃泡の位置")]
    [SerializeField] private float attackBubbleOffsetX = 0.15f;

    [Header("攻撃時の敵の向き")]
    [SerializeField] private Transform enemyVisual;
    [SerializeField] private float maxAimAngle = 45f;
    [SerializeField] private float aimRotateSpeed = 10f;

    [Header("攻撃予告")]
    [SerializeField] private EnemyAimLine aimLine;
    [SerializeField] private float aimWarningTime = 0.8f;
    private Coroutine aimCoroutine;

    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float bubbleTimer = 0f;
    private Transform player;
    private bool isAttacking = false;
    private bool isGameOverStopped = false;
    private bool isSpawning = false;
    [Header("画面上撃破判定")]
    [SerializeField] private float defeatHeightOffset = 0.5f;
    private SpriteRenderer spriteRenderer;
    [Header("撃破時の水回復")]
    [SerializeField] private int waterRecoveryAmount = 2;
    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
    private void Update()
    {
        if (isGameOverStopped)
        {
            return;
        }

        if (isSpawning)
        {
            return;
        }

        // 通常弾を受けている間は移動と攻撃を停止する
        if (stunTimer > 0f && !isBubbled)
        {
            stunTimer -= Time.deltaTime;

            if (rb != null)
                rb.velocity = Vector2.zero;

            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                attackTimer = attackCooldown;
            }

            return;
        }

        if (player != null && !isBubbled && !isAttacking)
        {
            ChasePlayer();
        }
        // =========================
        // 敵の向き
        // =========================

        if (player != null && !isBubbled)
        {
            turnTimer -= Time.deltaTime;

            // プレイヤーとの横方向の距離
            float distanceX =
                player.position.x - transform.position.x;

            // 真上・真下付近では最後の向きを維持する
            if (Mathf.Abs(distanceX) > turnThreshold)
            {
                bool playerIsLeft = distanceX < 0f;

                bool enemyIsFacingLeft =
                    transform.localScale.x > 0f;

                bool needTurn =
                    playerIsLeft != enemyIsFacingLeft;

                if (needTurn && turnTimer <= 0f)
                {
                    float scaleX = Mathf.Abs(transform.localScale.x);

                    transform.localScale = new Vector3(
                        playerIsLeft ? scaleX : -scaleX,
                        transform.localScale.y,
                        transform.localScale.z
                    );

                    turnTimer = turnCooldown;
                }
            }
        }

        if (player != null && !isBubbled && IsVisibleOnScreen())
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                ShootEnemyBubble();
                attackTimer = attackCooldown;
            }
        }
        if (isBubbled)
        {
            // 上へ浮かぶ
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // 完全に画面外へ出たら撃破
            if (IsAboveScreen())
            {
                Defeat();
                return;
            }

            // 泡の残り時間を減らす
            bubbleTimer -= Time.deltaTime;

            // 時間切れなら敵を解放
            if (bubbleTimer <= 0f)
            {
                BubbleFinish bubbleFinish = GetComponent<BubbleFinish>();

                // 3発目が成立している場合は泡を解除しない
                if (bubbleFinish == null || !bubbleFinish.IsFinishing)
                {
                    ReleaseBubble();
                }
            }
        }
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                rb.velocity = Vector2.zero;
            }
        }

    }

    public void HitByBubble(bool isFullCharge)
    {
        // 画面外および画面に入ってからの無敵時間中は泡攻撃を無効化
        EnemySpawnProtection protection = GetComponent<EnemySpawnProtection>();
        if (protection != null && protection.IsProtected)
            return;

        if (isBubbled || isGameOverStopped || isSpawning)
            return;

        if (isFullCharge)
        {
            stunTimer = 0f;
            CancelChargingAttack();
            GetBubbled(fullChargeBubbleScale, fullChargeBubbleTime);
            return;
        }

        // 通常弾：停止時間を更新する（何発当てても捕獲しない）
        stunTimer = stunDuration;

        if (rb != null)
            rb.velocity = Vector2.zero;

        CancelChargingAttack();
    }

    // チャージ中の攻撃泡を消し、膨らんだ体を元に戻す
    private void CancelChargingAttack()
    {
        CancelAim();

        // チャージ中の泡は敵の子オブジェクトになっている。
        // 発射済みの泡は親から外れているため削除しない。
        EnemyBubble[] chargingBubbles = GetComponentsInChildren<EnemyBubble>();
        foreach (EnemyBubble bubble in chargingBubbles)
        {
            if (bubble != null)
                Destroy(bubble.gameObject);
        }

        ResetInflateSmooth(0.15f);
        EndAttack();
        attackTimer = attackCooldown;
    }

    private void GetBubbled(
      float bubbleScale,
      float bubbleTime
  )
    {
        isBubbled = true;
        CancelAim();

        attackTimer = attackCooldown;
        bubbleTimer = bubbleTime;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);

            bubbleVisual.transform.localScale =
                Vector3.one * bubbleScale;
        }

        if (dashUI != null)
        {
            dashUI.SetActive(false);
        }

        Debug.Log(
            "敵が泡に包まれた！ 制限時間: " +
            bubbleTime +
            "秒"
        );
    }

    private void ReleaseBubble()
    {


        isBubbled = false;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }
        attackTimer = attackCooldown;
        if (dashUI != null)
        {
            dashUI.SetActive(false);
        }
        EnemyBubblePop pop =
    GetComponent<EnemyBubblePop>();

        if (pop != null)
        {
            pop.PlayPop();
        }
        currentHits = 0;

        Debug.Log("泡の時間切れ！敵が元に戻った！");
    }
    public bool IsBubbled
    {
        get { return isBubbled; }
    }

    // EnemyBubbleから通常弾の停止状態を参照する
    public bool IsStunned
    {
        get { return stunTimer > 0f; }
    }
    public void ShowDashUI(bool show)
    {
        if (!isBubbled)
        {
            return;
        }

        if (dashUI != null)
        {
            dashUI.SetActive(show);
        }
    }
    public void Defeat(bool isPerfect = false)
    {
        if (!isBubbled)
            return;

        Debug.Log("敵を撃破！");

        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
            stageManager.AddDefeatedEnemy("Pufferfish");

        WaterGun waterGun =
            FindObjectOfType<WaterGun>();

        if (waterGun != null)
        {
            int recoveryAmount = isPerfect
                ? waterRecoveryAmount * 2
                : waterRecoveryAmount;

            waterGun.RecoverWater(recoveryAmount);
        }

        Destroy(gameObject);
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void DashKnockback(Vector2 direction)
    {
        if (rb == null)
        {
            return;
        }

        direction = direction.normalized;

        rb.MovePosition(
            rb.position + direction * dashKnockbackSpeed * dashKnockbackTime
        );
    }

    public void SyncInflate(float amount)
    {
        amount = Mathf.Clamp01(amount);

        float scale = Mathf.Lerp(
            normalScale,
            maxInflateScale,
            amount
        );

        float directionX = Mathf.Sign(transform.localScale.x);

        transform.localScale = new Vector3(
            directionX * scale,
            scale,
            scale
        );
    }

    private Coroutine resetInflateCoroutine;

    public void ResetInflateSmooth(float duration)
    {
        if (resetInflateCoroutine != null)
            StopCoroutine(resetInflateCoroutine);

        resetInflateCoroutine = StartCoroutine(ResetInflateCoroutine(duration));
    }

    private System.Collections.IEnumerator ResetInflateCoroutine(float duration)
    {
        float startScale = Mathf.Abs(transform.localScale.x);
        float time = 0f;

        float directionX = Mathf.Sign(transform.localScale.x);

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

            float scale = Mathf.Lerp(
                startScale,
                normalScale,
                t
            );

            transform.localScale = new Vector3(
                directionX * scale,
                scale,
                scale
            );

            yield return null;
        }

        transform.localScale = new Vector3(
            directionX * normalScale,
            normalScale,
            normalScale
        );
    }
    // 敵の見た目が画面に入るまで攻撃タイマーを進めない
    private bool IsVisibleOnScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Bounds bounds = spriteRenderer != null
            ? spriteRenderer.bounds
            : new Bounds(transform.position, Vector3.zero);

        Vector3 min = cam.WorldToViewportPoint(bounds.min);
        Vector3 max = cam.WorldToViewportPoint(bounds.max);

        if (min.z <= 0f || max.z <= 0f)
            return false;

        // フグのSprite全体が画面内に収まってから攻撃を許可する
        return min.x >= 0f && max.x <= 1f &&
               min.y >= 0f && max.y <= 1f;
    }

    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null || attackMuzzle == null || player == null)
            return;

        isAttacking = true;
        // フグの口からプレイヤーへの方向
        Vector2 shootDirection =
            (player.position - attackMuzzle.position).normalized;

        // 泡の生成位置を口から少し前にずらす
        Vector3 bubblePosition =
            attackMuzzle.position +
            (Vector3)(shootDirection * attackBubbleOffsetX);

        GameObject bubbleObject = Instantiate(
            enemyBubblePrefab, bubblePosition, Quaternion.identity, attackMuzzle);

        EnemyBubble enemyBubble = bubbleObject.GetComponentInChildren<EnemyBubble>();
        if (enemyBubble == null)
        {
            Debug.LogError("EnemyBubble.csがPrefabに見つかりません！");
            Destroy(bubbleObject);
            isAttacking = false;
            return;
        }

        enemyBubble.SetOwner(this);
        enemyBubble.SetDirection(shootDirection);
        if (aimCoroutine != null) StopCoroutine(aimCoroutine);
        aimCoroutine = StartCoroutine(AimBeforeShot(enemyBubble));
    }

    private IEnumerator AimBeforeShot(EnemyBubble bubble)
    {
        float timer = 0f;
        Vector2 direction = Vector2.right;
        while (timer < aimWarningTime)
        {
            if (bubble == null || player == null || isBubbled ||
                isGameOverStopped || isSpawning)
            {
                if (aimLine != null) aimLine.Hide();
                aimCoroutine = null;
                yield break;
            }

            Vector2 start = attackMuzzle.position;
            Vector2 target = player.position;
            direction = (target - start).normalized;
            RotateEnemyToAim(direction);
            if (direction.sqrMagnitude < 0.001f)
                direction = transform.localScale.x > 0f ? Vector2.left : Vector2.right;

            bubble.SetDirection(direction);
            if (aimLine != null) aimLine.Show(start, target);
            timer += Time.deltaTime;
            yield return null;
        }

        // この時点の方向を固定。EnemyBubbleは最大チャージ時に自動発射する。
        if (aimLine != null) aimLine.Hide();
        aimCoroutine = null;
    }

    private void CancelAim()
    {
        if (aimCoroutine != null)
        {
            StopCoroutine(aimCoroutine);
            aimCoroutine = null;
        }
        if (aimLine != null) aimLine.Hide();
    }


    private void ChasePlayer()
    {
        if (player == null || rb == null)
        {
            return;
        }

        // プレイヤーとの横方向の距離
        float distanceX =
            player.position.x - transform.position.x;

        // プレイヤーが真上・真下付近にいる場合
        if (Mathf.Abs(distanceX) <= turnThreshold)
        {
            // 左右移動を停止
            rb.velocity = new Vector2(
                0f,
                rb.velocity.y
            );

            return;
        }

        // プレイヤーが左右に離れている場合だけ追跡
        float direction = Mathf.Sign(distanceX);

        rb.velocity = new Vector2(
            direction * chaseSpeed,
            rb.velocity.y
        );
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (enemyVisual != null)
        {
            enemyVisual.localRotation = Quaternion.identity;
        }
    }
    public void StopForGameOver()
    {
        if (isGameOverStopped)
        {
            return;
        }

        isGameOverStopped = true;
        CancelAim();

        // 移動を完全に停止
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 攻撃中も解除
        isAttacking = false;
    }
    private bool IsAboveScreen()
    {
        if (Camera.main == null || spriteRenderer == null)
        {
            return false;
        }

        float cameraTop =
            Camera.main.ViewportToWorldPoint(
                new Vector3(0.5f, 1f, 0f)
            ).y;

        float defeatLine =
            cameraTop - defeatHeightOffset;

        return spriteRenderer.bounds.center.y > defeatLine;
    }
    public void StartSpawnSequence()
    {
        isSpawning = true;
        CancelAim();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        isAttacking = false;
    }
    public void EndSpawnSequence()
    {
        isSpawning = false;

        // 出現直後に即攻撃しないようにする
        attackTimer = attackCooldown;
    }
    private void RotateEnemyToAim(Vector2 direction)
    {
        if (enemyVisual == null) return;

        // 左右反転を考慮して、上下の傾きを計算
        float angle = Mathf.Atan2(
            direction.y,
            Mathf.Abs(direction.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -maxAimAngle, maxAimAngle);

        // 左右反転に応じて回転方向を調整
        float facingSign = Mathf.Sign(transform.localScale.x);

        float targetAngle = -angle * facingSign;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, targetAngle);

        enemyVisual.localRotation = Quaternion.Lerp(
            enemyVisual.localRotation,
            targetRotation,
            aimRotateSpeed * Time.deltaTime
        );
    }
}