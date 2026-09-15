using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("–A")]
    [SerializeField] private GameObject bubbleVisual;

    [Header("–A‚Ì‘å‚«‚³")]
    [SerializeField] private float smallBubbleScale = 0.2f;
    [SerializeField] private float fullChargeBubbleScale = 0.35f;

    [Header("–A‚Ì§ŒÀŠÔ")]
    [SerializeField] private float smallBubbleTime = 3f;
    [SerializeField] private float fullChargeBubbleTime = 7f;

    [SerializeField] private float floatSpeed = 1.5f;

    [Header("¬‚³‚¢–A")]
    [SerializeField] private int requiredHits = 3;
    [Header("ƒ_ƒbƒVƒ…UI")]
    [SerializeField] private GameObject dashUI;
    private int currentHits = 0;
    private bool isBubbled = false;

    private float bubbleTimer = 0f;

    private void Update()
    {
        if (isBubbled)
        {
            // “G‚ğã‚É•‚‚©‚¹‚é
            transform.position +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // –A‚ÌŠÔ‚ğŒ¸‚ç‚·
            bubbleTimer -= Time.deltaTime;

            // ŠÔØ‚ê
            if (bubbleTimer <= 0f)
            {
                ReleaseBubble();
            }
        }
    }

    public void HitByBubble(bool isFullCharge)
    {
        if (isBubbled)
        {
            return;
        }

        // ƒtƒ‹ƒ`ƒƒ[ƒW‚È‚ç1”­‚Å•ï‚Ş
        if (isFullCharge)
        {
            GetBubbled(
                fullChargeBubbleScale,
                fullChargeBubbleTime
            );

            return;
        }

        // ¬‚³‚¢–A‚Í•¡”‰ñ•K—v
        currentHits++;

        Debug.Log(
            "“G‚É–A‚ª“–‚½‚Á‚½: " +
            currentHits +
            " / " +
            requiredHits
        );

        if (currentHits >= requiredHits)
        {
            GetBubbled(
                smallBubbleScale,
                smallBubbleTime
            );
        }
    }

    private void GetBubbled(
      float bubbleScale,
      float bubbleTime
  )
    {
        isBubbled = true;

        bubbleTimer = bubbleTime;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);

            bubbleVisual.transform.localScale =
                Vector3.one * bubbleScale;
        }

        if (dashUI != null)
        {
            dashUI.SetActive(false);
        }

        Debug.Log(
            "“G‚ª–A‚É•ï‚Ü‚ê‚½I §ŒÀŠÔ: " +
            bubbleTime +
            "•b"
        );
    }

    private void ReleaseBubble()
    {
        isBubbled = false;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }

        if (dashUI != null)
        {
            dashUI.SetActive(false);
        }

        currentHits = 0;

        Debug.Log("–A‚ÌŠÔØ‚êI“G‚ªŒ³‚É–ß‚Á‚½I");
    }
    public bool IsBubbled
    {
        get { return isBubbled; }
    }
    public void ShowDashUI(bool show)
    {
        if (!isBubbled)
        {
            return;
        }

        if (dashUI != null)
        {
            dashUI.SetActive(show);
        }
    }
    public void Defeat()
    {
        if (!isBubbled)
        {
            return;
        }

        Debug.Log("“G‚ğŒ‚”jI");

        ComboManager comboManager =
            FindObjectOfType<ComboManager>();

        if (comboManager != null)
        {
            comboManager.AddCombo();
        }

        ScoreManager scoreManager =
            FindObjectOfType<ScoreManager>();

        if (scoreManager != null)
        {
            int comboCount = 1;

            if (comboManager != null)
            {
                comboCount = comboManager.GetComboCount();
            }

            scoreManager.AddScore(comboCount);
        }

        Destroy(gameObject);
    }
}