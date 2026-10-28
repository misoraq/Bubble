using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("画面")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject titlePanel;

    [Header("音量スライダー")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("音量表示（任意）")]
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text seValueText;

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.wholeNumbers = false;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (seSlider != null)
        {
            seSlider.minValue = 0f;
            seSlider.maxValue = 1f;
            seSlider.wholeNumbers = false;
            seSlider.onValueChanged.AddListener(OnSEChanged);
        }

        RefreshSliders();
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        if (seSlider != null)
            seSlider.onValueChanged.RemoveListener(OnSEChanged);
    }

    public void OpenOptions()
    {
        RefreshSliders();
        if (titlePanel != null) titlePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (titlePanel != null) titlePanel.SetActive(true);
    }

    private void RefreshSliders()
    {
        GameSoundManager manager = GameSoundManager.Instance;
        float bgm = manager != null ? manager.BGMSetting :
            PlayerPrefs.GetFloat("Bubble_BGMVolume", 1f);
        float se = manager != null ? manager.SESetting :
            PlayerPrefs.GetFloat("Bubble_SEVolume", 1f);

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(Mathf.Clamp01(bgm));
        if (seSlider != null) seSlider.SetValueWithoutNotify(Mathf.Clamp01(se));
        UpdateLabels(bgm, se);
    }

    public void OnBGMChanged(float value)
    {
        if (GameSoundManager.Instance != null)
            GameSoundManager.Instance.SetBGMVolume(value);
        else
        {
            PlayerPrefs.SetFloat("Bubble_BGMVolume", Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
        UpdateLabels(value, seSlider != null ? seSlider.value : 1f);
    }

    public void OnSEChanged(float value)
    {
        if (GameSoundManager.Instance != null)
            GameSoundManager.Instance.SetSEVolume(value);
        else
        {
            PlayerPrefs.SetFloat("Bubble_SEVolume", Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
        UpdateLabels(bgmSlider != null ? bgmSlider.value : 1f, value);
    }

    private void UpdateLabels(float bgm, float se)
    {
        if (bgmValueText != null)
            bgmValueText.text = Mathf.RoundToInt(Mathf.Clamp01(bgm) * 100f) + "%";
        if (seValueText != null)
            seValueText.text = Mathf.RoundToInt(Mathf.Clamp01(se) * 100f) + "%";
    }
}
