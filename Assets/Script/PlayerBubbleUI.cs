using UnityEngine;
using UnityEngine.UI;

public class PlayerBubbleUI : MonoBehaviour
{
    [Header("Playerの泡UI")]
    [SerializeField] private Image[] bubbleImages;

    [Header("Game Over演出")]
    [SerializeField] private GameOverEffect gameOverEffect;

    private int currentLives;

    private void Start()
    {
        currentLives = bubbleImages.Length;
    }

    public void TakeDamage()
    {
        if (currentLives <= 0)
            return;

        currentLives--;

        bubbleImages[currentLives].gameObject.SetActive(false);

        Debug.Log("Playerダメージ！ 残り泡 : " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");

        if (gameOverEffect != null)
        {
            gameOverEffect.StartGameOver();
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}