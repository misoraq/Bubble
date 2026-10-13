
using UnityEngine;

public class RecoveryItem : MonoBehaviour
{
    [Header("ˆÚ“®İ’è")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("‰ñ•œ—Ê")]
    [SerializeField] private int recoveryAmount = 3;

    [Header("Á–ÅŠÔ")]
    [SerializeField] private float lifeTime = 10f;

    private Vector2 moveDirection = Vector2.right;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // ‰_‚©‚ç”ò‚Ño‚·•ûŒü‚ğİ’è
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        // İ’è‚³‚ê‚½•ûŒü‚É”ò‚Ñ‘±‚¯‚é
        transform.position +=
            (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("‡@ ÚGF" + other.name);

        WaterGun waterGun =
      other.GetComponentInChildren<WaterGun>();

        Debug.Log("‡A WaterGunæ“¾F" + (waterGun != null));

        if (waterGun == null)
            return;

        Debug.Log("‡B ‰ñ•œˆ—ŠJn");

        waterGun.RecoverWater(recoveryAmount);

        Debug.Log("‡C ‰ñ•œˆ—I—¹");

        Destroy(gameObject);
    }


}
