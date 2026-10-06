using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("–A")]
    [SerializeField] private GameObject bubbleVisual;

    [Header("–A‚Ì‘å‚«‚³")]
    [SerializeField] private float smallBubbleScale = 0.2f;
    [SerializeField] private float fullChargeBubbleScale = 0.35f;

    [Header("–A‚Ì§ŒÀŠÔ")]
    [SerializeField] private float smallBubbleTime = 3f;
    [SerializeField] private float fullChargeBubbleTime = 7f;

    [SerializeField] private float floatSpeed = 1.5f;

    [Header("¬‚³‚¢–A")]
    [SerializeField] private int requiredHits = 3;
    [Header("ƒ_ƒbƒVƒ…UI")]
    [SerializeField] private GameObject dashUI;
    private int currentHits = 0;
    private bool isBubbled = false;
    [Header("ƒ_ƒbƒVƒ…Õ“Ë")]
    [SerializeField] private float dashKnockbackSpeed = 4f;
    [SerializeField] private float dashKnockbackTime = 0.2f;

    [Header("Player’ÇÕ")]
    [SerializeField] private float chaseSpeed = 2f;

    [Header("“G‚Ì–c’£")]
    [SerializeField] private float normalScale = 0.3f;
    [SerializeField] private float maxInflateScale = 0.6f;

    private float originalScaleX;
    private float originalScaleY;
    private float originalScaleZ;

    [Header("Œü‚«§Œä")]
    [SerializeField] private float turnThreshold = 0.3f;
    [SerializeField] private float turnCooldown = 0.2f;

    private float turnTimer = 0f;

    [Header("“G‚Ì–AUŒ‚")]
    [SerializeField] private GameObject enemyBubblePrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackCooldown = 3f;

    [Header("“GUŒ‚–A‚ÌˆÊ’u")]
    [SerializeField] private float attackBubbleOffsetX = 0.15f;


    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float bubbleTimer = 0f;
    private Transform player;
    private bool isAttacking = false;
    private bool isGameOverStopped = false;
    private bool isSpawning = false;
    [Header("‰æ–ÊãŒ‚”j”»’è")]
    [SerializeField] private float defeatHeightOffset = 0.5f;
    private SpriteRenderer spriteRenderer;
    [Header("Œ‚”j‚Ì…‰ñ•œ")]
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

        if (player != null && !isBubbled && !isAttacking)
        {
            ChasePlayer();
        }
        // =========================
        // “G‚ÌŒü‚«
        // =========================
        if (player != null)
        {
            turnTimer -= Time.deltaTime;

            float distanceX =
                player.position.x - transform.position.x;

            // ˆê’èˆÈãX•ûŒü‚É—£‚ê‚Ä‚¢‚éê‡‚¾‚¯Œü‚«‚ğ•ÏX
            if (Mathf.Abs(distanceX) > turnThreshold)
            {
                // ¶‰E‚ğ”»’è
                bool playerIsLeft = distanceX < 0f;

                bool enemyIsFacingLeft =
                    transform.localScale.x > 0f;

                // Œü‚«‚ğ•Ï‚¦‚é•K—v‚ª‚ ‚é‚©
                bool needTurn =
                    playerIsLeft != enemyIsFacingLeft;

                // ­‚µŠÔ‚ğ‹ó‚¯‚Ä‚©‚ç”½“]
                if (needTurn && turnTimer <= 0f)
                {
                    if (playerIsLeft)
                    {
                        // ¶
                        transform.localScale = new Vector3(
                            Mathf.Abs(transform.localScale.x),
                            transform.localScale.y,
                            transform.localScale.z
                        );
                    }
                    else
                    {
                        // ‰E
                        transform.localScale = new Vector3(
                            -Mathf.Abs(transform.localScale.x),
                            transform.localScale.y,
                            transform.localScale.z
                        );
                    }

                    turnTimer = turnCooldown;
                }
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
            // ã‚Ö•‚‚©‚Ô
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // Š®‘S‚É‰æ–ÊŠO‚Öo‚½‚çŒ‚”j
            if (IsAboveScreen())
            {
                Defeat();
                return;
            }

            // –A‚Ìc‚èŠÔ‚ğŒ¸‚ç‚·
            bubbleTimer -= Time.deltaTime;

            // ŠÔØ‚ê‚È‚ç“G‚ğ‰ğ•ú
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

        // ƒtƒ‹ƒ`ƒƒ[ƒW‚È‚ç1”­‚Å•ï‚Ş
        if (isFullCharge)
        {
            GetBubbled(
                fullChargeBubbleScale,
                fullChargeBubbleTime
            );

            return;
        }

        // ¬‚³‚¢–A‚Í•¡”‰ñ•K—v
        currentHits++;

        Debug.Log(
            "“G‚É–A‚ª“–‚½‚Á‚½: " +
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
            "“G‚ª–A‚É•ï‚Ü‚ê‚½I §ŒÀŠÔ: " +
            bubbleTime +
            "•b"
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

        Debug.Log("–A‚ÌŠÔØ‚êI“G‚ªŒ³‚É–ß‚Á‚½I");
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

        Debug.Log("“G‚ğŒ‚”jI");

        // Œ‚”j”‚ğ‘‚â‚·
        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            stageManager.AddDefeatedEnemy();
        }

        // …‚ğ‰ñ•œ
        WaterGun waterGun =
            FindObjectOfType<WaterGun>();

        if (waterGun != null)
        {
            waterGun.RecoverWater(
                waterRecoveryAmount
            );
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

    public void ResetInflateSmooth(float duration)
    {
        StartCoroutine(ResetInflateCoroutine(duration));
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
    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null || attackMuzzle == null)
            return;

        isAttacking = true;

        float directionX = Mathf.Sign(transform.localScale.x);

        Vector3 bubblePosition =
            attackMuzzle.position +
            new Vector3(directionX * attackBubbleOffsetX, 0f, 0f);

        GameObject bubbleObject = Instantiate(
            enemyBubblePrefab,
            bubblePosition,
            Quaternion.identity,
            attackMuzzle
        );

        EnemyBubble enemyBubble =
            bubbleObject.GetComponentInChildren<EnemyBubble>();

        if (enemyBubble == null)
        {
            Debug.LogError("EnemyBubble.cs‚ªPrefab‚ÉŒ©‚Â‚©‚è‚Ü‚¹‚ñI");
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
    public void EndAttack()
    {
        isAttacking = false;
    }
    public void StopForGameOver()
    {
        if (isGameOverStopped)
        {
            return;
        }

        isGameOverStopped = true;

        // ˆÚ“®‚ğŠ®‘S‚É’â~
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // UŒ‚’†‚à‰ğœ
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

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        isAttacking = false;
    }
    public void EndSpawnSequence()
    {
        isSpawning = false;

        // oŒ»’¼Œã‚É‘¦UŒ‚‚µ‚È‚¢‚æ‚¤‚É‚·‚é
        attackTimer = attackCooldown;
    }
}