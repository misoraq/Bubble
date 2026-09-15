using UnityEngine;
using TMPro;

public class ComboManager : MonoBehaviour
{
    [Header("コンボ")]
    [SerializeField] private float comboTime = 3f;

    [Header("UI")]
    [SerializeField] private TMP_Text comboText;

    private int comboCount = 0;
    private float comboTimer = 0f;

    private void Start()
    {
        UpdateComboUI();
    }

    private void Update()
    {
        if (comboCount > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void AddCombo()
    {
        comboCount++;

        comboTimer = comboTime;

        Debug.Log(
            "COMBO : " + comboCount
        );

        UpdateComboUI();
    }

    private void ResetCombo()
    {
        Debug.Log(
            "コンボ終了！ 最終コンボ : " +
            comboCount
        );

        comboCount = 0;
        comboTimer = 0f;

        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            comboText.text = "COMBO : " + comboCount;
        }
    }

    public int GetComboCount()
    {
        return comboCount;
    }
}