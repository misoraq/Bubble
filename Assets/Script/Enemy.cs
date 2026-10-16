using UnityEngine;
using System.Collections;

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

    [Header("’Êí’e‚É‚æ‚é’â~")]
    [SerializeField] private float stunDuration = 2f;
    private float stunTimer = 0f;
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
    [SerializeField] private float turnThreshold = 0.8f;
    [SerializeField] private float turnCooldown = 0.2f;

    private float turnTimer = 0f;

    [Header("“G‚Ì–AUŒ‚")]
    [SerializeField] private GameObject enemyBubblePrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackCooldown = 3f;

    [Header("“GUŒ‚–A‚ÌˆÊ’u")]
    [SerializeField] private float attackBubbleOffsetX = 0.15f;

    [Header("UŒ‚‚Ì“G‚ÌŒü‚«")]
    [SerializeField] private Transform enemyVisual;
    [SerializeField] private float maxAimAngle = 45f;
    [SerializeField] private float aimRotateSpeed = 10f;

    [Header("UŒ‚—\")]
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

        // ’Êí’e‚ğó‚¯‚Ä‚¢‚éŠÔ‚ÍˆÚ“®‚ÆUŒ‚‚ğ’â~‚·‚é
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
        // “G‚ÌŒü‚«
        // =========================

        if (player != null && !isBubbled)
        {
            turnTimer -= Time.deltaTime;

            // ƒvƒŒƒCƒ„[‚Æ‚Ì‰¡•ûŒü‚Ì‹——£
            float distanceX =
                player.position.x - transform.position.x;

            // ^ãE^‰º•t‹ß‚Å‚ÍÅŒã‚ÌŒü‚«‚ğˆÛ‚·‚é
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
        if (isBubbled || isGameOverStopped || isSpawning)
            return;

        if (isFullCharge)
        {
            stunTimer = 0f;
            CancelChargingAttack();
            GetBubbled(fullChargeBubbleScale, fullChargeBubbleTime);
            return;
        }

        // ’Êí’eF’â~ŠÔ‚ğXV‚·‚éi‰½”­“–‚Ä‚Ä‚à•ßŠl‚µ‚È‚¢j
        stunTimer = stunDuration;

        if (rb != null)
            rb.velocity = Vector2.zero;

        CancelChargingAttack();
    }

    // ƒ`ƒƒ[ƒW’†‚ÌUŒ‚–A‚ğÁ‚µA–c‚ç‚ñ‚¾‘Ì‚ğŒ³‚É–ß‚·
    private void CancelChargingAttack()
    {
        CancelAim();

        // ƒ`ƒƒ[ƒW’†‚Ì–A‚Í“G‚ÌqƒIƒuƒWƒFƒNƒg‚É‚È‚Á‚Ä‚¢‚éB
        // ”­ËÏ‚İ‚Ì–A‚Íe‚©‚çŠO‚ê‚Ä‚¢‚é‚½‚ßíœ‚µ‚È‚¢B
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
        EnemyBubblePop pop =
    GetComponent<EnemyBubblePop>();

        if (pop != null)
        {
            pop.PlayPop();
        }
        currentHits = 0;

        Debug.Log("–A‚ÌŠÔØ‚êI“G‚ªŒ³‚É–ß‚Á‚½I");
    }
    public bool IsBubbled
    {
        get { return isBubbled; }
    }

    // EnemyBubble‚©‚ç’Êí’e‚Ì’â~ó‘Ô‚ğQÆ‚·‚é
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
    public void Defeat()
    {
        if (!isBubbled)
        {
            return;
        }

        Debug.Log("“G‚ğŒ‚”jI");
        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
        {
            pop.PlayPop();
        }
        // Œ‚”j”‚ğ‘‚â‚·
        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            stageManager.AddDefeatedEnemy("Pufferfish");
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
    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null || attackMuzzle == null || player == null)
            return;

        isAttacking = true;
        float directionX = Mathf.Sign(transform.localScale.x);
        Vector3 bubblePosition = attackMuzzle.position +
            new Vector3(directionX * attackBubbleOffsetX, 0f, 0f);

        GameObject bubbleObject = Instantiate(
            enemyBubblePrefab, bubblePosition, Quaternion.identity, attackMuzzle);

        EnemyBubble enemyBubble = bubbleObject.GetComponentInChildren<EnemyBubble>();
        if (enemyBubble == null)
        {
            Debug.LogError("EnemyBubble.cs‚ªPrefab‚ÉŒ©‚Â‚©‚è‚Ü‚¹‚ñI");
            Destroy(bubbleObject);
            isAttacking = false;
            return;
        }

        enemyBubble.SetOwner(this);
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

        // ‚±‚Ì“_‚Ì•ûŒü‚ğŒÅ’èBEnemyBubble‚ÍÅ‘åƒ`ƒƒ[ƒW‚É©“®”­Ë‚·‚éB
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

        // ƒvƒŒƒCƒ„[‚Æ‚Ì‰¡•ûŒü‚Ì‹——£
        float distanceX =
            player.position.x - transform.position.x;

        // ƒvƒŒƒCƒ„[‚ª^ãE^‰º•t‹ß‚É‚¢‚éê‡
        if (Mathf.Abs(distanceX) <= turnThreshold)
        {
            // ¶‰EˆÚ“®‚ğ’â~
            rb.velocity = new Vector2(
                0f,
                rb.velocity.y
            );

            return;
        }

        // ƒvƒŒƒCƒ„[‚ª¶‰E‚É—£‚ê‚Ä‚¢‚éê‡‚¾‚¯’ÇÕ
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

        // oŒ»’¼Œã‚É‘¦UŒ‚‚µ‚È‚¢‚æ‚¤‚É‚·‚é
        attackTimer = attackCooldown;
    }
    private void RotateEnemyToAim(Vector2 direction)
    {
        if (enemyVisual == null) return;

        // ¶‰E”½“]‚ğl—¶‚µ‚ÄAã‰º‚ÌŒX‚«‚ğŒvZ
        float angle = Mathf.Atan2(
            direction.y,
            Mathf.Abs(direction.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -maxAimAngle, maxAimAngle);

        // ¶‰E”½“]‚É‰‚¶‚Ä‰ñ“]•ûŒü‚ğ’²®
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