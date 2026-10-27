
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCursorController : MonoBehaviour
{
    [Header("ゲームプレイ中のカーソル設定")]
    [SerializeField] private bool hideCursor = true;

    // リザルト画面を表示しているか
    private static bool isResultScreen = false;

    private void Start()
    {
        // シーン開始時は通常のカーソル設定に戻す
        isResultScreen = false;
        ApplyCursorSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isResultScreen = false;
        ApplyCursorSettings();
    }

    // クリア・ゲームオーバー時に呼び出す
    public static void ShowResultCursor()
    {
        isResultScreen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // リトライ・ゲームプレイ再開時に呼び出す
    public static void HideResultCursor()
    {
        isResultScreen = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void ApplyCursorSettings()
    {
        Cursor.lockState = CursorLockMode.None;

        if (isResultScreen)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = !hideCursor;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursorSettings();
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
