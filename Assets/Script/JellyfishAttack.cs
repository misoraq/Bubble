using System.Collections;
using UnityEngine;

public class JellyfishAttack : MonoBehaviour
{
    [Header("ìdåÇçUåÇ")]
    [SerializeField] private GameObject electricBulletPrefab;
    [SerializeField] private Transform attackMuzzle;
    [SerializeField] private float attackInterval = 4f;
    [SerializeField] private float chargeDuration = 1.5f;

    [Header("ë—ìdââèo")]
    [SerializeField] private SpriteRenderer jellyfishRenderer;
    [SerializeField] private Color chargeColor = new Color(1f, 0.9f, 0.25f, 1f);

    private Transform player;
    private JellyfishMovement movement;
    private Color originalColor = Color.white;
    private float attackTimer;
    private bool isCharging;
    private bool isPausedForBubble;
    private Coroutine chargeCoroutine;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        movement = GetComponent<JellyfishMovement>();
        if (jellyfishRenderer == null) jellyfishRenderer = GetComponent<SpriteRenderer>();
        if (jellyfishRenderer != null) originalColor = jellyfishRenderer.color;
        attackTimer = attackInterval;
    }

    private void Update()
    {
        if (player == null || isCharging || isPausedForBubble) return;
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
            chargeCoroutine = StartCoroutine(ChargeAndShoot());
    }

    public void PauseForBubble(bool paused)
    {
        isPausedForBubble = paused;
        if (paused)
        {
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }
            isCharging = false;
            if (jellyfishRenderer != null) jellyfishRenderer.color = originalColor;
            if (movement != null) movement.SetChargingElectricity(false);
        }
        else
        {
            attackTimer = attackInterval;
        }
    }

    private IEnumerator ChargeAndShoot()
    {
        isCharging = true;
        if (movement != null) movement.SetChargingElectricity(true);
        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            if (jellyfishRenderer != null)
            {
                float blink = (Mathf.Sin(elapsed * 18f) + 1f) * 0.5f;
                jellyfishRenderer.color = Color.Lerp(originalColor, chargeColor, blink);
            }
            yield return null;
        }

        if (jellyfishRenderer != null) jellyfishRenderer.color = originalColor;
        ShootElectricBullet();
        if (movement != null) movement.SetChargingElectricity(false);
        attackTimer = attackInterval;
        isCharging = false;
        chargeCoroutine = null;
    }

    private void ShootElectricBullet()
    {
        if (electricBulletPrefab == null || attackMuzzle == null || player == null) return;
        Vector2 direction = (player.position - attackMuzzle.position).normalized;
        GameObject bulletObject = Instantiate(electricBulletPrefab, attackMuzzle.position, Quaternion.identity);
        JellyfishElectricBullet bullet = bulletObject.GetComponent<JellyfishElectricBullet>();
        if (bullet != null) bullet.SetDirection(direction);
    }
}
