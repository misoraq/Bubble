
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

    public enum MovementPattern
    {
        NormalChase,
        Wave,
        Circle,
        Random
    }

    [Header("ÉtÉOÇÃà⁄ìÆÉpÉ^Å[Éì")]
    [SerializeField]
    private MovementPattern movementPattern =
        MovementPattern.Random;

    [SerializeField] private float waveAmplitude = 1.0f;
    [SerializeField] private float waveFrequency = 0.8f;

    [SerializeField] private float circleRadiusX = 1.5f;
    [SerializeField] private float circleRadiusY = 0.9f;
    [SerializeField] private float circleSpeed = 1.5f;
    [SerializeField] private float circleCenterFollowSpeed = 0.5f;
    [SerializeField] private float maxCircleMoveSpeed = 4f;

    [SerializeField] private float screenMargin = 0.15f;
    [SerializeField] private float minimumMoveHeight = 1.0f;

    private MovementPattern activeMovementPattern;
    private bool specialMovementStarted;
    private float movementTime;
    private float movementBaseY;
    private Vector2 circleCenter;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        SelectMovementPattern();
    }

    private void Update()
    {
        if (isGameOverStopped || isSpawning)
            return;

        // í èÌíeÇ…ÇÊÇÈÉXÉ^Éì
        if (stunTimer > 0f && !isBubbled)
        {
            stunTimer -= Time.deltaTime;

            if (rb != null)
                rb.velocity = Vector2.zero;

            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                specialMovementStarted = false;
                attackTimer = attackCooldown;
            }

            return;
        }

        // ÉvÉåÉCÉÑÅ[ÇÃï˚å¸Çå¸Ç≠
        if (player != null && !isBubbled)
        {
            turnTimer -= Time.deltaTime;

            float distanceX =
                player.position.x - transform.position.x;

            if (Mathf.Abs(distanceX) > turnThreshold)
            {
                bool playerIsLeft = distanceX < 0f;
                bool enemyIsFacingLeft =
                    transform.localScale.x > 0f;

                bool needTurn =
                    playerIsLeft != enemyIsFacingLeft;

                if (needTurn && turnTimer <= 0f)
                {
                    float scaleX =
                        Mathf.Abs(transform.localScale.x);

                    transform.localScale = new Vector3(
                        playerIsLeft ? scaleX : -scaleX,
                        transform.localScale.y,
                        transform.localScale.z
                    );

                    turnTimer = turnCooldown;
                }
            }
        }

        // ÉtÉOëSëÃÇ™âÊñ ì‡Ç…ì¸Ç¡ÇƒÇ©ÇÁçUåÇ
        if (player != null &&
            !isBubbled &&
            IsVisibleOnScreen())
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                ShootEnemyBubble();
                attackTimer = attackCooldown;
            }
        }

        // ñAÇ…ïﬂÇ‹Ç¡ÇƒÇ¢ÇÈä‘
        if (isBubbled)
        {
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            if (IsAboveScreen())
            {
                Defeat();
                return;
            }

            bubbleTimer -= Time.deltaTime;

            if (bubbleTimer <= 0f)
            {
                BubbleFinish bubbleFinish =
                    GetComponent<BubbleFinish>();

                if (bubbleFinish == null ||
                    !bubbleFinish.IsFinishing)
                {
                    ReleaseBubble();
                }
            }
        }

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f && rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    // Rigidbody2DÇÃà⁄ìÆÇÕï®óùçXêVÇ≈çsÇ§
    private void FixedUpdate()
    {
        if (isGameOverStopped ||
            isSpawning ||
            isBubbled ||
            isAttacking ||
            stunTimer > 0f)
        {
            return;
        }

        if (player != null)
        {
            UpdateMovementPattern();
        }
    }

    public void HitByBubble(bool isFullCharge)
    {
        EnemySpawnProtection protection =
            GetComponent<EnemySpawnProtection>();

        if (protection != null && protection.IsProtected)
            return;

        if (isBubbled || isGameOverStopped || isSpawning)
            return;

        if (isFullCharge)
        {
            stunTimer = 0f;
            CancelChargingAttack();

            GetBubbled(
                fullChargeBubbleScale,
                fullChargeBubbleTime
            );

            return;
        }

        stunTimer = stunDuration;

        if (rb != null)
            rb.velocity = Vector2.zero;

        CancelChargingAttack();
    }

    private void CancelChargingAttack()
    {
        CancelAim();

        EnemyBubble[] chargingBubbles =
            GetComponentsInChildren<EnemyBubble>();

        foreach (EnemyBubble bubble in chargingBubbles)
        {
            if (bubble != null)
                Destroy(bubble.gameObject);
        }

        ResetInflateSmooth(0.15f);
        EndAttack();
        attackTimer = attackCooldown;
    }

    private void GetBubbled(float bubbleScale, float bubbleTime)
    {
        isBubbled = true;
        GameSoundManager.Instance?.PlayEnemyTrap();
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
            dashUI.SetActive(false);

        Debug.Log(
            "ìGÇ™ñAÇ…ïÔÇ‹ÇÍÇΩÅI êßå¿éûä‘: " +
            bubbleTime + "ïb"
        );
    }

    private void ReleaseBubble()
    {
        isBubbled = false;
        specialMovementStarted = false;

        if (bubbleVisual != null)
            bubbleVisual.SetActive(false);

        attackTimer = attackCooldown;

        if (dashUI != null)
            dashUI.SetActive(false);

        EnemyBubblePop pop =
            GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        currentHits = 0;

        Debug.Log("ñAÇÃéûä‘êÿÇÍÅIìGÇ™å≥Ç…ñﬂÇ¡ÇΩÅI");
    }

    public bool IsBubbled
    {
        get { return isBubbled; }
    }

    public bool IsStunned
    {
        get { return stunTimer > 0f; }
    }

    public void ShowDashUI(bool show)
    {
        if (!isBubbled)
            return;

        if (dashUI != null)
            dashUI.SetActive(show);
    }

    public void Defeat(bool isPerfect = false)
    {
        if (!isBubbled)
            return;

        Debug.Log("ìGÇåÇîjÅI");

        EnemyBubblePop pop =
            GetComponent<EnemyBubblePop>();

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

    public void DashKnockback(Vector2 direction)
    {
        if (rb == null)
            return;

        direction = direction.normalized;

        rb.MovePosition(
            rb.position +
            direction * dashKnockbackSpeed * dashKnockbackTime
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

        float directionX =
            Mathf.Sign(transform.localScale.x);

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

        resetInflateCoroutine =
            StartCoroutine(ResetInflateCoroutine(duration));
    }

    private IEnumerator ResetInflateCoroutine(float duration)
    {
        float startScale =
            Mathf.Abs(transform.localScale.x);

        float time = 0f;
        float directionX =
            Mathf.Sign(transform.localScale.x);

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

    // ÉtÉOëSëÃÇ™âÊñ ì‡Ç…Ç†ÇÈÇ©
    private bool IsVisibleOnScreen()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return false;

        Bounds bounds = spriteRenderer != null
            ? spriteRenderer.bounds
            : new Bounds(transform.position, Vector3.zero);

        Vector3 min =
            cam.WorldToViewportPoint(bounds.min);

        Vector3 max =
            cam.WorldToViewportPoint(bounds.max);

        if (min.z <= 0f || max.z <= 0f)
            return false;

        return min.x >= 0f &&
               max.x <= 1f &&
               min.y >= 0f &&
               max.y <= 1f;
    }

    private void ShootEnemyBubble()
    {
        if (enemyBubblePrefab == null ||
            attackMuzzle == null ||
            player == null)
        {
            return;
        }

        isAttacking = true;

        Vector2 shootDirection =
            (player.position - attackMuzzle.position).normalized;

        Vector3 bubblePosition =
            attackMuzzle.position +
            (Vector3)(shootDirection * attackBubbleOffsetX);

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
            Debug.LogError(
                "EnemyBubble.csÇ™PrefabÇ…å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅI"
            );

            Destroy(bubbleObject);
            isAttacking = false;
            return;
        }

        enemyBubble.SetOwner(this);
        enemyBubble.SetDirection(shootDirection);

        if (aimCoroutine != null)
            StopCoroutine(aimCoroutine);

        aimCoroutine =
            StartCoroutine(AimBeforeShot(enemyBubble));
    }

    private IEnumerator AimBeforeShot(EnemyBubble bubble)
    {
        float timer = 0f;
        Vector2 direction = Vector2.right;

        while (timer < aimWarningTime)
        {
            if (bubble == null ||
                player == null ||
                isBubbled ||
                isGameOverStopped ||
                isSpawning)
            {
                if (aimLine != null)
                    aimLine.Hide();

                aimCoroutine = null;
                yield break;
            }

            Vector2 start = attackMuzzle.position;
            Vector2 target = player.position;

            direction = (target - start).normalized;

            RotateEnemyToAim(direction);

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.localScale.x > 0f
                    ? Vector2.left
                    : Vector2.right;
            }

            bubble.SetDirection(direction);

            if (aimLine != null)
                aimLine.Show(start, target);

            timer += Time.deltaTime;
            yield return null;
        }

        if (aimLine != null)
            aimLine.Hide();

        aimCoroutine = null;
    }

    private void CancelAim()
    {
        if (aimCoroutine != null)
        {
            StopCoroutine(aimCoroutine);
            aimCoroutine = null;
        }

        if (aimLine != null)
            aimLine.Hide();
    }

    // à⁄ìÆÉpÉ^Å[ÉìÇëIë
    private void SelectMovementPattern()
    {
        activeMovementPattern = movementPattern;

        if (movementPattern == MovementPattern.Random)
        {
            int choice = Random.Range(0, 10);

            activeMovementPattern = choice < 4
                ? MovementPattern.NormalChase
                : choice < 7
                    ? MovementPattern.Wave
                    : MovementPattern.Circle;
        }

        specialMovementStarted = false;
        movementTime = 0f;
    }

    // ÉtÉOÇÃà⁄ìÆÉpÉ^Å[ÉìÇçXêV
    private void UpdateMovementPattern()
    {
        if (rb == null || player == null)
            return;

        // âÊñ äOÇ≈ÇÕí èÌí«îˆÇ≈ìoèÍ
        if (!specialMovementStarted)
        {
            if (!IsVisibleOnScreen())
            {
                ChasePlayer();
                return;
            }

            specialMovementStarted = true;
            movementTime = 0f;
            movementBaseY = transform.position.y;

            // åªç›à íuÇ©ÇÁâ~à⁄ìÆÇäJén
            circleCenter =
                rb.position - new Vector2(circleRadiusX, 0f);
        }

        if (activeMovementPattern ==
            MovementPattern.NormalChase)
        {
            ChasePlayer();
            return;
        }

        movementTime += Time.fixedDeltaTime;

        // îgèÛà⁄ìÆ
        if (activeMovementPattern == MovementPattern.Wave)
        {
            float distanceX =
                player.position.x - transform.position.x;

            float horizontal =
                Mathf.Abs(distanceX) > turnThreshold
                    ? Mathf.Sign(distanceX) * chaseSpeed
                    : 0f;

            float targetY =
                movementBaseY +
                Mathf.Sin(
                    movementTime *
                    waveFrequency *
                    Mathf.PI * 2f
                ) * waveAmplitude;

            Vector2 target = ClampMovementTarget(
                new Vector2(rb.position.x, targetY)
            );

            float vertical = Mathf.Clamp(
                (target.y - rb.position.y) * 5f,
                -maxCircleMoveSpeed,
                maxCircleMoveSpeed
            );

            rb.velocity = new Vector2(
                horizontal,
                vertical
            );

            return;
        }

        // â~ÇÃíÜêSÇÇ‰Ç¡Ç≠ÇËÉvÉåÉCÉÑÅ[Ç÷í«è]
        float followStep =
            Mathf.Max(0f, circleCenterFollowSpeed) *
            Time.fixedDeltaTime;

        circleCenter.x = Mathf.MoveTowards(
            circleCenter.x,
            player.position.x,
            followStep
        );

        float angle = movementTime * circleSpeed;

        Vector2 circleTarget =
            circleCenter +
            new Vector2(
                Mathf.Cos(angle) * circleRadiusX,
                Mathf.Sin(angle) * circleRadiusY
            );

        circleTarget = ClampMovementTarget(circleTarget);

        // ë¨ìxÇääÇÁÇ©Ç…êßå‰
        const float circleTrackingStrength = 3f;

        Vector2 desiredVelocity =
            (circleTarget - rb.position) *
            circleTrackingStrength;

        rb.velocity = Vector2.ClampMagnitude(
            desiredVelocity,
            Mathf.Max(0.1f, maxCircleMoveSpeed)
        );
    }

    // âÊñ í[ÅEínñ Ç÷ÇÃà⁄ìÆÇêßå¿
    private Vector2 ClampMovementTarget(Vector2 target)
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
        {
            return new Vector2(
                target.x,
                Mathf.Max(target.y, minimumMoveHeight)
            );
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 center = cam.transform.position;

        Vector3 extents = spriteRenderer != null
            ? spriteRenderer.bounds.extents
            : Vector3.zero;

        float left =
            center.x - halfWidth +
            extents.x + screenMargin;

        float right =
            center.x + halfWidth -
            extents.x - screenMargin;

        float bottom = Mathf.Max(
            center.y - halfHeight +
            extents.y + screenMargin,
            minimumMoveHeight
        );

        float top =
            center.y + halfHeight -
            extents.y - screenMargin;

        target.x = Mathf.Clamp(
            target.x,
            Mathf.Min(left, right),
            Mathf.Max(left, right)
        );

        target.y = Mathf.Clamp(
            target.y,
            Mathf.Min(bottom, top),
            Mathf.Max(bottom, top)
        );

        return target;
    }

    // è]óàÇÃâ°ï˚å¸í«îˆ
    private void ChasePlayer()
    {
        if (player == null || rb == null)
            return;

        float distanceX =
            player.position.x - transform.position.x;

        if (Mathf.Abs(distanceX) <= turnThreshold)
        {
            rb.velocity = new Vector2(
                0f,
                rb.velocity.y
            );

            return;
        }

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
            enemyVisual.localRotation =
                Quaternion.identity;
        }
    }

    public void StopForGameOver()
    {
        if (isGameOverStopped)
            return;

        isGameOverStopped = true;
        CancelAim();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        isAttacking = false;
    }

    private bool IsAboveScreen()
    {
        if (Camera.main == null ||
            spriteRenderer == null)
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
        specialMovementStarted = false;

        CancelAim();

        if (rb != null)
            rb.velocity = Vector2.zero;

        isAttacking = false;
    }

    public void EndSpawnSequence()
    {
        isSpawning = false;
        attackTimer = attackCooldown;
    }

    private void RotateEnemyToAim(Vector2 direction)
    {
        if (enemyVisual == null)
            return;

        float angle = Mathf.Atan2(
            direction.y,
            Mathf.Abs(direction.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(
            angle,
            -maxAimAngle,
            maxAimAngle
        );

        float facingSign =
            Mathf.Sign(transform.localScale.x);

        float targetAngle =
            -angle * facingSign;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, targetAngle);

        enemyVisual.localRotation = Quaternion.Lerp(
            enemyVisual.localRotation,
            targetRotation,
            aimRotateSpeed * Time.deltaTime
        );
    }
}
