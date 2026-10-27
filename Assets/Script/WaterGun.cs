
using UnityEngine;

public class WaterGun : MonoBehaviour
{
    private PlayerMove playerMove;
    [Header("水切れ")]
    [SerializeField] private float emptyGameOverTime = 3f;

    private float emptyTimer = 0f;
    private bool isWaterEmpty = false;

    [Header("水鉄砲")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bubblePrefab;

    [Header("水鉄砲の水")]
    [SerializeField] private int maxWater = 10;
    [SerializeField] private int currentWater = 10;

    [Header("水消費量")]
    [SerializeField] private int normalWaterCost = 1;
    [SerializeField] private int fullChargeWaterCost = 2;

    [Header("ゲームオーバー")]
    [SerializeField] private GameOverEffect gameOverEffect;

    [Header("泡と銃口の距離調整")]
    [SerializeField] private float bubbleOffset = 0.15f;

    [Header("水切れ演出")]
    [SerializeField] private DangerFlash dangerFlash;

    [Header("射撃方向切り替え")]
    [SerializeField] private Transform gunPivot;
    [SerializeField] private float upwardAngle = -30f;

    private bool isUpwardAim = false;
    private bool isGameOver = false;

    [Header("クラゲの電撃による射撃停止")]
    [SerializeField] private float electricDisableDuration = 3f;
    private float electricDisableTimer = 0f;
    private bool mustReleaseMouseAfterShock = false;

    private GameObject chargingBubble;
    private Bubble chargingBubbleScript;
    private ElectricStunVisual electricStunVisual;
    private void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        electricStunVisual = GetComponent<ElectricStunVisual>();
        UpdateGunRotation();
    }

    private void Update()
    {
        if (PhaseAnnouncementUI.IsPlaying)
        {
            CancelChargingForPhase();
            mustReleaseMouseAfterShock = true;
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (isWaterEmpty)
        {
            UpdateWaterEmpty();
        }

        if (electricDisableTimer > 0f)
        {
            electricDisableTimer = Mathf.Max(0f, electricDisableTimer - Time.deltaTime);

            // 感電が終了した瞬間に銃の見た目を戻す
            if (electricDisableTimer <= 0f && electricStunVisual != null)
            {
                electricStunVisual.EndStunVisual();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mustReleaseMouseAfterShock = false;
        }

        // 電撃中は射撃・チャージを禁止する（照準切り替えは可能）
        if (electricDisableTimer > 0f || mustReleaseMouseAfterShock)
        {
            return;
        }
        if (playerMove != null && playerMove.IsCrabStunned)
        {
            return;
        }
        // 右クリックで射撃方向を切り替える
        if (Input.GetMouseButtonDown(1))
        {
            isUpwardAim = !isUpwardAim;
            UpdateGunRotation();
        }

        // 左クリックでチャージ開始
        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        // 左クリック長押しでチャージ
        if (Input.GetMouseButton(0))
        {
            ChargeBubble();
        }

        // 左クリックを離して発射
        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
        }
    }

    // 銃と手を一緒に回転
    private void CancelChargingForPhase()
    {
        if (chargingBubble != null)
            Destroy(chargingBubble);

        chargingBubble = null;
        chargingBubbleScript = null;
    }

    private void UpdateGunRotation()
    {
        if (gunPivot == null)
        {
            return;
        }

        float angle = isUpwardAim ? upwardAngle : 0f;

        gunPivot.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    // プレイヤーの向き
    private float GetFacingDirection()
    {
        return Mathf.Abs(
            Mathf.DeltaAngle(transform.eulerAngles.y, 180f)
        ) < 1f ? 1f : -1f;
    }

    // 水平または斜め上の発射方向
    private Vector2 GetShootDirection()
    {
        float direction = GetFacingDirection();

        float angle = isUpwardAim
            ? Mathf.Abs(upwardAngle)
            : 0f;

        float radians = angle * Mathf.Deg2Rad;

        return new Vector2(
            direction * Mathf.Cos(radians),
            Mathf.Sin(radians)
        ).normalized;
    }

    private void StartCharging()
    {
        if (chargingBubble != null)
        {
            return;
        }

        // 水が足りなければチャージしない
        if (currentWater < normalWaterCost)
        {
            return;
        }

        chargingBubble = Instantiate(
            bubblePrefab,
            muzzle.position,
            muzzle.rotation
        );

        chargingBubbleScript =
            chargingBubble.GetComponent<Bubble>();

        if (chargingBubbleScript != null)
        {
            chargingBubbleScript.StartCharging();
        }
    }

    private void ChargeBubble()
    {
        if (chargingBubbleScript == null ||
            chargingBubble == null)
        {
            return;
        }

        // 泡を成長させる
        chargingBubbleScript.Charge();

        // 現在の射撃方向を泡に設定
        Vector2 shootDirection = GetShootDirection();
        chargingBubbleScript.SetDirection(shootDirection);

        SpriteRenderer bubbleRenderer =
            chargingBubble.GetComponent<SpriteRenderer>();

        if (bubbleRenderer != null)
        {
            float bubbleRadius =
                bubbleRenderer.bounds.extents.x;

            // 水平射撃と斜め射撃で泡の距離を調整
            float distance = bubbleRadius - bubbleOffset;

            if (isUpwardAim)
            {
                // 斜め射撃では泡を銃口に近づける
                distance -= 0.35f;
            }

            distance = Mathf.Max(0f, distance);

            Vector3 offset =
                (Vector3)shootDirection * distance;

            chargingBubble.transform.position =
                muzzle.position + offset;
        }
        else
        {
            chargingBubble.transform.position =
                muzzle.position;
        }

        chargingBubble.transform.rotation = muzzle.rotation;
    }

    private void Shoot()
    {
        if (chargingBubble == null ||
            chargingBubbleScript == null)
        {
            return;
        }

        // 発射方向を確定
        Vector2 shootDirection = GetShootDirection();

        chargingBubbleScript.SetDirection(shootDirection);

        // 泡を発射
        chargingBubbleScript.Shoot();

        // 水鉄砲を実際に発射したときだけ効果音を鳴らす
        GameSoundManager.Instance?.PlayWaterGunShoot();

        // チャージ量に応じて水を消費
        int waterCost;

        if (chargingBubbleScript.IsFullCharge())
        {
            waterCost = fullChargeWaterCost;
        }
        else
        {
            waterCost = normalWaterCost;
        }

        currentWater -= waterCost;
        currentWater = Mathf.Max(currentWater, 0);

        if (currentWater <= 0)
        {
            StartWaterEmpty();
        }

        chargingBubble = null;
        chargingBubbleScript = null;
    }

    // クラゲの電撃を受けたときに呼び出す
    public void DisableShootingByElectricity()
    {
        if (isGameOver) return;

        electricDisableTimer = electricDisableDuration;

        if (electricStunVisual != null)
        {
            electricStunVisual.StartStunVisual();
        }
        mustReleaseMouseAfterShock = true;

        // チャージ中の泡は水を消費せずキャンセル
        if (chargingBubble != null)
        {
            Destroy(chargingBubble);
            chargingBubble = null;
            chargingBubbleScript = null;
        }

        Debug.Log("感電！水鉄砲を" + electricDisableDuration + "秒間使用できません");
    }

    public bool IsShootingDisabledByElectricity()
    {
        return electricDisableTimer > 0f;
    }

    public int GetCurrentWater()
    {
        return currentWater;
    }

    public int GetMaxWater()
    {
        return maxWater;
    }

    private void StartWaterEmpty()
    {
        if (isWaterEmpty)
        {
            return;
        }

        isWaterEmpty = true;
        emptyTimer = emptyGameOverTime;
        GameSoundManager.Instance?.PlayWarningSound();

        if (dangerFlash != null)
        {
            dangerFlash.StartFlash(emptyGameOverTime);
        }
    }

    private void UpdateWaterEmpty()
    {
        if (currentWater > 0)
        {
            isWaterEmpty = false;
            GameSoundManager.Instance?.StopWarningSound();

            if (dangerFlash != null)
            {
                dangerFlash.StopFlash();
            }

            return;
        }

        emptyTimer -= Time.deltaTime;

        if (emptyTimer <= 0f)
        {
            isWaterEmpty = false;
            GameSoundManager.Instance?.StopWarningSound();

            if (dangerFlash != null)
            {
                dangerFlash.StopFlash();
            }

            GameOver();
        }
    }

    private void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        // ゲームオーバー確定時に1回だけ再生
        GameSoundManager.Instance?.PlayGameOver();

        Debug.Log("水残量0！ GAME OVER");

        // すべての敵を停止
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.StopForGameOver();
        }

        // ゲームオーバー演出
        if (gameOverEffect != null)
        {
            gameOverEffect.StartGameOver();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver)
        {
            return;
        }

        if (damage <= 0) return;

        currentWater -= damage;
        currentWater = Mathf.Max(currentWater, 0);

        // 敵の攻撃で水が減ったときに再生
        GameSoundManager.Instance?.PlayPlayerDamage();

        Debug.Log("ダメージ！ 水残量 : " + currentWater);

        if (currentWater <= 0)
        {
            StartWaterEmpty();
        }
    }

    public void RecoverWater(int amount)
    {
        if (isGameOver)
        {
            return;
        }

        currentWater += amount;

        currentWater = Mathf.Clamp(
            currentWater,
            0,
            maxWater
        );

        Debug.Log(
            "水回復！ 現在の水 : " +
            currentWater +
            " / " +
            maxWater
        );
    }

    public void CancelChargingByCrab()
    {
        // 突進を受けた瞬間、チャージ中の泡を消す
        if (chargingBubble != null)
        {
            Destroy(chargingBubble);
            chargingBubble = null;
            chargingBubbleScript = null;
        }

        // 行動不能が解除されるまで、
        // マウスを押しっぱなしにしても再チャージさせない
        mustReleaseMouseAfterShock = true;
    }

    // ステージクリア・ゲームオーバー時に発射とチャージを停止
    public void StopForStageEnd()
    {
        isGameOver = true;
        mustReleaseMouseAfterShock = true;
        electricDisableTimer = 0f;
        isWaterEmpty = false;
        GameSoundManager.Instance?.StopWarningSound();

        if (chargingBubble != null)
            Destroy(chargingBubble);

        chargingBubble = null;
        chargingBubbleScript = null;

        if (electricStunVisual != null)
            electricStunVisual.EndStunVisual();

        if (dangerFlash != null)
            dangerFlash.StopFlash();
    }

}
