
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // STARTボタンから呼び出す
    public void StartGame()
    {
        // 時間を通常に戻す
        Time.timeScale = 1f;

        // メインシーンへ移動
        SceneManager.LoadScene("MainScene");
    }
}
