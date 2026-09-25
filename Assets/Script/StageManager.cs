using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TMP_Text resultScoreText;
    [SerializeField] private TMP_Text resultComboText;
    [SerializeField] private TMP_Text starText;

    [Header("★評価")]
    [SerializeField] private int threeStarScore = 1000;
    [SerializeField] private int twoStarScore = 500;

    private bool stageCleared = false;

    private void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    private void Update()
    {
        CheckStageClear();
    }

    private void CheckStageClear()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (enemies.Length == 0 && !stageCleared)
        {
            StageClear();
        }
    }

    private void StageClear()
    {
        stageCleared = true;

        Debug.Log("ステージクリア！");

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        // ScoreManagerから現在のスコアを取得
        ScoreManager scoreManager =
            FindObjectOfType<ScoreManager>();

        int finalScore = 0;

        if (scoreManager != null)
        {
            finalScore = scoreManager.GetScore();

            if (resultScoreText != null)
            {
                resultScoreText.text =
                    "SCORE : " + finalScore;
            }
        }

        // ComboManagerから最終コンボを取得
        ComboManager comboManager =
            FindObjectOfType<ComboManager>();

        if (comboManager != null)
        {
            int finalCombo = comboManager.GetComboCount();

            if (resultComboText != null)
            {
                resultComboText.text =
                    "COMBO : " + finalCombo;
            }
        }

        // ★評価
        if (starText != null)
        {
            if (finalScore >= threeStarScore)
            {
                starText.text = "★★★";
            }
            else if (finalScore >= twoStarScore)
            {
                starText.text = "★★";
            }
            else
            {
                starText.text = "★";
            }
        }
    }
    public void RetryStage()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
}