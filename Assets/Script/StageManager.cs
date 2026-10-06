using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    [Header("ƒNƒŠƒAUI")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TMP_Text starText;

    [Header("ƒXƒe[ƒW‚Ì“G")]
    [SerializeField] private int totalEnemies = 0;

    [Header("“G‚ÌŒ‚”j”")]
    [SerializeField] private int defeatedEnemies = 0;
    private int spawnedEnemies = 0;

    [Header("š•]‰¿")]
    [SerializeField] private int threeStarKills = 8;
    [SerializeField] private int twoStarKills = 5;

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
        if (stageCleared)
        {
            return;
        }

        // ‚Ü‚¾—\’è”‚Ì“G‚ğo‚µ‚Ä‚¢‚È‚¢
        if (spawnedEnemies < totalEnemies)
        {
            return;
        }

        // ‚Ü‚¾Enemy‚ªc‚Á‚Ä‚¢‚é
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (enemies.Length > 0)
        {
            return;
        }

        StageClear();
    }

    private void StageClear()
    {
        stageCleared = true;

        Debug.Log("ƒXƒe[ƒWƒNƒŠƒAI");
        Debug.Log("ÅIŒ‚”j” : " + defeatedEnemies);

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        // š•]‰¿
        if (starText != null)
        {
            if (defeatedEnemies >= threeStarKills)
            {
                starText.text = "ššš";
            }
            else if (defeatedEnemies >= twoStarKills)
            {
                starText.text = "šš";
            }
            else
            {
                starText.text = "š";
            }
        }
    }
    public void RetryStage()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
    public void AddDefeatedEnemy()
    {
        defeatedEnemies++;

        Debug.Log("“GŒ‚”j” : " + defeatedEnemies);
    }
    public void AddSpawnedEnemy()
    {
        spawnedEnemies++;

        Debug.Log(
            "“GoŒ»” : " +
            spawnedEnemies +
            " / " +
            totalEnemies
        );
    }
    public void SetTotalEnemies(int amount)
{
    totalEnemies = amount;

    Debug.Log(
        "ƒXƒe[ƒW“G‘” : " +
        totalEnemies
    );
}
}