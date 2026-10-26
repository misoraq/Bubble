
using UnityEngine;

public class EnemySpawnProtection : MonoBehaviour
{
    [Header("画面内に入ってからの無敵時間")]
    [SerializeField] private float protectionDuration = 0.5f;

    [Header("画面内判定の余白")]
    [SerializeField] private float screenMargin = 0f;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    private float protectionTimer;
    private bool hasEnteredScreen;

    public bool IsProtected
    {
        get
        {
            // まだ画面内に入っていない場合は無敵
            if (!hasEnteredScreen)
                return true;

            // 画面内に入った後も一定時間は無敵
            return protectionTimer > 0f;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        hasEnteredScreen = false;
        protectionTimer = protectionDuration;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        // まだ画面内に入っていない場合
        if (!hasEnteredScreen)
        {
            if (IsInsideScreen())
            {
                hasEnteredScreen = true;

                // 画面内に入った瞬間から無敵時間開始
                protectionTimer = protectionDuration;
            }

            return;
        }

        // 画面内に入った後の無敵時間
        if (protectionTimer > 0f)
        {
            protectionTimer -= Time.deltaTime;
        }
    }

    private bool IsInsideScreen()
    {
        if (mainCamera == null)
            return false;

        Bounds bounds;

        if (spriteRenderer != null)
        {
            bounds = spriteRenderer.bounds;
        }
        else
        {
            bounds = new Bounds(
                transform.position,
                Vector3.zero
            );
        }

        Vector3 viewportMin =
            mainCamera.WorldToViewportPoint(
                bounds.min
            );

        Vector3 viewportMax =
            mainCamera.WorldToViewportPoint(
                bounds.max
            );

        // カメラの背後にいる場合は画面外
        if (viewportMin.z <= 0f ||
            viewportMax.z <= 0f)
        {
            return false;
        }

        // 敵のSpriteが少しでも画面内に入ったら判定
        bool insideX =
            viewportMax.x >= screenMargin &&
            viewportMin.x <= 1f - screenMargin;

        bool insideY =
            viewportMax.y >= screenMargin &&
            viewportMin.y <= 1f - screenMargin;

        return insideX && insideY;
    }

    // 必要に応じて無敵状態をリセット
    public void StartProtection()
    {
        hasEnteredScreen = false;
        protectionTimer = protectionDuration;
    }
}
