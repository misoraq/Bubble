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

    [Header("演出時間")]
    [SerializeField] private float darkenDuration = 0.8f;
    [SerializeField] private float shrinkDuration = 0.8f;
    [SerializeField] private float waitDuration = 0.5f;

    [Header("プレイヤー周囲で停止する時間")]
    [SerializeField] private float circleHoldDuration = 1f;

    [Header("完全暗転までの時間")]
    [SerializeField] private float finalDarkenDuration = 0.5f;
    [Header("GAME OVER落下演出")]
    [SerializeField] private GameOverDropUI gameOverDropUI;
    [Header("Game Over UI")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("円の大きさ")]
    [SerializeField] private float startRadius = 0.65f;
    [SerializeField] private float endRadius = 0.15f;

    [Header("ゲームオーバー時に非表示にするUI")]
    [SerializeField] private GameObject jetpackFuelGauge;
    [Header("暗さ")]
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
            // このGameOverEffect専用のMaterialを作る
            effectMaterial = new Material(effectImage.material);

            effectImage.material = effectMaterial;
        }

        // 最初は暗転しない
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_Alpha", 0f);
            effectMaterial.SetFloat("_Radius", startRadius);
        }

        // 最初はGame Over Panelを非表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (player == null || mainCamera == null || effectMaterial == null)
            return;

        // Playerのワールド座標をViewport座標へ変換
        Vector3 screenPosition =
            mainCamera.WorldToViewportPoint(player.position);

        // 円の中心をPlayerに合わせる
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
        GameCursorController.ShowResultCursor();

        if (jetpackFuelGauge != null)
        {
            jetpackFuelGauge.SetActive(false);
        }
        if (isGameOver)
            return;

        isGameOver = true;
      
        // Playerの操作を止める
        StopPlayer();

        // 演出開始
        StartCoroutine(GameOverSequence());
    }

    private void StopPlayer()
    {

        if (player == null)
            return;

        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove != null)
            playerMove.StopForStageEnd();

        WaterGun[] guns = player.GetComponentsInChildren<WaterGun>(true);
        foreach (WaterGun gun in guns)
            gun.StopForStageEnd();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
    }

    private IEnumerator GameOverSequence()
    {
        // -------------------------
        // ① 周囲を徐々に暗くする
        // -------------------------

        float timer = 0f;

        while (timer < darkenDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / darkenDuration);

            // 少し滑らかに
            t = Mathf.SmoothStep(0f, 1f, t);

            effectMaterial.SetFloat(
                "_Alpha",
                Mathf.Lerp(0f, maxAlpha, t)
            );

            yield return null;
        }

        // -------------------------
        // ② 円を徐々に小さくする
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
        // プレイヤーの周囲を映した状態で停止
        yield return new WaitForSecondsRealtime(circleHoldDuration);

        // 円形の視界を完全に閉じる
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

        // 完全に暗転した状態にする
        effectMaterial.SetFloat("_Radius", 0f);
        effectMaterial.SetFloat("_Alpha", 1f);
        // -------------------------
        // ③ 少し停止
        // -------------------------



        // -------------------------
        // ④ GAME OVER表示
        // -------------------------

        yield return new WaitForSecondsRealtime(waitDuration);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Panel内のGameOverImageを表示
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