
using UnityEngine;

public class JellyfishMovement : MonoBehaviour
{
    [Header("ïÇóVà⁄ìÆ")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("ç∂âEà⁄ìÆ")]
    [SerializeField] private float moveRange = 2f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float elapsedTime;
    private bool isChargingElectricity = false;

    public void SetChargingElectricity(bool charging)
    {
        isChargingElectricity = charging;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = rb.position;
    }

    private void FixedUpdate()
    {
        if (isChargingElectricity)
        {
            return;
        }
        elapsedTime += Time.fixedDeltaTime;

        // ç∂âEÇ…Ç‰Ç¡Ç≠ÇËâùïú
        float x = Mathf.PingPong(
            elapsedTime * moveSpeed,
            moveRange * 2f
        ) - moveRange;

        // è„â∫Ç…Ç”ÇÌÇ”ÇÌïÇóV
        float y = Mathf.Sin(
            elapsedTime * floatSpeed
        ) * floatHeight;

        Vector2 targetPosition = new Vector2(
            startPosition.x + x,
            startPosition.y + y
        );

        rb.MovePosition(targetPosition);
    }
}
