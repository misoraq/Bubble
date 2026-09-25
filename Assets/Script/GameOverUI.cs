using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("GAME OVER")]
    [SerializeField] private Image gameOverImage;

    [Header("GAME OVER Sprite")]
    [SerializeField] private Sprite smallSprite;
    [SerializeField] private Sprite bigSprite;
    [SerializeField] private Sprite burstSprite;
    [SerializeField] private Sprite particlesSprite;

    [Header("Retry")]
    [SerializeField] private GameObject retryButton;

    [Header("ââèoéûä‘")]
    [SerializeField] private float appearDuration = 0.25f;
    [SerializeField] private float inflateDuration = 0.45f;
    [SerializeField] private float burstDuration = 0.12f;
    [SerializeField] private float particleDuration = 0.35f;
    [SerializeField] private float retryDelay = 0.15f;

    [Header("ëÂÇ´Ç≥")]
    [SerializeField] private float startScale = 0.65f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float maxScale = 1.12f;

    private RectTransform gameOverRect;

    private void Awake()
    {
        if (gameOverImage != null)
        {
            gameOverRect =
                gameOverImage.GetComponent<RectTransform>();
        }

        if (retryButton != null)
        {
            retryButton.SetActive(false);
        }

        if (gameOverImage != null)
        {
            gameOverImage.gameObject.SetActive(false);
        }
    }

    public void PlayGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // --------------------------------
        // á@ GAME OVERìoèÍ
        // --------------------------------

        gameOverImage.gameObject.SetActive(true);

        gameOverImage.sprite = smallSprite;

        gameOverRect.localScale =
            Vector3.one * startScale;

        float timer = 0f;

        while (timer < appearDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / appearDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            gameOverRect.localScale =
                Vector3.Lerp(
                    Vector3.one * startScale,
                    Vector3.one * normalScale,
                    t
                );

            yield return null;
        }


        // --------------------------------
        // áA Ç≥ÇÁÇ…ñcÇÁÇﬁ
        // --------------------------------

        timer = 0f;

        while (timer < inflateDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / inflateDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            gameOverImage.sprite = smallSprite;

            gameOverRect.localScale =
                Vector3.Lerp(
                    Vector3.one * normalScale,
                    Vector3.one * maxScale,
                    t
                );

            yield return null;
        }


        // --------------------------------
        // áB ç≈ëÂÇ‹Ç≈ñcÇÁÇÒÇæèÛë‘
        // --------------------------------

        gameOverImage.sprite = bigSprite;

        yield return new WaitForSecondsRealtime(0.08f);


        // --------------------------------
        // áC ÉpÉìÉbÅIÇ∆äÑÇÍÇÈ
        // --------------------------------

        gameOverImage.sprite = burstSprite;

        gameOverRect.localScale =
            Vector3.one * 1.15f;

        yield return new WaitForSecondsRealtime(
            burstDuration
        );


        // --------------------------------
        // áD îjï–Ç…Ç»ÇÈ
        // --------------------------------

        gameOverImage.sprite = particlesSprite;

        gameOverRect.localScale =
            Vector3.one;

        yield return new WaitForSecondsRealtime(
            particleDuration
        );


        // --------------------------------
        // áE GAME OVERè¡Ç¶ÇÈ
        // --------------------------------

        gameOverImage.gameObject.SetActive(false);


        // --------------------------------
        // áF RetryìoèÍ
        // --------------------------------

        yield return new WaitForSecondsRealtime(
            retryDelay
        );

        if (retryButton != null)
        {
            retryButton.SetActive(true);

            RectTransform retryRect =
                retryButton.GetComponent<RectTransform>();

            retryRect.localScale =
                Vector3.zero;

            timer = 0f;

            while (timer < 0.3f)
            {
                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(timer / 0.3f);

                t = Mathf.SmoothStep(0f, 1f, t);

                retryRect.localScale =
                    Vector3.Lerp(
                        Vector3.zero,
                        Vector3.one,
                        t
                    );

                yield return null;
            }

            retryRect.localScale =
                Vector3.one;
        }
    }
}