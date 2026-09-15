using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("–A")]
    [SerializeField] private GameObject bubbleVisual;

    [SerializeField] private float floatSpeed = 1.5f;

    private bool isBubbled = false;

    private void Update()
    {
        if (isBubbled)
        {
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;
        }
    }

    public void GetBubbled()
    {
        if (isBubbled)
        {
            return;
        }

        isBubbled = true;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }
    }
}