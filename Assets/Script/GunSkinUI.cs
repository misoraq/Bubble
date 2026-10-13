
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
    }
}
