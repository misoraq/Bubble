
using UnityEngine;

public class CrabStunVisual : MonoBehaviour
{
    [Header("プレイヤー")]
    [SerializeField] private PlayerMove playerMove;

    [Header("点滅させるSpriteRenderer")]
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("演出")]
    [SerializeField]
    private Color stunColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);

    [SerializeField] private float blinkSpeed = 16f;

    private Color[] originalColors;
    private bool wasStunned;

    private void Awake()
    {
        if (playerMove == null)
            playerMove = GetComponent<PlayerMove>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }
    }

    private void Update()
    {
        if (playerMove == null)
            return;

        if (playerMove.IsCrabStunned)
        {
            wasStunned = true;

            float blink =
                (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i].color =
                    Color.Lerp(
                        originalColors[i],
                        stunColor,
                        blink
                    );
            }
        }
        else if (wasStunned)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].color = originalColors[i];
            }

            wasStunned = false;
        }
    }
}
