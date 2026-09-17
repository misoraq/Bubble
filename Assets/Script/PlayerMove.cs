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

    [Header("画面端")]
    [SerializeField] private float screenMargin = 0.5f;

    private Rigidbody2D rb;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float bounceTimer = 0f;
    // ダッシュする方向
    private Vector2 dashDirection;

    private float normalGravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        normalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        Move();
        Jump();
        Dash();
        UpdateAnimation();
        LimitToScreen();
        UpdateDashUI();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("衝突した相手 : " + collision.gameObject.name);
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            return;
        }

        // ダッシュ中
        if (isDashing)
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

        // EnemyからPlayerへ向かう方向
        Vector2 direction =
            (transform.position - enemy.transform.position).normalized;

        float bouncePower = 3f;

        // 横から当たった場合
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            bounceTimer = 0.15f;

            // Playerを反発
            rb.velocity = new Vector2(
                Mathf.Sign(direction.x) * 2f,
                rb.velocity.y
            );

            // Enemyも少し反発
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                enemyRb.velocity = new Vector2(
                    -Mathf.Sign(direction.x) * 1.5f,
                    enemyRb.velocity.y
                );
            }
        }
        // 上下から当たった場合
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
        // ダッシュ中は通常移動しない
        if (isDashing)
        {
            return;
        }
        if (bounceTimer > 0f)
        {
            bounceTimer -= Time.deltaTime;
            return;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");

        // 地上
        if (IsGrounded())
        {
            rb.velocity = new Vector2(
                horizontal * moveSpeed,
                rb.velocity.y
            );
        }
        // 空中
        else
        {
            float targetSpeed = horizontal * moveSpeed;

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

        // 向き
        if (horizontal < 0)
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, 0f);
        }
        else if (horizontal > 0)
        {
            transform.rotation =
                Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void Jump()
    {
        // ダッシュ中はジェットパックを使わない
        if (isDashing)
        {
            if (jetFlame != null)
            {
                jetFlame.SetActive(false);
            }

            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (rb.velocity.y < jetpackMaxSpeed)
            {
                rb.AddForce(Vector2.up * jetpackForce);
            }

            if (jetFlame != null)
            {
                jetFlame.SetActive(true);
            }
        }
        else
        {
            if (jetFlame != null)
            {
                jetFlame.SetActive(false);
            }
        }
    }

    private void Dash()
    {
        // Shiftを押したらダッシュ開始
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    private void StartDash()
    {
        // 近くの泡状態の敵を探す
        Enemy targetEnemy = FindNearestBubbledEnemy();

        // 泡状態の敵がいなければダッシュしない
        if (targetEnemy == null)
        {
            Debug.Log("近くに泡状態の敵がいません");
            return;
        }

        // ダッシュ開始
        isDashing = true;

        dashTimer = dashDuration;

        // 敵の方向を計算
        Vector2 direction =
            targetEnemy.transform.position - transform.position;

        dashDirection = direction.normalized;

        // 重力OFF
        rb.gravityScale = 0f;

        // 敵の方向へダッシュ
        rb.velocity = dashDirection * dashSpeed;

        Debug.Log(
            "ダッシュ開始！ ターゲット: " +
            targetEnemy.name
        );
    }

    private void EndDash()
    {
        isDashing = false;

        // 重力を元に戻す
        rb.gravityScale = normalGravityScale;

        // 横方向の速度を止める
        rb.velocity = new Vector2(
            0f,
            rb.velocity.y
        );
    }

    private Enemy FindNearestBubbledEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy nearestEnemy = null;

        float nearestDistance = dashRange;

        foreach (Enemy enemy in enemies)
        {
            // 泡状態じゃない敵は対象外
            if (!enemy.IsBubbled)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );

            // ダッシュ範囲内で一番近い敵
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
        float horizontal = Input.GetAxisRaw("Horizontal");

        bool isWalking =
            horizontal != 0 && !isDashing;

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
        Vector3 viewPos =
            Camera.main.WorldToViewportPoint(
                transform.position
            );

        // 左右：画面外に出たら反対側へ
        if (viewPos.x < 0f)
        {
            viewPos.x = 1f;
        }
        else if (viewPos.x > 1f)
        {
            viewPos.x = 0f;
        }

        // 上：画面外に出ない
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
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsBubbled)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <= dashRange)
            {
                enemy.ShowDashUI(true);
            }
            else
            {
                enemy.ShowDashUI(false);
            }
        }
    }
}