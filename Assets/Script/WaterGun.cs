using UnityEngine;

public class WaterGun : MonoBehaviour
{
    [Header("水鉄砲")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bubblePrefab;

    private GameObject chargingBubble;
    private Bubble chargingBubbleScript;

    private void Update()
    {
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
        // シャボン玉を銃口に生成
        chargingBubble = Instantiate(
            bubblePrefab,
            muzzle.position,
            muzzle.rotation
        );

        // Bubbleスクリプトを取得
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

        // 参照をリセット
        chargingBubble = null;
        chargingBubbleScript = null;
    }
}