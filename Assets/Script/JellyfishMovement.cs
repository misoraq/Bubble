
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JellyfishMovement : MonoBehaviour
{
    [Header("浮遊移動")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float floatHeight = 0.35f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("左右移動")]
    [SerializeField] private float moveRange = 0.8f;

    [Header("画面外からの登場")]
    [SerializeField] private float approachSpeed = 1.5f;
    [SerializeField] private float screenInsideMargin = 2f;

    private Rigidbody2D rb;
    private Camera mainCamera;

    private Vector2 floatCenter;
    private float elapsedTime;

    private bool isEntering = true;
    private bool isChargingElectricity = false;

    public void SetChargingElectricity(bool charging)
    {
        isChargingElectricity = charging;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        floatCenter = rb.position;
    }

    private void FixedUpdate()
    {
        // 攻撃チャージ中は移動停止
        if (isChargingElectricity)
            return;

        if (isEntering)
        {
            EnterScreen();
            return;
        }

        FloatingMovement();
    }

    private void EnterScreen()
    {
        if (mainCamera == null)
        {
            isEntering = false;
            floatCenter = rb.position;
            return;
        }

        float distance =
            Mathf.Abs(transform.position.z -
                      mainCamera.transform.position.z);

        float left = mainCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distance)
        ).x;

        float right = mainCamera.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distance)
        ).x;

        float centerX = (left + right) * 0.5f;

        // 画面の左から来た場合は右へ、
        // 右から来た場合は左へ移動する
        float targetX = rb.position.x < centerX
            ? left + screenInsideMargin
            : right - screenInsideMargin;

        Vector2 targetPosition = new Vector2(
            targetX,
            rb.position.y
        );

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            approachSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(
            nextPosition,
            targetPosition
        ) <= 0.05f)
        {
            isEntering = false;
            floatCenter = targetPosition;
            elapsedTime = 0f;
        }
    }


    private void FloatingMovement()
    {
        elapsedTime += Time.fixedDeltaTime;

        float x = Mathf.Sin(
            elapsedTime * moveSpeed
        ) * moveRange;

        float y = Mathf.Sin(
            elapsedTime * floatSpeed
        ) * floatHeight;

        Vector2 targetPosition = new Vector2(
            floatCenter.x + x,
            floatCenter.y + y
        );

        // 画面の左右端を超えないように制限
        if (mainCamera != null)
        {
            float distance = Mathf.Abs(
                transform.position.z -
                mainCamera.transform.position.z
            );

            float left = mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, distance)
            ).x;

            float right = mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, distance)
            ).x;

            // 画面端からの余白
            float margin = 1.5f;

            targetPosition.x = Mathf.Clamp(
                targetPosition.x,
                left + margin,
                right - margin
            );
        }

        rb.MovePosition(targetPosition);
    }

}
