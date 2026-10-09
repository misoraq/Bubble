
using UnityEngine;

public class CloudPairMove : MonoBehaviour
{
    [Header("雲")]
    [SerializeField] private SpriteRenderer cloud1;
    [SerializeField] private SpriteRenderer cloud2;

    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 0.5f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (cloud1 == null || cloud2 == null)
            return;

        // Cloud2をCloud1の左隣に配置
        Vector3 pos = cloud2.transform.position;
        pos.x = cloud1.bounds.min.x - cloud2.bounds.extents.x;
        cloud2.transform.position = pos;
    }

    private void Update()
    {
        if (cloud1 == null || cloud2 == null ||
            mainCamera == null)
            return;

        float movement = moveSpeed * Time.deltaTime;

        cloud1.transform.position += Vector3.right * movement;
        cloud2.transform.position += Vector3.right * movement;

        float distance = Mathf.Abs(
            cloud1.transform.position.z -
            mainCamera.transform.position.z
        );

        float rightEdge = mainCamera.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distance)
        ).x;

        // 完全に画面右側へ抜けた雲を左隣に移動
        if (cloud1.bounds.min.x > rightEdge)
        {
            MoveToLeft(cloud1, cloud2);
        }

        if (cloud2.bounds.min.x > rightEdge)
        {
            MoveToLeft(cloud2, cloud1);
        }
    }

    private void MoveToLeft(
        SpriteRenderer moving,
        SpriteRenderer other)
    {
        Vector3 pos = moving.transform.position;
        pos.x = other.bounds.min.x -
                moving.bounds.extents.x;

        moving.transform.position = pos;
    }
}
