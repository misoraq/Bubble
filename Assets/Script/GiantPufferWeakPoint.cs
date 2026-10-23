using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GiantPufferWeakPoint : MonoBehaviour
{
    [SerializeField] private GiantPufferBoss boss;

    private void Awake()
    {
        if (boss == null)
            boss = GetComponentInParent<GiantPufferBoss>(true);

        Collider2D hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
    }

    public bool TryHit()
    {
        return boss != null && boss.TryDamageWeakPoint();
    }
}
