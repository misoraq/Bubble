using UnityEngine;

// 小さな黄色い四角を並べて攻撃予告の点線を描画する。
public class EnemyAimLine : MonoBehaviour
{
    [SerializeField] private int dotCount = 18;
    [SerializeField] private float dotSpacing = 0.35f;
    [SerializeField] private float dotSize = 0.07f;
    [SerializeField] private Color dotColor = Color.yellow;
    [SerializeField] private int sortingOrder = 100;

    private Transform[] dots;

    private void Awake()
    {
        dotCount = Mathf.Max(1, dotCount);
        Sprite sprite = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        dots = new Transform[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = new GameObject("AimDot_" + i);
            dot.transform.SetParent(transform, false);
            dot.transform.localScale = Vector3.one * dotSize;
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = dotColor;
            sr.sortingOrder = sortingOrder;
            dots[i] = dot.transform;
            dot.SetActive(false);
        }
    }

    public void Show(Vector2 origin, Vector2 target)
    {
        if (dots == null) return;

        Vector2 difference = target - origin;
        float distance = difference.magnitude;

        if (distance <= 0.001f)
        {
            Hide();
            return;
        }

        Vector2 direction = difference.normalized;

        for (int i = 0; i < dots.Length; i++)
        {
            float dotDistance = dotSpacing * (i + 1);

            // プレイヤーまでの距離を超える点は非表示
            if (dotDistance > distance)
            {
                dots[i].gameObject.SetActive(false);
                continue;
            }

            dots[i].position = origin + direction * dotDistance;
            dots[i].gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (dots == null) return;
        foreach (Transform dot in dots)
            dot.gameObject.SetActive(false);
    }
}
