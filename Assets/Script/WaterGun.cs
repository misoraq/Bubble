using UnityEngine;

public class WaterGun : MonoBehaviour
{
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
    [Header("水消費")]
    [SerializeField] private int normalWaterCost = 1;
    [SerializeField] private int fullChargeWaterCost = 2;
    [Header("ゲームオーバー")]
    [SerializeField] private GameOverEffect gameOverEffect;

    [Header("水切れ演出")]
    [SerializeField] private DangerFlash dangerFlash;

    private bool isGameOver = false;
    private GameObject chargingBubble;
    private Bubble chargingBubbleScript;

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }
        if (isWaterEmpty)
        {
            UpdateWaterEmpty();
        }
        // 左クリックを押した瞬間
        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        // 左クリックを押している間
        if (Input.GetMouseButton(0))
        {
            ChargeBubble();
        }

        // 左クリックを離した瞬間
        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
        }
    }

    private void StartCharging()
    {
        // 水が足りなければ撃てない
        if (currentWater < normalWaterCost)
        {
            return;
        }

        chargingBubble = Instantiate(
            bubblePrefab,
            muzzle.position,
            muzzle.rotation
        );

        chargingBubbleScript = chargingBubble.GetComponent<Bubble>();

        if (chargingBubbleScript != null)
        {
            chargingBubbleScript.StartCharging();
        }
    }

    private void ChargeBubble()
    {
        if (chargingBubbleScript != null)
        {
            // シャボン玉を銃口についてこさせる
            chargingBubble.transform.position = muzzle.position;
            chargingBubble.transform.rotation = muzzle.rotation;

            // シャボン玉を膨らませる
            chargingBubbleScript.Charge();
        }
    }

    private void Shoot()
    {
        if (chargingBubble == null || chargingBubbleScript == null)
        {
            return;
        }

        // Playerが右向きか左向きか確認
        float direction;

        if (transform.eulerAngles.y == 180f)
        {
            direction = 1f;
        }
        else
        {
            direction = -1f;
        }

        // シャボン玉を発射
        chargingBubbleScript.SetDirection(direction);

        // 発射状態にする
        chargingBubbleScript.Shoot();

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

        // 参照をリセット
        chargingBubble = null;
        chargingBubbleScript = null;
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

        Debug.Log("水残量0！ GAME OVER");

        // 全ての敵を停止
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.StopForGameOver();
        }

        // GameOver演出
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

        currentWater -= damage;
        currentWater = Mathf.Max(currentWater, 0);

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
   
}