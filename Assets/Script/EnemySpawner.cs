using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("出現ルート")]
    [SerializeField] private EnemySpawnRoute leftRoute;
    [SerializeField] private EnemySpawnRoute rightRoute;

    [Header("出現移動")]
    [SerializeField] private float spawnMoveSpeed = 3f;

    private Enemy leftEnemy;
    private Enemy rightEnemy;

    private Transform[] leftPoints;
    private Transform[] rightPoints;

    private int leftPointIndex = 1;
    private int rightPointIndex = 1;

    private bool leftArrived = false;
    private bool rightArrived = false;

    private bool aiStarted = false;

    private void Start()
    {
        SpawnEnemies();
    }

    private void Update()
    {
        MoveEnemies();
    }

    private void SpawnEnemies()
    {
        leftPoints = leftRoute.GetRoutePoints();
        rightPoints = rightRoute.GetRoutePoints();

        if (leftPoints.Length < 2 || rightPoints.Length < 2)
        {
            Debug.LogError("出現ルートには2個以上のPointが必要です！");
            return;
        }

        // 左ルートの敵
        GameObject leftObject = Instantiate(
            enemyPrefab,
            leftPoints[0].position,
            Quaternion.identity
        );

        leftEnemy = leftObject.GetComponent<Enemy>();

        if (leftEnemy != null)
        {
            leftEnemy.StartSpawnSequence();
        }

        // 右ルートの敵
        GameObject rightObject = Instantiate(
            enemyPrefab,
            rightPoints[0].position,
            Quaternion.identity
        );

        rightEnemy = rightObject.GetComponent<Enemy>();

        if (rightEnemy != null)
        {
            rightEnemy.StartSpawnSequence();
        }
    }

    private void MoveEnemies()
    {
        MoveEnemyAlongRoute(
            leftEnemy,
            leftPoints,
            ref leftPointIndex,
            ref leftArrived
        );

        MoveEnemyAlongRoute(
            rightEnemy,
            rightPoints,
            ref rightPointIndex,
            ref rightArrived
        );

        // 2体とも最後まで到着
        if (leftArrived && rightArrived && !aiStarted)
        {
            StartEnemyAI();
        }
    }

    private void MoveEnemyAlongRoute(
        Enemy enemy,
        Transform[] points,
        ref int pointIndex,
        ref bool arrived
    )
    {
        if (enemy == null || arrived)
        {
            return;
        }

        Transform targetPoint = points[pointIndex];

        enemy.transform.position =
            Vector3.MoveTowards(
                enemy.transform.position,
                targetPoint.position,
                spawnMoveSpeed * Time.deltaTime
            );

        // Pointに到着
        if (Vector3.Distance(
                enemy.transform.position,
                targetPoint.position
            ) < 0.01f)
        {
            pointIndex++;

            // 最後のPointまで到着
            if (pointIndex >= points.Length)
            {
                arrived = true;
            }
        }
    }

    private void StartEnemyAI()
    {
        aiStarted = true;

        if (leftEnemy != null)
        {
            leftEnemy.EndSpawnSequence();
        }

        if (rightEnemy != null)
        {
            rightEnemy.EndSpawnSequence();
        }

        Debug.Log("2体とも出現完了！AI開始！");
    }
}