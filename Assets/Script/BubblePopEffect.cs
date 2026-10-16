
using UnityEngine;

public class BubblePopEffect : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1f;

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
