using UnityEngine;

public class JellyfishElectricBullet : MonoBehaviour
{
    [Header("電撃弾の設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lifeTime = 5f;

    [Header("電撃ヒット時の水ダメージ")]
    [SerializeField] private int waterDamage = 2;

    private Vector2 moveDirection = Vector2.left;
    private bool hasHitPlayer = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            moveDirection = direction.normalized;
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer) return;
        Debug.Log("電撃が接触：" + other.gameObject.name);

        // Playerタグが付いているオブジェクトを探す
        Transform target = other.transform;

        while (target != null && !target.CompareTag("Player"))
        {
            target = target.parent;
        }

        // プレイヤー以外に接触した場合は何もしない
        if (target == null)
        {
            return;
        }

        // プレイヤーの水鉄砲を取得
        WaterGun waterGun = target.GetComponentInChildren<WaterGun>();

        if (waterGun != null)
        {
            Debug.Log("感電成功！水鉄砲を3秒間停止");

            hasHitPlayer = true;
            waterGun.DisableShootingByElectricity();

            // 電撃による水の減少（1発につき1回のみ）
            if (waterDamage > 0)
                waterGun.TakeDamage(waterDamage);

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Playerは見つかりましたがWaterGunがありません");
        }
    }


}
