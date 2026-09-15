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
    [Header("画面端制限")]
    [SerializeField] private float screenMargin = 0.5f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Move();
        Jump();
        UpdateAnimation();
        LimitToScreen();
    }

    private void Move()
    {
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

        // 左右で向きを変更
        if (horizontal < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (horizontal > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void Jump()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // 上昇速度に上限をつける
            if (rb.velocity.y < jetpackMaxSpeed)
            {
                rb.AddForce(Vector2.up * jetpackForce);
            }

            // 炎ON
            jetFlame.SetActive(true);
        }
        else
        {
            // Spaceを離したら炎OFF
            jetFlame.SetActive(false);
        }
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

        bool isWalking = horizontal != 0;

        animator.SetBool("IsWalking", isWalking);
    }
    private void LimitToScreen()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

        // =========================
        // 左右：反対側から出てくる
        // =========================

        if (viewPos.x < 0f)
        {
            viewPos.x = 1f;
        }
        else if (viewPos.x > 1f)
        {
            viewPos.x = 0f;
        }

        // =========================
        // 上：画面外に出ない
        // =========================

        if (viewPos.y > 1f)
        {
            viewPos.y = 1f;

            // 上方向の速度を止める
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    0f
                );
            }
        }

        transform.position = Camera.main.ViewportToWorldPoint(viewPos);
    }
}