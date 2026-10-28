using UnityEngine;

public class GameSoundManager : MonoBehaviour
{
    public static GameSoundManager Instance { get; private set; }

    [Header("効果音の再生元")]
    [SerializeField] private AudioSource sfxSource;

    [Header("効果音（既存の7種類）")]
    [SerializeField] private AudioClip waterGunShoot;
    [SerializeField] private AudioClip enemyHit;
    [SerializeField] private AudioClip enemyDefeat;
    [SerializeField] private AudioClip playerDamage;
    [SerializeField] private AudioClip phaseStart;
    [SerializeField] private AudioClip gameClear;
    [SerializeField] private AudioClip gameOver;

    [Header("追加効果音（3種類）")]
    [SerializeField] private AudioClip enemyTrap;
    [SerializeField] private AudioClip resultScoreCount;
    [SerializeField] private AudioClip perfectText;

    [Header("水切れ警告音")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioSource warningSource;
    [SerializeField] private bool loopWarning = true;

    [Header("メインBGM")]
    [SerializeField] private AudioClip mainBGM;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private bool playBGMOnStart = true;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float warningBgmVolumeMultiplier = 0.35f;

    [Header("音量")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    private const string BgmVolumeKey = "Bubble_BGMVolume";
    private const string SeVolumeKey = "Bubble_SEVolume";

    private float bgmSetting = 1f;
    private float seSetting = 1f;
    private bool warningActive;

    public float BGMSetting => bgmSetting;
    public float SESetting => seSetting;

    public void SetBGMVolume(float value)
    {
        bgmSetting = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmSetting);
        PlayerPrefs.Save();
        ApplyBGMVolume();
    }

    public void SetSEVolume(float value)
    {
        seSetting = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SeVolumeKey, seSetting);
        PlayerPrefs.Save();
        ApplyWarningVolume();
    }

    private void ApplyBGMVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * bgmSetting *
                (warningActive ? warningBgmVolumeMultiplier : 1f);
    }

    private void ApplyWarningVolume()
    {
        if (warningSource != null)
            warningSource.volume = sfxVolume * seSetting;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        bgmSetting = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, 1f));
        seSetting = Mathf.Clamp01(PlayerPrefs.GetFloat(SeVolumeKey, 1f));
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (warningSource == null || warningSource == sfxSource)
            warningSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null || bgmSource == sfxSource || bgmSource == warningSource)
            bgmSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(sfxSource, false);
        ConfigureSource(warningSource, loopWarning);
        ConfigureSource(bgmSource, true);
        ApplyBGMVolume();
        ApplyWarningVolume();
    }

    private void Start()
    {
        if (playBGMOnStart) PlayMainBGM();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ConfigureSource(AudioSource source, bool loop)
    {
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxVolume * seSetting);
    }

    public void PlayMainBGM()
    {
        if (mainBGM == null || bgmSource == null) return;
        bgmSource.clip = mainBGM;
        bgmSource.loop = true;
        ApplyBGMVolume();
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    public void StopMainBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlayWarningSound()
    {
        if (warningActive) return;
        warningActive = true;
        ApplyBGMVolume();
        if (warningSound == null || warningSource == null) return;
        warningSource.Stop();
        warningSource.clip = warningSound;
        warningSource.loop = loopWarning;
        ApplyWarningVolume();
        warningSource.Play();
    }

    public void StopWarningSound()
    {
        warningActive = false;
        if (warningSource != null) warningSource.Stop();
        ApplyBGMVolume();
    }

    public void PlayEnemyTrap() { PlaySound(enemyTrap); }
    public void PlayResultScoreCount() { PlaySound(resultScoreCount); }
    public void PlayPerfectText() { PlaySound(perfectText); }

    public void PlayWaterGunShoot() { PlaySound(waterGunShoot); }
    public void PlayEnemyHit() { PlaySound(enemyHit); }
    public void PlayEnemyDefeat() { PlaySound(enemyDefeat); }
    public void PlayPlayerDamage() { PlaySound(playerDamage); }
    public void PlayPhaseStart() { PlaySound(phaseStart); }

    public void PlayGameClear()
    {
        StopWarningSound();
        StopMainBGM();
        PlaySound(gameClear);
    }

    public void PlayGameOver()
    {
        StopWarningSound();
        StopMainBGM();
        PlaySound(gameOver);
    }
}
