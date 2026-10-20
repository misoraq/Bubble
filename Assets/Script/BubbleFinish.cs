using System.Collections;
using UnityEngine;

public class BubbleFinish : MonoBehaviour
{
    [Header("追加攻撃による撃破")]
    [SerializeField] private int requiredHits = 3;
    [Header("3発目の満タン表示時間")]
    [SerializeField] private float finishDelay = 0.25f;
    private int currentHits = 0;
    private bool isDefeated = false;
    [SerializeField] private BubbleHitIndicator hitIndicator;

    private void Update()
    {
        if (hitIndicator == null)
            return;

        // 泡で捕獲されている間は表示
        // 3発目の撃破待機中も満タンゲージを表示する
        bool showIndicator = IsCaptured();

        hitIndicator.gameObject.SetActive(showIndicator);
    }




    public bool IsCaptured()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
            return enemy.IsBubbled;

        JellyfishBubbleHit jellyfish =
            GetComponent<JellyfishBubbleHit>();
        if (jellyfish != null)
            return jellyfish.IsBubbled;

        CrabBubbleHit crab = GetComponent<CrabBubbleHit>();
        if (crab != null)
            return crab.IsBubbled;

        return false;
    }


    public void AddHit()
    {
        if (isDefeated || !IsCaptured())
            return;

        currentHits++;
        if (hitIndicator != null)
        {
            hitIndicator.UpdateHits(currentHits);
        }
        HitTextEffect textEffect =
            FindObjectOfType<HitTextEffect>();

        Debug.Log(
            gameObject.name + " 追加ヒット：" +
            currentHits + "/" + requiredHits
        );

        // 1発目・2発目はHIT!
        if (currentHits < requiredHits)
        {
            if (textEffect != null)
            {
                textEffect.ShowExtraHit(
                    transform,
                    currentHits
                );
            }

            return;
        }

        // 3発目はPERFECT!のみ
        isDefeated = true;

        StartCoroutine(FinishAfterDelay(textEffect));
    }


    private void FinishEnemy()
    {
        Enemy enemy = GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Defeat(true);
            return;
        }

        JellyfishBubbleHit jellyfish =
            GetComponent<JellyfishBubbleHit>();

        if (jellyfish != null)
        {
            jellyfish.DefeatByAdditionalHits(true);
            return;
        }

        CrabBubbleHit crab = GetComponent<CrabBubbleHit>();

        if (crab != null)
        {
            crab.Defeat(true);
        }
    }

    public void ResetHits()
    {
        currentHits = 0;
        isDefeated = false;
        if (hitIndicator != null)
        {
            hitIndicator.UpdateHits(currentHits);
        }
    }

    private IEnumerator FinishAfterDelay(HitTextEffect textEffect)
    {
        // 3発目で満タンになったゲージを表示
        if (hitIndicator != null)
        {
            hitIndicator.UpdateHits(requiredHits);
            hitIndicator.gameObject.SetActive(true);
        }

        // 満タンの状態を少しだけ見せる
        yield return new WaitForSeconds(finishDelay);

        // PERFECT! を表示
        if (textEffect != null)
        {
            textEffect.ShowPerfect(transform);
        }

        // 敵を撃破
        FinishEnemy();
    }

}
