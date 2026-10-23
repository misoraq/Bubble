using System.Collections;
using UnityEngine;

public class GiantPufferBoss : MonoBehaviour
{
    [Header("既存の参照")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private GameObject weakPointObject;
    [SerializeField] private GameObject warningObject;
    [SerializeField] private Transform attackPoint;

    [Header("ボス本体のAnimator")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private string idleState = "Boss_Idle";
    [SerializeField] private string bubbleState = "Boss_BubbleAttack";
    [SerializeField] private string spikeState = "Boss_SpikeAttack";
    [SerializeField] private string waveState = "Boss_WaveAttack";
    [SerializeField] private string tiredState = "Boss_Tired";

    [Header("HPと行動時間")]
    [SerializeField, Min(1)] private int maxHP = 3;
    [SerializeField] private float idleDuration = 1f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float weakPointDuration = 2.5f;

    [Header("弾の見た目")]
    [SerializeField] private Sprite bubbleShotSprite;
    [SerializeField] private Sprite spikeSprite;
    [SerializeField] private Sprite waveSprite;
    [SerializeField] private int projectileSortingOrder = 15;

    [Header("バブルショット")]
    [SerializeField] private int bubbleShotCount = 5;
    [SerializeField] private float bubbleSpreadAngle = 60f;
    [SerializeField] private float bubbleShotSpeed = 4.5f;
    [SerializeField] private float bubbleShotSize = 0.35f;
    [SerializeField] private float bubbleWindupDuration = 0.35f;

    [Header("トゲ膨張")]
    [SerializeField] private int spikeCount = 8;
    [SerializeField] private float spikeSpeed = 5f;
    [SerializeField] private float spikeSize = 0.35f;
    [SerializeField] private float inflateScale = 1.4f;
    [SerializeField] private float inflateDuration = 0.7f;

    [Header("水流ウェーブ")]
    [SerializeField] private float waveSpeed = 6f;
    [SerializeField] private float waveSize = 0.85f;
    [SerializeField] private float waveGroundY = -3.3f;
    [SerializeField] private float waveWindupDuration = 0.5f;

    [Header("共通")]
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private float attackRecoveryDuration = 0.45f;
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color damageColor = Color.white;

    private int currentHP;
    private bool weakPointOpen;
    private bool damagedThisOpening;
    private bool defeated;
    private Transform player;
    private Color originalColor;
    private Vector3 originalScale;
    private int attackIndex;
    private Coroutine battleCoroutine;
    private Coroutine flashCoroutine;

    public int CurrentHP => currentHP;
    public bool IsDefeated => defeated;
    public bool IsWeakPointOpen => weakPointOpen && !defeated;

    private void Awake()
    {
        currentHP = Mathf.Max(1, maxHP);
        originalScale = transform.localScale;

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<SpriteRenderer>();

        if (bodyAnimator == null && bodyRenderer != null)
            bodyAnimator = bodyRenderer.GetComponent<Animator>();

        originalColor = bodyRenderer != null ? bodyRenderer.color : Color.white;
        SetWeakPoint(false);

        if (warningObject != null)
            warningObject.SetActive(false);
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogWarning("GiantPufferBoss: Playerタグのオブジェクトが見つかりません。");

        PlayAnimation(idleState);
        battleCoroutine = StartCoroutine(BattleRoutine());
    }

    private IEnumerator BattleRoutine()
    {
        while (!defeated)
        {
            PlayAnimation(idleState);
            yield return new WaitForSeconds(Mathf.Max(0f, idleDuration));

            int pattern = ChooseAttackPattern();
            yield return WarningRoutine();
            if (defeated) yield break;

            if (pattern == 0)
                yield return BubbleShotRoutine();
            else if (pattern == 1)
                yield return SpikeRoutine();
            else
                yield return WaveRoutine();

            if (defeated) yield break;

            yield return new WaitForSeconds(Mathf.Max(0f, attackRecoveryDuration));
            if (defeated) yield break;

            damagedThisOpening = false;
            SetWeakPoint(true);
            PlayAnimation(tiredState);

            yield return new WaitForSeconds(Mathf.Max(0.1f, weakPointDuration));
            if (defeated) yield break;

            SetWeakPoint(false);
            PlayAnimation(idleState);
        }
    }

    private int ChooseAttackPattern()
    {
        // HP3: 泡のみ / HP2: 泡とトゲ / HP1: 全3種類
        int unlocked = currentHP >= 3 ? 1 : (currentHP == 2 ? 2 : 3);
        int pattern = attackIndex % unlocked;
        attackIndex++;
        return pattern;
    }

    private IEnumerator WarningRoutine()
    {
        PlayAnimation(idleState);
        if (warningObject != null)
            warningObject.SetActive(true);

        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, warningDuration);

        while (elapsed < duration && !defeated)
        {
            elapsed += Time.deltaTime;
            if (bodyRenderer != null)
            {
                bodyRenderer.color = Mathf.FloorToInt(elapsed * 10f) % 2 == 0
                    ? warningColor : originalColor;
            }
            yield return null;
        }

        if (bodyRenderer != null)
            bodyRenderer.color = originalColor;

        if (warningObject != null)
            warningObject.SetActive(false);
    }

    private IEnumerator BubbleShotRoutine()
    {
        PlayAnimation(bubbleState);
        yield return new WaitForSeconds(Mathf.Max(0f, bubbleWindupDuration));
        if (defeated) yield break;

        Vector2 origin = GetAttackPosition();
        Vector2 direction = player != null
            ? (Vector2)player.position - origin : Vector2.left;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.left;
        direction.Normalize();

        int count = Mathf.Max(1, bubbleShotCount);
        for (int i = 0; i < count; i++)
        {
            float ratio = count == 1 ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-bubbleSpreadAngle * 0.5f,
                bubbleSpreadAngle * 0.5f, ratio);
            Vector2 shotDirection = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnProjectile("BossBubble", bubbleShotSprite, origin,
                shotDirection, bubbleShotSpeed, bubbleShotSize);
        }

        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator SpikeRoutine()
    {
        PlayAnimation(spikeState);

        Vector3 from = originalScale;
        Vector3 to = originalScale * Mathf.Max(1f, inflateScale);
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, inflateDuration);

        while (elapsed < duration && !defeated)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to,
                Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        if (defeated) yield break;

        Vector2 origin = transform.position;
        int count = Mathf.Max(1, spikeCount);
        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            SpawnProjectile("BossSpike", spikeSprite,
                origin + direction * 0.65f, direction, spikeSpeed, spikeSize);
        }

        elapsed = 0f;
        while (elapsed < duration && !defeated)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(to, from,
                Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private IEnumerator WaveRoutine()
    {
        PlayAnimation(waveState);
        yield return new WaitForSeconds(Mathf.Max(0f, waveWindupDuration));
        if (defeated) yield break;

        Vector2 origin = new Vector2(transform.position.x, waveGroundY);
        Vector2 direction = player != null && player.position.x > transform.position.x
            ? Vector2.right : Vector2.left;

        SpawnProjectile("BossWave", waveSprite, origin,
            direction, waveSpeed, waveSize);

        yield return new WaitForSeconds(0.25f);
    }

    private void PlayAnimation(string stateName)
    {
        if (bodyAnimator == null || !bodyAnimator.isActiveAndEnabled ||
            bodyAnimator.runtimeAnimatorController == null ||
            string.IsNullOrEmpty(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);
        if (bodyAnimator.HasState(0, stateHash))
            bodyAnimator.Play(stateHash, 0, 0f);
        else
            Debug.LogWarning("GiantPufferBoss: AnimatorにStateがありません: " + stateName);
    }

    private Vector2 GetAttackPosition()
    {
        return attackPoint != null
            ? (Vector2)attackPoint.position : (Vector2)transform.position;
    }

    private void SpawnProjectile(string objectName, Sprite sprite,
        Vector2 position, Vector2 direction, float speed, float size)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * Mathf.Max(0.05f, size);

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = projectileSortingOrder;

        if (sprite == null)
            Debug.LogWarning("GiantPufferBoss: " + objectName + " のSpriteが未設定です。");

        CircleCollider2D hitbox = obj.AddComponent<CircleCollider2D>();
        hitbox.isTrigger = true;

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        GiantPufferProjectile projectile = obj.AddComponent<GiantPufferProjectile>();
        projectile.Initialize(direction, speed, projectileLifetime);
    }

    private void SetWeakPoint(bool open)
    {
        weakPointOpen = open;
        if (weakPointObject != null)
            weakPointObject.SetActive(open);
    }

    public bool TryDamageWeakPoint()
    {
        if (defeated || !weakPointOpen || damagedThisOpening)
            return false;

        damagedThisOpening = true;
        currentHP--;
        SetWeakPoint(false);
        Debug.Log("ボスに命中！ 残りHP: " + currentHP);

        if (currentHP <= 0)
        {
            defeated = true;
            if (battleCoroutine != null)
                StopCoroutine(battleCoroutine);
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            if (warningObject != null)
                warningObject.SetActive(false);
            Debug.Log("巨大フグボス撃破！ ステージクリア連携は次の工程で追加します。");
            gameObject.SetActive(false);
        }
        else
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlash());
        }

        return true;
    }

    private IEnumerator DamageFlash()
    {
        if (bodyRenderer == null)
            yield break;

        bodyRenderer.color = damageColor;
        yield return new WaitForSeconds(0.15f);
        if (bodyRenderer != null)
            bodyRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        if (bodyRenderer != null)
            bodyRenderer.color = originalColor;
    }
}
