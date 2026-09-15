using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("ÉXÉRÉA")]
    [SerializeField] private int baseScore = 100;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int comboCount)
    {
        int multiplier = Mathf.Max(1, comboCount);

        int addScore = baseScore * multiplier;

        score += addScore;

        Debug.Log(
            "ìGåÇîjÅI " +
            "COMBO : " + comboCount +
            " / SCORE +" + addScore +
            " / TOTAL : " + score
        );

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE : " + score;
        }
    }

    public int GetScore()
    {
        return score;
    }
}