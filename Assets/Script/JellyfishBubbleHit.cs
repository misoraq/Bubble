using UnityEngine;

public class JellyfishBubbleHit : MonoBehaviour
{
    [Header("ñAÇÃå©ÇΩñ⁄ÅiéqÉIÉuÉWÉFÉNÉgÅj")]
    [SerializeField] private GameObject bubbleVisual;
    [SerializeField] private float smallBubbleScale = 0.2f;
    [SerializeField] private float fullChargeBubbleScale = 0.35f;

    [Header("ïﬂäléûä‘Ç∆è„è∏ë¨ìx")]
    [SerializeField] private float smallBubbleTime = 3f;
    [SerializeField] private float fullChargeBubbleTime = 7f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private int requiredHits = 3;

    [Header("í èÌíeÇ…ÇÊÇÈí‚é~")]
    [SerializeField] private float stunDuration = 2f;

    private float stunTimer = 0f;

    [Header("åÇîj")]
    [SerializeField] private float defeatHeightOffset = 0.5f;
    [SerializeField] private int waterRecoveryAmount = 2;

    private JellyfishMovement movement;
    private JellyfishAttack attack;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int currentHits;
    private float bubbleTimer;
    private bool isBubbled;
    private bool isDefeated;

    public bool IsBubbled { get { return isBubbled; } }

    private void Awake()
    {
        movement = GetComponent<JellyfishMovement>();
        attack = GetComponent<JellyfishAttack>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (bubbleVisual != null) bubbleVisual.SetActive(false);
    }


    private void Update()
    {

        if (stunTimer > 0f && !isBubbled && !isDefeated)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                stunTimer = 0f;

                if (movement != null)
                    movement.enabled = true;

                if (attack != null)
                    attack.PauseForBubble(false);
            }
        }

        if (!isBubbled || isDefeated)
        {
            return;
        }

        // ÉnÉäÉZÉìÉ{ÉìÇ∆ìØÇ∂ï˚ñ@Ç≈è„è∏
        transform.position +=
            Vector3.up * floatSpeed * Time.deltaTime;

        // âÊñ è„í[Ç…ìûíBÇµÇΩÇÁåÇîj
        if (IsAboveScreen())
        {
            Defeat();
            return;
        }

        // ïﬂäléûä‘ÇÃÉJÉEÉìÉg
        bubbleTimer -= Time.deltaTime;

        if (bubbleTimer <= 0f)
        {
            ReleaseBubble();
        }
    }



    public void HitByBubble(bool isFullCharge)
    {
        if (isBubbled || isDefeated) return;

        if (isFullCharge)
        {
            stunTimer = 0f;
            GetBubbled(
                fullChargeBubbleScale,
                fullChargeBubbleTime
            );
            return;
        }

        // í èÌíeÇ≈ÉNÉâÉQÇÃà⁄ìÆÅEçUåÇÇí‚é~
        stunTimer = stunDuration;

        if (movement != null)
            movement.enabled = false;

        if (attack != null)
            attack.PauseForBubble(true);

        if (rb != null)
            rb.velocity = Vector2.zero;
    }


    private void GetBubbled(float scale, float duration)
    {
        isBubbled = true;
        bubbleTimer = duration;

        if (attack != null) attack.PauseForBubble(true);
        if (movement != null) movement.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
            bubbleVisual.transform.localScale = Vector3.one * scale;
        }
    }


    private void ReleaseBubble()
    {
        if (!isBubbled || isDefeated)
            return;

        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        isBubbled = false;
        currentHits = 0;

        GetComponent<BubbleFinish>()?.ResetHits();

        if (bubbleVisual != null)
            bubbleVisual.SetActive(false);

        if (movement != null)
            movement.enabled = true;

        if (attack != null)
            attack.PauseForBubble(false);
    }


    private bool IsAboveScreen()
    {
        if (Camera.main == null || spriteRenderer == null) return false;
        float top = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
        return spriteRenderer.bounds.center.y > top - defeatHeightOffset;
    }


    private void Defeat()
    {
        if (!isBubbled || isDefeated)
            return;

        isDefeated = true;

        // åÇîjéûÇ…Ç‡ñAÇ™äÑÇÍÇÈ
        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
            stageManager.AddDefeatedEnemy("Jellyfish");

        WaterGun waterGun =
            FindObjectOfType<WaterGun>();

        if (waterGun != null)
            waterGun.RecoverWater(waterRecoveryAmount);

        Destroy(gameObject);
    }

    public void DefeatByAdditionalHits()
    {
        if (!isBubbled || isDefeated)
            return;

        Defeat();
    }
}
