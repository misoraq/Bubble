
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunSkinUI : MonoBehaviour
{
    [Header("ショップ画面")]
    [SerializeField] private GameObject gunSkinPanel;

    [Header("スキン詳細")]
    [SerializeField] private TMP_Text skinNameText;
    [SerializeField] private Image skinImage;
    [SerializeField] private TMP_Text unlockConditionText;

    [Header("スキン画像")]
    [SerializeField] private Sprite normalGunSprite;
    [SerializeField] private Sprite sharkGunSprite;
    [SerializeField] private Sprite bowGunSprite;
    [Header("装備ボタン")]
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;

    // 現在選択しているスキン
    private int selectedSkinId = GunSkinManager.NormalGun;
    private void Start()
    {
        if (gunSkinPanel != null)
        {
            gunSkinPanel.SetActive(false);
        }
    }

    public void OpenGunSkinPanel()
    {
        if (gunSkinPanel != null)
        {
            gunSkinPanel.SetActive(true);
        }

        SelectNormalGun();
    }

    public void CloseGunSkinPanel()
    {
        if (gunSkinPanel != null)
        {
            gunSkinPanel.SetActive(false);
        }
    }

    public void SelectNormalGun()
    {
        ShowSkin(GunSkinManager.NormalGun);
    }

    public void SelectSharkGun()
    {
        ShowSkin(GunSkinManager.SharkGun);
    }

    public void SelectBowGun()
    {
        ShowSkin(GunSkinManager.BowGun);
    }

    private void ShowSkin(int skinId)
    {
        selectedSkinId = skinId;
        if (skinNameText == null ||
            skinImage == null ||
            unlockConditionText == null)
        {
            Debug.LogWarning("スキン詳細UIが未設定です");
            return;
        }

        switch (skinId)
        {
            case GunSkinManager.NormalGun:
                skinNameText.text = "NORMAL GUN";
                skinImage.sprite = normalGunSprite;
                break;

            case GunSkinManager.SharkGun:
                skinNameText.text = "SHARK GUN";
                skinImage.sprite = sharkGunSprite;
                break;

            case GunSkinManager.BowGun:
                skinNameText.text = "BOW GUN";
                skinImage.sprite = bowGunSprite;
                break;
        }

        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null)
        {
            unlockConditionText.text = "スキン管理が見つかりません";
            return;
        }

        if (manager.GetEquippedSkin() == skinId)
        {
            unlockConditionText.text = "装備中";
        }
        else if (manager.IsUnlocked(skinId))
        {
            unlockConditionText.text = "解放済み";
        }
        else if (skinId == GunSkinManager.SharkGun)
        {
            unlockConditionText.text = "★★で解放";
        }
        else
        {
            unlockConditionText.text = "★★★で解放";
        }

        // 画像の縦横比を維持
        skinImage.preserveAspect = true;
        skinImage.enabled = skinImage.sprite != null;
        UpdateEquipButton();
    }

    private void UpdateEquipButton()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null || equipButton == null || equipButtonText == null)
            return;

        if (manager.GetEquippedSkin() == selectedSkinId)
        {
            equipButtonText.text = "装備中";
            equipButton.interactable = false;
        }
        else if (manager.IsUnlocked(selectedSkinId))
        {
            equipButtonText.text = "装備する";
            equipButton.interactable = true;
        }
        else
        {
            equipButtonText.text = "未解放";
            equipButton.interactable = false;
        }
    }

    public void EquipSelectedSkin()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null)
            return;

        // 選択中のスキンを装備・保存
        manager.EquipSkin(selectedSkinId);

        // 装備ボタンの表示を更新
        ShowSkin(selectedSkinId);

        Debug.Log("装備したスキンID: " + manager.GetEquippedSkin());
    }


}
