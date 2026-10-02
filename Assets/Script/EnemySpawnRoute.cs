using UnityEngine;

public class EnemySpawnRoute : MonoBehaviour
{
    [Header("’Ê‰ßƒ|ƒCƒ“ƒg")]
    [SerializeField] private Transform[] routePoints;

    public Transform[] GetRoutePoints()
    {
        return routePoints;
    }
}