using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class GiantPufferProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float age;
    private bool initialized;

    public void Initialize(Vector2 newDirection, float newSpeed, float newLifetime)
    {
        direction = newDirection.sqrMagnitude > 0.001f ? newDirection.normalized : Vector2.left;
        speed = Mathf.Max(0f, newSpeed);
        lifetime = Mathf.Max(0.1f, newLifetime);
        initialized = true;
    }

    private void Awake()
    {
        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
    }

    private void Update()
    {
        if (!initialized) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        age += Time.deltaTime;
        if (age >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || other.isTrigger) return;
        if (other.GetComponentInParent<GiantPufferBoss>() != null) return;
        if (other.GetComponentInParent<Bubble>() != null) return;
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            Debug.Log("ボスの攻撃がプレイヤーに命中しました（ダメージ連携は次の工程）。");
            Destroy(gameObject);
        }
    }
}
