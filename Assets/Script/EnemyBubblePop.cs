
using UnityEngine;

public class EnemyBubblePop : MonoBehaviour
{
    [Header("泡破裂エフェクト")]
    [SerializeField] private GameObject bubblePopPrefab;

    [Header("エフェクト位置")]
    [SerializeField] private Vector3 offset = Vector3.zero;

    public void PlayPop()
    {
        if (bubblePopPrefab == null)
            return;

        Instantiate(
            bubblePopPrefab,
            transform.position + offset,
            Quaternion.identity
        );
    }
}
