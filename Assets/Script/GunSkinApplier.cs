
using UnityEngine;

public class GunSkinApplier : MonoBehaviour
{
    [Header("銃のSprite Renderer")]
    [SerializeField] private SpriteRenderer gunSpriteRenderer;

    [Header("銃スキン画像")]
    [SerializeField] private Sprite normalGunSprite;
    [SerializeField] private Sprite sharkGunSprite;
    [SerializeField] private Sprite bowGunSprite;
    [Header("スキンごとのサイズ")]
    [SerializeField] private float normalGunScale = 1f;
    [SerializeField] private float sharkGunScale = 0.1f;
    [SerializeField] private float bowGunScale = 0.1f;
    [Header("スキンごとの左右反転")]
    [SerializeField] private bool normalGunFlipX = false;
    [SerializeField] private bool sharkGunFlipX = false;
    [SerializeField] private bool bowGunFlipX = false;
    [Header("泡の発射位置")]
    [SerializeField] private Transform muzzle;

    [SerializeField] private Vector3 normalMuzzlePosition;
    [SerializeField] private Vector3 sharkMuzzlePosition;
    [SerializeField] private Vector3 bowMuzzlePosition;
    private void Start()
    {
        ApplyEquippedSkin();
    }

    private void ApplyEquippedSkin()
    {
        if (gunSpriteRenderer == null)
        {
            Debug.LogWarning("銃のSprite Rendererが未設定です");
            return;
        }

        if (GunSkinManager.Instance == null)
        {
            Debug.LogWarning("GunSkinManagerが見つかりません");
            return;
        }

        int skinId = GunSkinManager.Instance.GetEquippedSkin();




        switch (skinId)
        {
            case GunSkinManager.NormalGun:
                gunSpriteRenderer.sprite = normalGunSprite;
                gunSpriteRenderer.transform.localScale =
                    Vector3.one * normalGunScale;
                gunSpriteRenderer.flipX = normalGunFlipX;

                SetMuzzlePosition(normalMuzzlePosition);
                break;

            case GunSkinManager.SharkGun:
                gunSpriteRenderer.sprite = sharkGunSprite;
                gunSpriteRenderer.transform.localScale =
                    Vector3.one * sharkGunScale;
                gunSpriteRenderer.flipX = sharkGunFlipX;

                SetMuzzlePosition(sharkMuzzlePosition);
                break;

            case GunSkinManager.BowGun:
                gunSpriteRenderer.sprite = bowGunSprite;
                gunSpriteRenderer.transform.localScale =
                    Vector3.one * bowGunScale;
                gunSpriteRenderer.flipX = bowGunFlipX;

                SetMuzzlePosition(bowMuzzlePosition);
                break;
        }



        Debug.Log("ゲーム内に反映した銃スキンID: " + skinId);
    }

    private void SetMuzzlePosition(Vector3 position)
    {
        if (muzzle == null)
        {
            Debug.LogWarning("Muzzleが設定されていません");
            return;
        }

        muzzle.localPosition = position;
    }

}
