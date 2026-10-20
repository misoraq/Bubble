
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
    [Header("各スキンの装備状態")]
    [SerializeField] private TMP_Text normalStatusText;
    [SerializeField] private TMP_Text sharkStatusText;
    [SerializeField] private TMP_Text bowStatusText;
    [Header("武器選択フレーム")]
    [SerializeField] private Image normalFrame;
    [SerializeField] private Image sharkFrame;
    [SerializeField] private Image bowFrame;

    [SerializeField] private Sprite normalFrameSprite;
    [SerializeField] private Sprite selectedFrameSprite;

    [Header("選択枠の色")]
    [SerializeField] private Color selectedFrameColor = Color.yellow;
    [SerializeField] private Color normalFrameColor = Color.white;
    [Header("未解放カギマーク")]
    [SerializeField] private GameObject sharkLockImage;
    [SerializeField] private GameObject bowLockImage;
    [Header("武器スプライト")]
    [SerializeField] private GameObject sharkGunImage;
    [SerializeField] private GameObject bowGunImage;
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
        UpdateAllSkinStatus();
        UpdateLockImages();
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
    public void SelectPufferGun()
    {
        ShowSkin(GunSkinManager.PufferGun);
    }

    public void SelectJellyfishGun()
    {
        ShowSkin(GunSkinManager.JellyfishGun);
    }

    public void SelectCrabGun()
    {
        ShowSkin(GunSkinManager.CrabGun);
    }

    private void ShowSkin(int skinId)
    {
        selectedSkinId = skinId;
        UpdateSelectedFrame();
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
            case GunSkinManager.PufferGun:
                skinNameText.text = "SKIN 4";
                skinImage.sprite = null;
                break;

            case GunSkinManager.JellyfishGun:
                skinNameText.text = "SKIN 5";
                skinImage.sprite = null;
                break;

            case GunSkinManager.CrabGun:
                skinNameText.text = "SKIN 6";
                skinImage.sprite = null;
                break;
        }

        GunSkinManager manager = GunSkinManager.Instance;

        if (manager != null)
        {
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
                unlockConditionText.text = "星2で解放";
            }
            else if (skinId == GunSkinManager.BowGun)
            {
                unlockConditionText.text = "星3で解放";
            }
            else if (skinId == GunSkinManager.PufferGun)
            {
                int kills = EnemyKillRecords.GetKills("Pufferfish");

                unlockConditionText.text =
                    $"フグを20体撃破\n{Mathf.Min(kills, 20)} / 20";
            }
            else if (skinId == GunSkinManager.JellyfishGun)
            {
                int kills = EnemyKillRecords.GetKills("Jellyfish");

                unlockConditionText.text =
                    $"クラゲを20体撃破\n{Mathf.Min(kills, 20)} / 20";
            }
            else if (skinId == GunSkinManager.CrabGun)
            {
                int kills = EnemyKillRecords.GetKills("Crab");

                unlockConditionText.text =
                    $"カニを15体撃破\n{Mathf.Min(kills, 15)} / 15";
            }
        }

        // 画像の縦横比を維持
        skinImage.preserveAspect = true;
        skinImage.enabled = skinImage.sprite != null;
        if (skinId == GunSkinManager.PufferGun ||
    skinId == GunSkinManager.JellyfishGun ||
    skinId == GunSkinManager.CrabGun)
        {
            string enemyType = "";
            string enemyName = "";
            int requiredKills = 0;

            switch (skinId)
            {
                case GunSkinManager.PufferGun:
                    enemyType = "Pufferfish";
                    enemyName = "フグ";
                    requiredKills = 20;
                    break;

                case GunSkinManager.JellyfishGun:
                    enemyType = "Jellyfish";
                    enemyName = "クラゲ";
                    requiredKills = 20;
                    break;

                case GunSkinManager.CrabGun:
                    enemyType = "Crab";
                    enemyName = "カニ";
                    requiredKills = 15;
                    break;
            }

            int kills = EnemyKillRecords.GetKills(enemyType);

            unlockConditionText.text =
                $"{enemyName}を累計{requiredKills}体撃破\n" +
                $"{Mathf.Min(kills, requiredKills)} / {requiredKills}\n" +
                (kills >= requiredKills
                    ? "条件達成！ 武器は準備中"
                    : "未解放");
        }
        UpdateEquipButton();
    }

    private void UpdateEquipButton()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null || equipButton == null || equipButtonText == null)
            return;
        if (selectedSkinId >= GunSkinManager.PufferGun &&
    selectedSkinId <= GunSkinManager.CrabGun)
        {
            equipButtonText.text = "準備中";
            equipButton.interactable = false;
            return;
        }
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
        UpdateAllSkinStatus();
        Debug.Log("装備したスキンID: " + manager.GetEquippedSkin());
    }
    private void UpdateAllSkinStatus()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null)
            return;

        int equippedSkin = manager.GetEquippedSkin();

        if (normalStatusText != null)
            normalStatusText.text =
                equippedSkin == GunSkinManager.NormalGun ? "装備中" : "未装備";

        if (sharkStatusText != null)
            sharkStatusText.text =
                !manager.IsUnlocked(GunSkinManager.SharkGun) ? "未解放" :
                equippedSkin == GunSkinManager.SharkGun ? "装備中" : "未装備";

        if (bowStatusText != null)
            bowStatusText.text =
                !manager.IsUnlocked(GunSkinManager.BowGun) ? "未解放" :
                equippedSkin == GunSkinManager.BowGun ? "装備中" : "未装備";
    }
    // NORMALを装備
    public void EquipNormalGun()
    {
        EquipGunFromList(GunSkinManager.NormalGun);
    }

    // SHARKを装備
    public void EquipSharkGun()
    {
        EquipGunFromList(GunSkinManager.SharkGun);
    }

    // BOWを装備
    public void EquipBowGun()
    {
        EquipGunFromList(GunSkinManager.BowGun);
    }

    // スキン一覧から装備する共通処理
    private void EquipGunFromList(int skinId)
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning("GunSkinManagerが見つかりません");
            return;
        }

        // 未解放なら装備しない
        if (!manager.IsUnlocked(skinId))
        {
            Debug.Log("このスキンはまだ解放されていません");
            return;
        }

        // 装備して保存
        manager.EquipSkin(skinId);

        // 全スキンの表示を更新
        UpdateAllSkinStatus();

        Debug.Log("装備したスキンID: " + skinId);
    }
    private void UpdateSelectedFrame()
    {
        if (normalFrame != null)
        {
            normalFrame.sprite =
                selectedSkinId == GunSkinManager.NormalGun
                ? selectedFrameSprite : normalFrameSprite;
        }

        if (sharkFrame != null)
        {
            sharkFrame.sprite =
                selectedSkinId == GunSkinManager.SharkGun
                ? selectedFrameSprite : normalFrameSprite;
        }

        if (bowFrame != null)
        {
            bowFrame.sprite =
                selectedSkinId == GunSkinManager.BowGun
                ? selectedFrameSprite : normalFrameSprite;
        }
    }
    private void UpdateLockImages()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null) return;

        bool sharkUnlocked = manager.IsUnlocked(GunSkinManager.SharkGun);
        bool bowUnlocked = manager.IsUnlocked(GunSkinManager.BowGun);

        // カギマーク
        if (sharkLockImage != null)
            sharkLockImage.SetActive(!sharkUnlocked);

        if (bowLockImage != null)
            bowLockImage.SetActive(!bowUnlocked);

        // 武器スプライト
        if (sharkGunImage != null)
            sharkGunImage.SetActive(sharkUnlocked);

        if (bowGunImage != null)
            bowGunImage.SetActive(bowUnlocked);
    }
}
