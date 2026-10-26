
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCursorController : MonoBehaviour
{
    [Header("ゲームプレイ中のカーソル設定")]
    [SerializeField] private bool hideCursor = true;

    private void Start()
    {
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
        ApplyCursorSettings();
    }

    private void ApplyCursorSettings()
    {
        Cursor.visible = !hideCursor;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursorSettings();
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
