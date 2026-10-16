
using UnityEngine;

public class EnemyWarning : MonoBehaviour
{
    [Header("三角の設定")]
    [SerializeField] private Transform warningArrow;
    [SerializeField] private float arrowDistance = 0.8f;
    [SerializeField] private float rotationSpeed = 720f;

    private Transform target;
    private Vector3 targetPosition;
    private bool hasTargetPosition;

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        hasTargetPosition = true;
        target = null;

        UpdateArrow(true);
    }

    public void SetTarget(Transform enemy)
    {
        target = enemy;
        hasTargetPosition = enemy != null;

        UpdateArrow(true);
    }

    private void Update()
    {
        UpdateArrow(false);
    }


    private void UpdateArrow(bool instant)
    {
        if (warningArrow == null || !hasTargetPosition)
            return;

        Vector3 destination = target != null
            ? target.position
            : targetPosition;

        // 敵が警告マークの左右どちらにいるか判定
        Camera cam = Camera.main;

        if (cam == null)
            return;

        float screenCenterX = cam.transform.position.x;

        // 画面の中心を基準に敵の出現方向を判定
        bool enemyIsRight = destination.x > screenCenterX;

        // 三角を丸いマークの左右に配置
        float x = enemyIsRight
            ? arrowDistance
            : -arrowDistance;

        warningArrow.localPosition = new Vector3(
            x,
            0f,
            0f
        );

        // 三角の元画像は右向きを基準にする
        float angle = enemyIsRight ? 180f : 0f;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, angle);

        if (instant)
        {
            warningArrow.localRotation = targetRotation;
        }
        else
        {
            warningArrow.localRotation =
                Quaternion.RotateTowards(
                    warningArrow.localRotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }
    }

}
