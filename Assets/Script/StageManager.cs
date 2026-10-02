using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    [Header("ƒNƒŠƒAUI")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TMP_Text starText;

    [Header("“G‚ÌŒ‚”j”")]
    [SerializeField] private int defeatedEnemies = 0;

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
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (enemies.Length == 0 && !stageCleared)
        {
            StageClear();
        }
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
}