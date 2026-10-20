using UnityEngine;
using System.Collections;

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

    [Header("í èÌíeÇ…ÇÊÇÈí‚é~")]
    [SerializeField] private float stunDuration = 2f;
    private float stunTimer = 0f;
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

    [Header("å¸Ç´êßå‰")]
    [SerializeField] private float turnThreshold = 0.8f;
    [SerializeField] private float turnCooldown = 0.2f;

    private float turnTimer = 0f;

    [Header("ìGÇÃñAçUåÇ")]
    [SerializeField] private GameObject enemyBubblePrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackCooldown = 3f;

    [Header("ìGçUåÇñAÇÃà íu")]
    [SerializeField] private float attackBubbleOffsetX = 0.15f;

    [Header("çUåÇéûÇÃìGÇÃå¸Ç´")]
    [SerializeField] private Transform enemyVisual;
    [SerializeField] private float maxAimAngle = 45f;
    [SerializeField] private float aimRotateSpeed = 10f;

    [Header("çUåÇó\çê")]
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
    [Header("âÊñ è„åÇîjîªíË")]
    [SerializeField] private float defeatHeightOffset = 0.5f;
    private SpriteRenderer spriteRenderer;
    [Header("åÇîjéûÇÃêÖâÒïú")]
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

        // í èÌíeÇéÛÇØÇƒÇ¢ÇÈä‘ÇÕà⁄ìÆÇ∆çUåÇÇí‚é~Ç∑ÇÈ
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
        // ìGÇÃå¸Ç´
        // =========================

        if (player != null && !isBubbled)
        {
            turnTimer -= Time.deltaTime;

            // ÉvÉåÉCÉÑÅ[Ç∆ÇÃâ°ï˚å¸ÇÃãóó£
            float distanceX =
                player.position.x - transform.position.x;

            // ê^è„ÅEê^â∫ïtãﬂÇ≈ÇÕç≈å„ÇÃå¸Ç´Çà€éùÇ∑ÇÈ
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
            // è„Ç÷ïÇÇ©Ç‘
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // äÆëSÇ…âÊñ äOÇ÷èoÇΩÇÁåÇîj
            if (IsAboveScreen())
            {
                Defeat();
                return;
            }

            // ñAÇÃécÇËéûä‘Çå∏ÇÁÇ∑
            bubbleTimer -= Time.deltaTime;

            // éûä‘êÿÇÍÇ»ÇÁìGÇâï˙
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

        // í èÌíeÅFí‚é~éûä‘ÇçXêVÇ∑ÇÈÅiâΩî≠ìñÇƒÇƒÇ‡ïﬂälÇµÇ»Ç¢Åj
        stunTimer = stunDuration;

        if (rb != null)
            rb.velocity = Vector2.zero;

        CancelChargingAttack();
    }

    // É`ÉÉÅ[ÉWíÜÇÃçUåÇñAÇè¡ÇµÅAñcÇÁÇÒÇæëÃÇå≥Ç…ñﬂÇ∑
    private void CancelChargingAttack()
    {
        CancelAim();

        // É`ÉÉÅ[ÉWíÜÇÃñAÇÕìGÇÃéqÉIÉuÉWÉFÉNÉgÇ…Ç»Ç¡ÇƒÇ¢ÇÈÅB
        // î≠éÀçœÇ›ÇÃñAÇÕêeÇ©ÇÁäOÇÍÇƒÇ¢ÇÈÇΩÇﬂçÌèúÇµÇ»Ç¢ÅB
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
        EnemyBubblePop pop =
    GetComponent<EnemyBubblePop>();

        if (pop != null)
        {
            pop.PlayPop();
        }
        currentHits = 0;

        Debug.Log("ñAÇÃéûä‘êÿÇÍÅIìGÇ™å≥Ç…ñﬂÇ¡ÇΩÅI");
    }
    public bool IsBubbled
    {
        get { return isBubbled; }
    }

    // EnemyBubbleÇ©ÇÁí èÌíeÇÃí‚é~èÛë‘ÇéQè∆Ç∑ÇÈ
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

        Debug.Log("ìGÇåÇîjÅI");

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
    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null || attackMuzzle == null || player == null)
            return;

        isAttacking = true;
        // ÉtÉOÇÃå˚Ç©ÇÁÉvÉåÉCÉÑÅ[Ç÷ÇÃï˚å¸
        Vector2 shootDirection =
            (player.position - attackMuzzle.position).normalized;

        // ñAÇÃê∂ê¨à íuÇå˚Ç©ÇÁè≠ÇµëOÇ…Ç∏ÇÁÇ∑
        Vector3 bubblePosition =
            attackMuzzle.position +
            (Vector3)(shootDirection * attackBubbleOffsetX);

        GameObject bubbleObject = Instantiate(
            enemyBubblePrefab, bubblePosition, Quaternion.identity, attackMuzzle);

        EnemyBubble enemyBubble = bubbleObject.GetComponentInChildren<EnemyBubble>();
        if (enemyBubble == null)
        {
            Debug.LogError("EnemyBubble.csÇ™PrefabÇ…å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅI");
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

        // Ç±ÇÃéûì_ÇÃï˚å¸Çå≈íËÅBEnemyBubbleÇÕç≈ëÂÉ`ÉÉÅ[ÉWéûÇ…é©ìÆî≠éÀÇ∑ÇÈÅB
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

        // ÉvÉåÉCÉÑÅ[Ç∆ÇÃâ°ï˚å¸ÇÃãóó£
        float distanceX =
            player.position.x - transform.position.x;

        // ÉvÉåÉCÉÑÅ[Ç™ê^è„ÅEê^â∫ïtãﬂÇ…Ç¢ÇÈèÍçá
        if (Mathf.Abs(distanceX) <= turnThreshold)
        {
            // ç∂âEà⁄ìÆÇí‚é~
            rb.velocity = new Vector2(
                0f,
                rb.velocity.y
            );

            return;
        }

        // ÉvÉåÉCÉÑÅ[Ç™ç∂âEÇ…ó£ÇÍÇƒÇ¢ÇÈèÍçáÇæÇØí«ê’
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

        // à⁄ìÆÇäÆëSÇ…í‚é~
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // çUåÇíÜÇ‡âèú
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

        // èoåªíºå„Ç…ë¶çUåÇÇµÇ»Ç¢ÇÊÇ§Ç…Ç∑ÇÈ
        attackTimer = attackCooldown;
    }
    private void RotateEnemyToAim(Vector2 direction)
    {
        if (enemyVisual == null) return;

        // ç∂âEîΩì]Ççló∂ÇµÇƒÅAè„â∫ÇÃåXÇ´ÇåvéZ
        float angle = Mathf.Atan2(
            direction.y,
            Mathf.Abs(direction.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -maxAimAngle, maxAimAngle);

        // ç∂âEîΩì]Ç…âûÇ∂ÇƒâÒì]ï˚å¸Çí≤êÆ
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