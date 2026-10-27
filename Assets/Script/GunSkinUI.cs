
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunSkinUI : MonoBehaviour
{
    [Header("ショップ画面")]
    [SerializeField] private GameObject gunSkinPanel;

    [Header("タイトル画面のボタン")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject gunSkinButton;

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
    [SerializeField] private Image pufferFrame;
    [SerializeField] private Image jellyfishFrame;
    [SerializeField] private Image crabFrame;

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
    [Header("未完成スキンの詳細表示")]
    [SerializeField] private GameObject skinQuestionText;

    [Header("追加スキンの鍵マーク")]
    [SerializeField] private GameObject pufferLockImage;
    [SerializeField] private GameObject jellyfishLockImage;
    [SerializeField] private GameObject crabLockImage;

    [Header("追加スキンの進捗テキスト")]
    [SerializeField] private TMP_Text pufferProgressText;
    [SerializeField] private TMP_Text jellyfishProgressText;
    [SerializeField] private TMP_Text crabProgressText;

    [Header("テスト用")]
    [SerializeField] private bool testLockedSkins = false;
    private void Start()
    {
        if (gunSkinPanel != null)
        {
            gunSkinPanel.SetActive(false);
        }

        SetTitleButtonsVisible(true);
    }

    private void SetTitleButtonsVisible(bool visible)
    {
        if (startButton != null) startButton.SetActive(visible);
        if (gunSkinButton != null) gunSkinButton.SetActive(visible);
    }

    public void OpenGunSkinPanel()
    {
        SetTitleButtonsVisible(false);

        if (gunSkinPanel != null)
        {
            gunSkinPanel.SetActive(true);
        }

        SelectNormalGun();
        UpdateAllSkinStatus();
        UpdateLockImages();

        // 追加スキンの累計撃破数と鍵マークを更新
        UpdateAdditionalSkinProgress();
    }

    public void CloseGunSkinPanel()
    {
        SetTitleButtonsVisible(true);

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
        bool isComingSoonSkin =
    skinId == GunSkinManager.PufferGun ||
    skinId == GunSkinManager.JellyfishGun ||
    skinId == GunSkinManager.CrabGun;

        if (skinQuestionText != null)
        {
            skinQuestionText.SetActive(isComingSoonSkin);
        }
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
                int kills = EnemyKillRecords.GetKills("Pufferfish");
                unlockConditionText.text = $"フグを20体撃破\n{Mathf.Min(kills, 20)} / 20";
            }
            else if (skinId == GunSkinManager.PufferGun)
            {
                unlockConditionText.text = "星3で解放";
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

        // 詳細画面の武器画像も未解放なら黒くする
        GunSkinManager skinManager = GunSkinManager.Instance;

        bool isUnlocked = skinManager != null &&
                          skinManager.IsUnlocked(skinId);

        // テスト中はSHARK・BOWを未解放として表示
        if (testLockedSkins &&
            (skinId == GunSkinManager.SharkGun ||
             skinId == GunSkinManager.BowGun))
        {
            isUnlocked = false;
        }

        skinImage.color = isUnlocked
            ? Color.white
            : Color.black;

        if (skinId == GunSkinManager.PufferGun)
        {
            bool conditionMet = manager != null && manager.GetBestStars() >= 3;
            unlockConditionText.text = conditionMet
                ? "条件達成！ 武器は準備中"
                : "星3でクリアすると解放\n未解放";
        }
        else if (skinId == GunSkinManager.JellyfishGun ||
    skinId == GunSkinManager.CrabGun)
        {
            string enemyType = "";
            string enemyName = "";
            int requiredKills = 0;

            switch (skinId)
            {
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
        SetFrame(normalFrame, GunSkinManager.NormalGun);
        SetFrame(sharkFrame, GunSkinManager.SharkGun);
        SetFrame(bowFrame, GunSkinManager.BowGun);

        SetFrame(pufferFrame, GunSkinManager.PufferGun);
        SetFrame(jellyfishFrame, GunSkinManager.JellyfishGun);
        SetFrame(crabFrame, GunSkinManager.CrabGun);
    }


    private void SetFrame(Image frame, int skinId)
    {
        if (frame == null)
            return;

        frame.sprite =
            selectedSkinId == skinId
            ? selectedFrameSprite
            : normalFrameSprite;

        // 元のデザインの色を維持
        frame.color = Color.white;
    }


    private void UpdateLockImages()
    {
        GunSkinManager manager = GunSkinManager.Instance;

        if (manager == null)
            return;

        bool sharkUnlocked =
     !testLockedSkins &&
     manager.IsUnlocked(GunSkinManager.SharkGun);

        bool bowUnlocked =
            !testLockedSkins &&
            manager.IsUnlocked(GunSkinManager.BowGun);

        // 鍵マークの表示
        if (sharkLockImage != null)
            sharkLockImage.SetActive(!sharkUnlocked);

        if (bowLockImage != null)
            bowLockImage.SetActive(!bowUnlocked);

        // 武器画像は常に表示し、未解放なら黒くする
        SetWeaponSilhouette(sharkGunImage, sharkUnlocked);
        SetWeaponSilhouette(bowGunImage, bowUnlocked);
    }

    // 未解放武器を黒いシルエットにする
    private void SetWeaponSilhouette(
        GameObject weaponObject,
        bool isUnlocked)
    {
        if (weaponObject == null)
            return;

        weaponObject.SetActive(true);

        Image weaponImage = weaponObject.GetComponent<Image>();

        if (weaponImage == null)
        {
            Debug.LogWarning(
                weaponObject.name + " にImageがありません。"
            );
            return;
        }

        weaponImage.color = isUnlocked
            ? Color.white
            : Color.black;
    }

    private void UpdateAdditionalSkinProgress()
    {
        GunSkinManager manager = GunSkinManager.Instance;
        bool pufferUnlocked = manager != null && manager.IsUnlocked(GunSkinManager.PufferGun);
        if (pufferLockImage != null)
            pufferLockImage.SetActive(!pufferUnlocked);
        if (pufferProgressText != null)
            pufferProgressText.text = pufferUnlocked ? "条件達成" : "星3で解放";

        UpdateSkinProgress(
            GunSkinManager.JellyfishGun,
            "Jellyfish",
            20,
            jellyfishLockImage,
            jellyfishProgressText
        );

        UpdateSkinProgress(
            GunSkinManager.CrabGun,
            "Crab",
            15,
            crabLockImage,
            crabProgressText
        );
    }

    private void UpdateSkinProgress(
        int skinId,
        string enemyType,
        int requiredKills,
        GameObject lockImage,
        TMP_Text progressText)
    {
        int kills = EnemyKillRecords.GetKills(enemyType);
        bool conditionMet = kills >= requiredKills;

        // 条件未達成のときだけ鍵を表示
        if (lockImage != null)
        {
            lockImage.SetActive(!conditionMet);
        }

        if (progressText != null)
        {
            progressText.text = conditionMet
                ? "条件達成"
                : $"{Mathf.Min(kills, requiredKills)} / {requiredKills}";
        }
    }


}
