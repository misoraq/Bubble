
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("空中移動")]
    [SerializeField] private float airAcceleration = 10f;

    [Header("アニメーション")]
    [SerializeField] private Animator animator;

    [Header("ジェットパック")]
    [SerializeField] private GameObject jetFlame;
    [SerializeField] private float jetpackForce = 8f;
    [SerializeField] private float jetpackMaxSpeed = 5f;

    [Header("ダッシュ")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashRange = 3f;

    [Header("ダッシュ能力")]
    [SerializeField] private bool canUseBubbleDash = false;

    [Header("カニの突進による行動不能")]
    [SerializeField] private float crabStunDuration = 1.2f;
    [SerializeField] private float crabKnockbackSpeed = 4f;
    [SerializeField] private float crabKnockbackTime = 0.15f;

    [Header("画面端")]
    [SerializeField] private float screenMargin = 0.5f;

    private Rigidbody2D rb;

    private float crabStunTimer;
    private float crabKnockbackTimer;
    private Vector2 crabKnockbackDirection;

    public bool IsCrabStunned => crabStunTimer > 0f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float bounceTimer = 0f;

    private Vector2 dashDirection;

    private float normalGravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        normalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // フェーズ演出中は操作禁止
        if (PhaseAnnouncementUI.IsPlaying)
        {
            StopInputDuringPhase();
            return;
        }

        // カニの突進を受けた場合
        if (crabStunTimer > 0f)
        {
            crabStunTimer -= Time.deltaTime;

            if (crabKnockbackTimer > 0f)
                crabKnockbackTimer -= Time.deltaTime;

            if (jetFlame != null)
                jetFlame.SetActive(false);

            if (animator != null)
                animator.SetBool("IsWalking", false);

            LimitToScreen();
            UpdateDashUI();
            return;
        }

        Move();
        Jump();
        Dash();
        UpdateAnimation();
        LimitToScreen();
        UpdateDashUI();
    }

    private void FixedUpdate()
    {
        if (PhaseAnnouncementUI.IsPlaying)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (IsCrabStunned)
        {
            if (crabKnockbackTimer > 0f)
            {
                rb.velocity = new Vector2(
                    crabKnockbackDirection.x * crabKnockbackSpeed,
                    rb.velocity.y
                );
            }
            else
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            return;
        }

        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
    }

    private void StopInputDuringPhase()
    {
        if (isDashing)
            EndDash();

        bounceTimer = 0f;

        rb.velocity = new Vector2(
            0f,
            rb.velocity.y
        );

        if (jetFlame != null)
            jetFlame.SetActive(false);

        if (animator != null)
            animator.SetBool("IsWalking", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PhaseAnnouncementUI.IsPlaying)
            return;

        Debug.Log(
            "衝突した相手 : " + collision.gameObject.name
        );

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy == null)
            return;

        // ダッシュ中に敵へ接触
        if (isDashing && canUseBubbleDash)
        {
            if (enemy.IsBubbled)
            {
                enemy.Defeat();
            }
            else
            {
                Vector2 knockbackDirection =
                    (enemy.transform.position - transform.position).normalized;

                enemy.DashKnockback(knockbackDirection);
            }

            return;
        }

        // 通常時の敵との衝突
        Vector2 direction =
            (transform.position - enemy.transform.position).normalized;

        float bouncePower = 3f;

        // 横方向から接触
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            bounceTimer = 0.15f;

            rb.velocity = new Vector2(
                Mathf.Sign(direction.x) * 2f,
                rb.velocity.y
            );

            Rigidbody2D enemyRb =
                enemy.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                enemyRb.velocity = new Vector2(
                    -Mathf.Sign(direction.x) * 1.5f,
                    enemyRb.velocity.y
                );
            }
        }
        // 上下方向から接触
        else
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Sign(direction.y) * bouncePower
            );
        }
    }

    private void Move()
    {
        if (isDashing)
            return;

        if (bounceTimer > 0f)
        {
            bounceTimer -= Time.deltaTime;
            return;
        }

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        // 地上移動
        if (IsGrounded())
        {
            rb.velocity = new Vector2(
                horizontal * moveSpeed,
                rb.velocity.y
            );
        }
        // 空中移動
        else
        {
            float targetSpeed =
                horizontal * moveSpeed;

            float newSpeed = Mathf.MoveTowards(
                rb.velocity.x,
                targetSpeed,
                airAcceleration * Time.deltaTime
            );

            rb.velocity = new Vector2(
                newSpeed,
                rb.velocity.y
            );
        }

        // プレイヤーの向き
        if (horizontal < 0f)
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, 0f);
        }
        else if (horizontal > 0f)
        {
            transform.rotation =
                Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void Jump()
    {
        if (isDashing)
        {
            if (jetFlame != null)
                jetFlame.SetActive(false);

            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (rb.velocity.y < jetpackMaxSpeed)
            {
                rb.AddForce(
                    Vector2.up * jetpackForce
                );
            }

            if (jetFlame != null)
                jetFlame.SetActive(true);
        }
        else
        {
            if (jetFlame != null)
                jetFlame.SetActive(false);
        }
    }

    private void Dash()
    {
        // ダッシュ能力が無効の場合は使用不可
        if (!canUseBubbleDash)
        {
            if (isDashing)
                EndDash();

            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)
            && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
                EndDash();
        }
    }

    private void StartDash()
    {
        if (!canUseBubbleDash)
            return;

        Enemy targetEnemy =
            FindNearestBubbledEnemy();

        if (targetEnemy == null)
        {
            Debug.Log(
                "近くに泡状態の敵がいません"
            );
            return;
        }

        isDashing = true;
        dashTimer = dashDuration;

        Vector2 direction =
            targetEnemy.transform.position -
            transform.position;

        dashDirection = direction.normalized;

        // ダッシュ中は重力を無効化
        rb.gravityScale = 0f;

        rb.velocity =
            dashDirection * dashSpeed;

        Debug.Log(
            "ダッシュ開始！ ターゲット: " +
            targetEnemy.name
        );
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;

        rb.gravityScale =
            normalGravityScale;

        rb.velocity = new Vector2(
            0f,
            rb.velocity.y
        );
    }

    private Enemy FindNearestBubbledEnemy()
    {
        Enemy[] enemies =
            FindObjectsOfType<Enemy>();

        Enemy nearestEnemy = null;

        float nearestDistance =
            dashRange;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsBubbled)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private bool IsGrounded()
    {
        return Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.6f
        );
    }

    private void UpdateAnimation()
    {
        float horizontal =
            Input.GetAxisRaw("Horizontal");

        bool isWalking =
            horizontal != 0f && !isDashing;

        if (animator != null)
        {
            animator.SetBool(
                "IsWalking",
                isWalking
            );
        }
    }

    private void LimitToScreen()
    {
        if (Camera.main == null)
            return;

        Vector3 viewPos =
            Camera.main.WorldToViewportPoint(
                transform.position
            );

        // 左右の画面端を超えると反対側へ
        if (viewPos.x < 0f)
        {
            viewPos.x = 1f;
        }
        else if (viewPos.x > 1f)
        {
            viewPos.x = 0f;
        }

        // 画面上端より上へ出られないようにする
        if (viewPos.y > 1f)
        {
            viewPos.y = 1f;

            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    0f
                );
            }
        }

        transform.position =
            Camera.main.ViewportToWorldPoint(
                viewPos
            );
    }

    private void UpdateDashUI()
    {
        Enemy[] enemies =
            FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsBubbled)
                continue;

            // ダッシュ能力が無効なら案内UIも非表示
            if (!canUseBubbleDash)
            {
                enemy.ShowDashUI(false);
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            enemy.ShowDashUI(
                distance <= dashRange
            );
        }
    }

    public void HitByCrabCharge(Vector2 direction)
    {
        if (PhaseAnnouncementUI.IsPlaying)
            return;

        crabStunTimer =
            crabStunDuration;

        crabKnockbackTimer =
            crabKnockbackTime;

        crabKnockbackDirection =
            direction.x >= 0f
                ? Vector2.right
                : Vector2.left;

        if (isDashing)
            EndDash();

        bounceTimer = 0f;

        rb.velocity = new Vector2(
            crabKnockbackDirection.x *
            crabKnockbackSpeed,
            rb.velocity.y
        );

        WaterGun waterGun =
            GetComponentInChildren<WaterGun>();

        if (waterGun != null)
        {
            waterGun.CancelChargingByCrab();
        }
    }

    // ステージクリア・ゲームオーバー時の停止
    public void StopForStageEnd()
    {
        isDashing = false;
        dashTimer = 0f;
        bounceTimer = 0f;
        crabStunTimer = 0f;
        crabKnockbackTimer = 0f;

        if (jetFlame != null)
            jetFlame.SetActive(false);

        if (rb != null)
        {
            rb.gravityScale = normalGravityScale;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 現在のアニメーションのフレームで停止
        if (animator != null)
        {
            animator.speed = 0f;
        }

        enabled = false;
    }

    // アイテム取得時などにダッシュ能力を切り替える
    public void SetBubbleDashEnabled(bool enabled)
    {
        canUseBubbleDash = enabled;

        if (!canUseBubbleDash && isDashing)
            EndDash();

        UpdateDashUI();
    }
}
