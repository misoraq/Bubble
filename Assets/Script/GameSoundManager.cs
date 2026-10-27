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

    private bool warningActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (warningSource == null || warningSource == sfxSource)
            warningSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null || bgmSource == sfxSource || bgmSource == warningSource)
            bgmSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(sfxSource, false);
        ConfigureSource(warningSource, loopWarning);
        ConfigureSource(bgmSource, true);
        bgmSource.volume = bgmVolume;
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
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayMainBGM()
    {
        if (mainBGM == null || bgmSource == null) return;
        bgmSource.clip = mainBGM;
        bgmSource.loop = true;
        bgmSource.volume = warningActive ? bgmVolume * warningBgmVolumeMultiplier : bgmVolume;
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
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * warningBgmVolumeMultiplier;
        if (warningSound == null || warningSource == null) return;
        warningSource.Stop();
        warningSource.clip = warningSound;
        warningSource.loop = loopWarning;
        warningSource.volume = sfxVolume;
        warningSource.Play();
    }

    public void StopWarningSound()
    {
        warningActive = false;
        if (warningSource != null) warningSource.Stop();
        if (bgmSource != null) bgmSource.volume = bgmVolume;
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
