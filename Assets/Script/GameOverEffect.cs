using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverEffect : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("‰‰oŠÔ")]
    [SerializeField] private float darkenDuration = 0.8f;
    [SerializeField] private float shrinkDuration = 0.8f;
    [SerializeField] private float waitDuration = 0.5f;

    [Header("ƒvƒŒƒCƒ„[üˆÍ‚Å’â~‚·‚éŠÔ")]
    [SerializeField] private float circleHoldDuration = 1f;

    [Header("Š®‘SˆÃ“]‚Ü‚Å‚ÌŠÔ")]
    [SerializeField] private float finalDarkenDuration = 0.5f;
    [Header("GAME OVER—‰º‰‰o")]
    [SerializeField] private GameOverDropUI gameOverDropUI;
    [Header("Game Over UI")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("‰~‚Ì‘å‚«‚³")]
    [SerializeField] private float startRadius = 0.65f;
    [SerializeField] private float endRadius = 0.15f;

    [Header("ˆÃ‚³")]
    [SerializeField] private float maxAlpha = 0.9f;

    private Image effectImage;
    private Material effectMaterial;

    private bool isGameOver = false;

    private void Awake()
    {
        effectImage = GetComponent<Image>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (effectImage != null && effectImage.material != null)
        {
            // ‚±‚ÌGameOverEffectê—p‚ÌMaterial‚ğì‚é
            effectMaterial = new Material(effectImage.material);

            effectImage.material = effectMaterial;
        }

        // Å‰‚ÍˆÃ“]‚µ‚È‚¢
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_Alpha", 0f);
            effectMaterial.SetFloat("_Radius", startRadius);
        }

        // Å‰‚ÍGame Over Panel‚ğ”ñ•\¦
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (player == null || mainCamera == null || effectMaterial == null)
            return;

        // Player‚Ìƒ[ƒ‹ƒhÀ•W‚ğViewportÀ•W‚Ö•ÏŠ·
        Vector3 screenPosition =
            mainCamera.WorldToViewportPoint(player.position);

        // ‰~‚Ì’†S‚ğPlayer‚É‡‚í‚¹‚é
        effectMaterial.SetVector(
            "_Center",
            new Vector4(
                screenPosition.x,
                screenPosition.y,
                0f,
                0f
            )
        );
    }

    public void StartGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        // Player‚Ì‘€ì‚ğ~‚ß‚é
        StopPlayer();

        // ‰‰oŠJn
        StartCoroutine(GameOverSequence());
    }

    private void StopPlayer()
    {
        if (player == null)
            return;

        // PlayerMove‚ğ’â~
        PlayerMove playerMove =
            player.GetComponent<PlayerMove>();

        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        // Player‚ÌˆÚ“®‘¬“x‚ğ~‚ß‚é
        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private IEnumerator GameOverSequence()
    {
        // -------------------------
        // ‡@ üˆÍ‚ğ™X‚ÉˆÃ‚­‚·‚é
        // -------------------------

        float timer = 0f;

        while (timer < darkenDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / darkenDuration);

            // ­‚µŠŠ‚ç‚©‚É
            t = Mathf.SmoothStep(0f, 1f, t);

            effectMaterial.SetFloat(
                "_Alpha",
                Mathf.Lerp(0f, maxAlpha, t)
            );

            yield return null;
        }

        // -------------------------
        // ‡A ‰~‚ğ™X‚É¬‚³‚­‚·‚é
        // -------------------------

        timer = 0f;

        while (timer < shrinkDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / shrinkDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            effectMaterial.SetFloat(
                "_Radius",
                Mathf.Lerp(startRadius, endRadius, t)
            );

            yield return null;
        }
        // ƒvƒŒƒCƒ„[‚ÌüˆÍ‚ğ‰f‚µ‚½ó‘Ô‚Å’â~
        yield return new WaitForSecondsRealtime(circleHoldDuration);

        // ‰~Œ`‚Ì‹ŠE‚ğŠ®‘S‚É•Â‚¶‚é
        float closeTimer = 0f;

        while (closeTimer < finalDarkenDuration)
        {
            closeTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                closeTimer / Mathf.Max(finalDarkenDuration, 0.01f)
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            effectMaterial.SetFloat(
                "_Radius",
                Mathf.Lerp(endRadius, 0f, t)
            );

            yield return null;
        }

        // Š®‘S‚ÉˆÃ“]‚µ‚½ó‘Ô‚É‚·‚é
        effectMaterial.SetFloat("_Radius", 0f);
        effectMaterial.SetFloat("_Alpha", 1f);
        // -------------------------
        // ‡B ­‚µ’â~
        // -------------------------



        // -------------------------
        // ‡C GAME OVER•\¦
        // -------------------------

        yield return new WaitForSecondsRealtime(waitDuration);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Panel“à‚ÌGameOverImage‚ğ•\¦
            Transform imageTransform =
                gameOverPanel.transform.Find("GameOverImage");

            if (imageTransform != null)
            {
                imageTransform.gameObject.SetActive(true);
            }
        }

        if (gameOverDropUI != null)
        {
            gameOverDropUI.PlayDrop();
        }
        /*if (gameOverUI != null)
        {
            gameOverUI.PlayGameOver();
        }*/
    }
}