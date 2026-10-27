
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CrabAttack))]
public class CrabBubbleHit : MonoBehaviour
{
    [Header("通常弾")]
    [SerializeField] private float stunDuration = 2f;


    [Header("フルチャージ捕獲")]
    [SerializeField] private GameObject captureBubblePrefab;
    [SerializeField] private float captureDuration = 5f;
    [SerializeField] private float riseSpeed = 1.5f;

    private CrabAttack crabAttack;
    private Rigidbody2D rb;
    private GameObject captureBubble;
    private Coroutine stunCoroutine;
    private Coroutine captureCoroutine;
    [Header("撃破時の水回復")]
    [SerializeField] private int waterRecoveryAmount = 2;
    public bool IsBubbled { get; private set; }

    private void Awake()
    {
        crabAttack = GetComponent<CrabAttack>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void HitByBubble(bool isFullCharge)
    {
        // 画面外および画面に入ってからの無敵時間中は泡攻撃を無効化
        EnemySpawnProtection protection = GetComponent<EnemySpawnProtection>();
        if (protection != null && protection.IsProtected)
            return;

        if (IsBubbled)
            return;

        if (isFullCharge)
            Capture();
        else
            Stun();
    }

    private void Stun()
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        crabAttack.SetPaused(true);
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        stunCoroutine = null;

        if (!IsBubbled)
            crabAttack.SetPaused(false);
    }

    private void Capture()
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        IsBubbled = true;
        GameSoundManager.Instance?.PlayEnemyTrap();
        crabAttack.SetPaused(true);


        if (captureBubblePrefab != null)
        {
            captureBubble = Instantiate(
                captureBubblePrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            // 非アクティブなPrefabでも必ず表示する
            captureBubble.SetActive(true);

            captureBubble.transform.localPosition =
                Vector3.zero;

            captureBubble.transform.localRotation =
                Quaternion.identity;

            // 泡のサイズ
            captureBubble.transform.localScale =
                new Vector3(0.5f, 0.5f, 0.5f);

            SpriteRenderer bubbleRenderer =
                captureBubble.GetComponent<SpriteRenderer>();

            SpriteRenderer crabRenderer =
                GetComponent<SpriteRenderer>();

            if (bubbleRenderer != null)
            {
                bubbleRenderer.enabled = true;

                if (crabRenderer != null)
                {
                    bubbleRenderer.sortingLayerID =
                        crabRenderer.sortingLayerID;

                    bubbleRenderer.sortingOrder =
                        crabRenderer.sortingOrder + 1;
                }
            }

            Debug.Log("カニの捕獲泡を生成しました！");
        }


        captureCoroutine = StartCoroutine(CaptureRoutine());
    }


    private IEnumerator CaptureRoutine()
    {
        float timer = captureDuration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // 捕獲中は上昇する
            transform.position +=
                Vector3.up * riseSpeed * Time.deltaTime;

            // 画面上部に到達したら撃破
            if (IsAboveScreen())
            {
                captureCoroutine = null;
                Defeat();
                yield break;
            }

            yield return null;
        }

        captureCoroutine = null;

        BubbleFinish bubbleFinish = GetComponent<BubbleFinish>();

        // PERFECT撃破が確定している場合は泡解除しない
        if (bubbleFinish == null || !bubbleFinish.IsFinishing)
        {
            Release();
        }
    }

    private bool IsAboveScreen()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return false;

        // カメラ内の上端を取得
        float cameraDistance =
            Mathf.Abs(transform.position.z -
                      cam.transform.position.z);

        float top = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 1f, cameraDistance)
        ).y;

        return transform.position.y >= top;
    }



    private void Release()
    {
        if (!IsBubbled)
            return;

        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        IsBubbled = false;
        captureCoroutine = null;

        GetComponent<BubbleFinish>()?.ResetHits();

        if (captureBubble != null)
        {
            Destroy(captureBubble);
            captureBubble = null;
        }

        if (crabAttack != null)
            crabAttack.SetPaused(false);
    }




    public void Defeat(bool isPerfect = false)
    {
        if (!IsBubbled)
            return;

        if (captureCoroutine != null)
        {
            StopCoroutine(captureCoroutine);
            captureCoroutine = null;
        }

        EnemyBubblePop pop = GetComponent<EnemyBubblePop>();

        if (pop != null)
            pop.PlayPop();

        StageManager stageManager =
            FindObjectOfType<StageManager>();

        if (stageManager != null)
            stageManager.AddDefeatedEnemy("Crab");

        WaterGun waterGun =
            FindObjectOfType<WaterGun>();

        if (waterGun != null)
        {
            int recoveryAmount = isPerfect
                ? waterRecoveryAmount * 2
                : waterRecoveryAmount;

            waterGun.RecoverWater(recoveryAmount);
        }

        Destroy(gameObject);
    }


}
