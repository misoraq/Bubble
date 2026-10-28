using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleMenuSelection : MonoBehaviour
{
    [Header("メニューボタン（上から順番）")]
    [SerializeField] private Button[] menuButtons;

    [Header("選択中の色")]
    [SerializeField] private Color selectedColor = Color.white;

    [Header("未選択の色")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("メニューを操作できる条件（任意）")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject shopPanel;

    private TMP_Text[] menuTexts;
    private int selectedIndex;

    private void Awake()
    {
        int count = menuButtons != null ? menuButtons.Length : 0;
        menuTexts = new TMP_Text[count];
        for (int i = 0; i < count; i++)
        {
            if (menuButtons[i] == null) continue;
            menuTexts[i] = menuButtons[i].GetComponentInChildren<TMP_Text>(true);

            // 文字がポインター判定を遮らないようにする
            if (menuTexts[i] != null) menuTexts[i].raycastTarget = false;

            // ボタンの選択色はこのスクリプトで管理する
            menuButtons[i].transition = Selectable.Transition.None;

            TitleMenuHoverTrigger trigger = menuButtons[i].GetComponent<TitleMenuHoverTrigger>();
            if (trigger == null) trigger = menuButtons[i].gameObject.AddComponent<TitleMenuHoverTrigger>();
            trigger.Initialize(this, i);
        }
    }

    private void Start()
    {
        SelectMenu(0);
    }

    private void Update()
    {
        if (!CanOperate() || menuButtons == null || menuButtons.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectMenu((selectedIndex + 1) % menuButtons.Length);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectMenu((selectedIndex - 1 + menuButtons.Length) % menuButtons.Length);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Button button = menuButtons[selectedIndex];
            if (button != null && button.IsActive() && button.IsInteractable())
                button.onClick.Invoke();
        }
    }

    private bool CanOperate()
    {
        if (optionsPanel != null && optionsPanel.activeInHierarchy) return false;
        if (shopPanel != null && shopPanel.activeInHierarchy) return false;
        return true;
    }

    public void OnMenuPointerEnter(int index)
    {
        if (!CanOperate()) return;
        SelectMenu(index);
    }

    public void SelectMenu(int index)
    {
        if (menuButtons == null || index < 0 || index >= menuButtons.Length) return;
        selectedIndex = index;
        RefreshColors();
    }

    private void RefreshColors()
    {
        for (int i = 0; i < menuTexts.Length; i++)
        {
            if (menuTexts[i] == null) continue;
            menuTexts[i].color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }
}

// 各ボタンのマウス進入を検出する補助コンポーネント
public class TitleMenuHoverTrigger : MonoBehaviour, IPointerEnterHandler
{
    private TitleMenuSelection owner;
    private int index;

    public void Initialize(TitleMenuSelection selection, int buttonIndex)
    {
        owner = selection;
        index = buttonIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null) owner.OnMenuPointerEnter(index);
    }
}
