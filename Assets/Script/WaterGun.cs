using UnityEngine;

public class WaterGun : MonoBehaviour
{
    [Header("水鉄砲")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bubblePrefab;
    [Header("水鉄砲の水")]
    [SerializeField] private int maxWater = 10;
    [SerializeField] private int currentWater = 10;
    [SerializeField] private int waterCost = 1;
    [Header("ゲームオーバー")]
    [SerializeField] private GameOverEffect gameOverEffect;

    private bool isGameOver = false;
    private GameObject chargingBubble;
    private Bubble chargingBubbleScript;

    private void Update()
    {
        if (isGameOver)
        {
            return;
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
        if (currentWater < waterCost)
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

        currentWater -= waterCost;

        currentWater = Mathf.Max(currentWater, 0);

        if (currentWater <= 0)
        {
            GameOver();
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
            GameOver();
        }
    }
}