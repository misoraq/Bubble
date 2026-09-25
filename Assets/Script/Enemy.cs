using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("ñA")]
    [SerializeField] private GameObject bubbleVisual;

    [Header("ñAÇÃëÂÇ´Ç≥")]
    [SerializeField] private float smallBubbleScale = 0.2f;
    [SerializeField] private float fullChargeBubbleScale = 0.35f;

    [Header("ñAÇÃêßå¿éûä‘")]
    [SerializeField] private float smallBubbleTime = 3f;
    [SerializeField] private float fullChargeBubbleTime = 7f;

    [SerializeField] private float floatSpeed = 1.5f;

    [Header("è¨Ç≥Ç¢ñA")]
    [SerializeField] private int requiredHits = 3;
    [Header("É_ÉbÉVÉÖUI")]
    [SerializeField] private GameObject dashUI;
    private int currentHits = 0;
    private bool isBubbled = false;
    [Header("É_ÉbÉVÉÖè’ìÀ")]
    [SerializeField] private float dashKnockbackSpeed = 4f;
    [SerializeField] private float dashKnockbackTime = 0.2f;

    [Header("Playerí«ê’")]
    [SerializeField] private float chaseSpeed = 2f;

    [Header("ìGÇÃñcí£")]
    [SerializeField] private float normalScale = 0.3f;
    [SerializeField] private float maxInflateScale = 0.6f;

    private float originalScaleX;
    private float originalScaleY;
    private float originalScaleZ;

    [Header("ìGÇÃñAçUåÇ")]
    [SerializeField] private GameObject enemyBubblePrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackCooldown = 3f;

    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float bubbleTimer = 0f;
    private Transform player;
    private bool isAttacking = false;
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
        if (player != null && !isBubbled)
        {
            ChasePlayer();
        }
        if (player != null && !isAttacking)
        {
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
        if (player != null && !isBubbled)
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
            // ìGÇè„Ç…ïÇÇ©ÇπÇÈ
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // ñAÇÃéûä‘Çå∏ÇÁÇ∑
            bubbleTimer -= Time.deltaTime;

            // éûä‘êÿÇÍ
            if (bubbleTimer <= 0f)
            {
                ReleaseBubble();
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
        if (isBubbled)
        {
            return;
        }

        // ÉtÉãÉ`ÉÉÅ[ÉWÇ»ÇÁ1î≠Ç≈ïÔÇﬁ
        if (isFullCharge)
        {
            GetBubbled(
                fullChargeBubbleScale,
                fullChargeBubbleTime
            );

            return;
        }

        // è¨Ç≥Ç¢ñAÇÕï°êîâÒïKóv
        currentHits++;

        Debug.Log(
            "ìGÇ…ñAÇ™ìñÇΩÇ¡ÇΩ: " +
            currentHits +
            " / " +
            requiredHits
        );

        if (currentHits >= requiredHits)
        {
            GetBubbled(
                smallBubbleScale,
                smallBubbleTime
            );
        }
    }

    private void GetBubbled(
      float bubbleScale,
      float bubbleTime
  )
    {
        isBubbled = true;

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
            "ìGÇ™ñAÇ…ïÔÇ‹ÇÍÇΩÅI êßå¿éûä‘: " +
            bubbleTime +
            "ïb"
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

        currentHits = 0;

        Debug.Log("ñAÇÃéûä‘êÿÇÍÅIìGÇ™å≥Ç…ñﬂÇ¡ÇΩÅI");
    }
    public bool IsBubbled
    {
        get { return isBubbled; }
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
    public void Defeat()
    {
        if (!isBubbled)
        {
            return;
        }

        Debug.Log("ìGÇåÇîjÅI");

        ComboManager comboManager =
            FindObjectOfType<ComboManager>();

        if (comboManager != null)
        {
            comboManager.AddCombo();
        }

        ScoreManager scoreManager =
            FindObjectOfType<ScoreManager>();

        if (scoreManager != null)
        {
            int comboCount = 1;

            if (comboManager != null)
            {
                comboCount = comboManager.GetComboCount();
            }

            scoreManager.AddScore(comboCount);
        }

        Destroy(gameObject);
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
       
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

    public void ResetInflate()
    {
        float directionX = Mathf.Sign(transform.localScale.x);

        transform.localScale = new Vector3(
            directionX * normalScale,
            normalScale,
            normalScale
        );
    }
    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null || attackMuzzle == null)
            return;

        isAttacking = true;

        GameObject bubbleObject = Instantiate(
     enemyBubblePrefab,
     attackMuzzle.position,
     Quaternion.identity,
     attackMuzzle
 );

        EnemyBubble enemyBubble =
            bubbleObject.GetComponentInChildren<EnemyBubble>();

        if (enemyBubble == null)
        {
            Debug.LogError("EnemyBubble.csÇ™PrefabÇ…å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅI");
            return;
        }

        enemyBubble.SetOwner(this);

        if (player != null)
        {
            enemyBubble.SetDirection(
                Mathf.Sign(
                    player.position.x -
                    transform.position.x
                )
            );
        }
    }
    private void ChasePlayer()
    {
        float direction =
            Mathf.Sign(player.position.x - transform.position.x);

        rb.velocity = new Vector2(
            direction * chaseSpeed,
            rb.velocity.y
        );
    }


}