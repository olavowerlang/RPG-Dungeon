using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central audio system. Persists across scenes.
/// Creates its own AudioSources — do NOT add AudioSource manually to this object.
/// Set every clip AND its volume slider right here in the Inspector.
/// All buttons in every scene are auto-wired to the UI click sound — no setup needed per button.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Music ─────────────────────────────────────────────────────────────────

    [Header("Music")]
    [SerializeField] private AudioClip            startAreaMusic;
    [SerializeField, Range(0f,1f)] private float  startAreaVolume  = 0.4f;

    [SerializeField] private AudioClip            combatAreaMusic;
    [SerializeField, Range(0f,1f)] private float  combatAreaVolume = 0.4f;

    [SerializeField] private AudioClip            shopAreaMusic;
    [SerializeField, Range(0f,1f)] private float  shopAreaVolume   = 0.35f;

    [SerializeField] private AudioClip            bossMusic;
    [SerializeField, Range(0f,1f)] private float  bossMusicVolume  = 0.5f;

    [SerializeField] private float musicFadeTime = 0.6f;

    // ── UI SFX ────────────────────────────────────────────────────────────────

    [Header("UI")]
    [SerializeField] private AudioClip            uiClickClip;
    [SerializeField, Range(0f,1f)] private float  uiClickVolume        = 0.7f;

    [SerializeField] private AudioClip            fusionClip;
    [SerializeField, Range(0f,1f)] private float  fusionVolume         = 1f;

    [SerializeField] private AudioClip            gameStartClip;
    [SerializeField, Range(0f,1f)] private float  gameStartVolume       = 1f;

    [SerializeField] private AudioClip            dialogueOpenClip;
    [SerializeField, Range(0f,1f)] private float  dialogueOpenVolume    = 0.6f;

    [SerializeField] private AudioClip            dialogueAdvanceClip;
    [SerializeField, Range(0f,1f)] private float  dialogueAdvanceVolume = 0.5f;

    [SerializeField] private AudioClip            consumableClip;
    [SerializeField, Range(0f,1f)] private float  consumableVolume      = 0.9f;

    [SerializeField] private AudioClip            coinRewardClip;
    [SerializeField, Range(0f,1f)] private float  coinRewardVolume      = 0.8f;

    [SerializeField] private AudioClip            shopBuyClip;
    [SerializeField, Range(0f,1f)] private float  shopBuyVolume         = 0.8f;

    [SerializeField] private AudioClip            dashUnlockClip;
    [SerializeField, Range(0f,1f)] private float  dashUnlockVolume      = 1f;

    [SerializeField] private AudioClip            shopErrorClip;
    [SerializeField, Range(0f,1f)] private float  shopErrorVolume       = 0.8f;

    // ── Player SFX ────────────────────────────────────────────────────────────

    [Header("Player")]
    [SerializeField] private AudioClip            playerAttackClip;
    [SerializeField] private AudioClip            playerAttackClip2;
    [SerializeField, Range(0f,1f)] private float  playerAttackVolume = 0.5f;

    [SerializeField] private AudioClip            playerTakeDmgClip;
    [SerializeField, Range(0f,1f)] private float  playerTakeDmgVolume = 0.8f;

    [SerializeField] private AudioClip            playerDashClip;
    [SerializeField, Range(0f,1f)] private float  playerDashVolume   = 0.6f;

    [SerializeField] private AudioClip            grabItemClip;
    [SerializeField, Range(0f,1f)] private float  grabItemVolume     = 0.8f;

    [SerializeField] private AudioClip            hitImpactClip;
    [SerializeField] private AudioClip            hitImpactClip2;
    [SerializeField, Range(0f,1f)] private float  hitImpactVolume    = 0.8f;

    // ── Slime SFX ─────────────────────────────────────────────────────────────

    [Header("Slime")]
    [SerializeField] private AudioClip            slimeJumpClip;
    [SerializeField, Range(0f,1f)] private float  slimeJumpVolume  = 0.7f;

    [SerializeField] private AudioClip            slimeDeathClip;
    [SerializeField, Range(0f,1f)] private float  slimeDeathVolume = 1f;

    // ── Skeleton SFX ──────────────────────────────────────────────────────────

    [Header("Skeleton Fighter")]
    [SerializeField] private AudioClip            skeletonAttackClip;
    [SerializeField, Range(0f,1f)] private float  skeletonAttackVolume = 1f;

    [SerializeField] private AudioClip            skeletonDeathClip1;
    [SerializeField] private AudioClip            skeletonDeathClip2;
    [SerializeField] private AudioClip            skeletonDeathClip3;
    [SerializeField] private AudioClip            skeletonDeathClip4;
    [SerializeField, Range(0f,1f)] private float  skeletonDeathVolume  = 1f;

    [SerializeField] private AudioClip            skeletonIdleClip1;
    [SerializeField] private AudioClip            skeletonIdleClip2;
    [SerializeField] private AudioClip            skeletonIdleClip3;
    [SerializeField, Range(0f,1f)] private float  skeletonIdleVolume   = 0.35f;

    // ── Bombshroom SFX ────────────────────────────────────────────────────────

    [Header("Bombshroom")]
    [SerializeField] private AudioClip            shroomRevealClip;
    [SerializeField, Range(0f,1f)] private float  shroomRevealVolume = 1f;

    [SerializeField] private AudioClip            shroomGasClip;
    [SerializeField, Range(0f,1f)] private float  shroomGasVolume    = 1f;

    [SerializeField] private AudioClip            shroomDeathClip;
    [SerializeField, Range(0f,1f)] private float  shroomDeathVolume  = 1f;

    [SerializeField] private AudioClip            shroomIdleClip;
    [SerializeField, Range(0f,1f)] private float  shroomIdleVolume   = 0.35f;

    // ── Boss / Clone SFX ──────────────────────────────────────────────────────

    [Header("Boss / Clone")]
    [SerializeField] private AudioClip            bossBarRevealClip;
    [SerializeField, Range(0f,1f)] private float  bossBarRevealVolume  = 1f;

    [SerializeField] private AudioClip            bossLetterBoomClip;
    [SerializeField, Range(0f,1f)] private float  bossLetterBoomVolume = 1f;

    [SerializeField] private AudioClip            bossTransformClip;
    [SerializeField, Range(0f,1f)] private float  bossTransformVolume = 1f;

    [SerializeField] private AudioClip            bossRevealClip;
    [SerializeField, Range(0f,1f)] private float  bossRevealVolume    = 1f;

    [SerializeField] private AudioClip            bossPunchClip;
    [SerializeField, Range(0f,1f)] private float  bossPunchVolume     = 0.8f;

    [SerializeField] private AudioClip            bossDeathClip;
    [SerializeField, Range(0f,1f)] private float  bossDeathVolume     = 1f;

    // ── Internal ──────────────────────────────────────────────────────────────

    // ── Ambient ───────────────────────────────────────────────────────────────

    [Header("Ambient")]
    [SerializeField] private AudioClip            startAreaAmbientClip;
    [SerializeField, Range(0f,1f)] private float  startAreaAmbientVolume = 0.3f;

    [SerializeField] private AudioClip            caveEntranceClip;
    [SerializeField, Range(0f,1f)] private float  caveEntranceVolume     = 0.8f;

    private AudioSource _music;
    private AudioSource _ambient;
    private AudioSource _sfx;
    private AudioSource _attackSource;
    private AudioSource _dashSource;
    private AudioSource _shopBuySource;
    private AudioSource _shopErrorSource;
    private Coroutine   _fadeRoutine;

    // Simultaneous-sound limiters (same pattern as EnemyAmbientSound)
    private const int MaxSimultaneousJumps = 2;
    private int _slimeJumpsActive;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _music             = gameObject.AddComponent<AudioSource>();
        _music.loop        = true;
        _music.playOnAwake = false;

        _ambient             = gameObject.AddComponent<AudioSource>();
        _ambient.loop        = true;
        _ambient.playOnAwake = false;

        _sfx             = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;

        _attackSource             = gameObject.AddComponent<AudioSource>();
        _attackSource.playOnAwake = false;

        _dashSource             = gameObject.AddComponent<AudioSource>();
        _dashSource.playOnAwake = false;

        _shopBuySource             = gameObject.AddComponent<AudioSource>();
        _shopBuySource.playOnAwake = false;

        _shopErrorSource             = gameObject.AddComponent<AudioSource>();
        _shopErrorSource.playOnAwake = false;
    }

    private void Start()
    {
        WireAllButtons();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayStartAreaAmbient();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WireAllButtons();
        EnemyAmbientSound.ResetCounters();
        StopMusic();
        _ambient.Stop();
        _ambient.clip = null;
        PlayStartAreaAmbient();
    }

    private void WireAllButtons()
    {
        foreach (var btn in FindObjectsOfType<Button>(true))
        {
            btn.onClick.RemoveListener(PlayUIClick);
            btn.onClick.AddListener(PlayUIClick);
        }
    }

    // ── Generic SFX ───────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _sfx.PlayOneShot(clip, volume);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    public void PlayUIClick()         => PlaySFX(uiClickClip,        uiClickVolume);
    public void PlayFusion()          => PlaySFX(fusionClip,          fusionVolume);
    public void PlayGameStart()        => PlaySFX(gameStartClip,       gameStartVolume);
    public float GameStartClipLength   => gameStartClip != null ? gameStartClip.length : 0f;
    public void PlayDialogueOpen()    => PlaySFX(dialogueOpenClip,    dialogueOpenVolume);
    public void PlayDialogueAdvance() => PlaySFX(dialogueAdvanceClip, dialogueAdvanceVolume);

    public void PlayConsumable()  => PlaySFX(consumableClip,  consumableVolume);
    public void PlayCoinReward()  => PlaySFX(coinRewardClip,  coinRewardVolume);
    public void PlayDashUnlock()  => PlaySFX(dashUnlockClip,  dashUnlockVolume);

    public void PlayShopBuy()
    {
        if (shopBuyClip == null) return;
        _shopBuySource.clip   = shopBuyClip;
        _shopBuySource.volume = shopBuyVolume;
        _shopBuySource.Play();
    }

    public void PlayShopError()
    {
        if (shopErrorClip == null) return;
        _shopErrorSource.clip   = shopErrorClip;
        _shopErrorSource.volume = shopErrorVolume;
        _shopErrorSource.Play();
    }

    // ── Player ────────────────────────────────────────────────────────────────

    public void PlayPlayerAttack()
    {
        var clip = (playerAttackClip2 != null && Random.value > 0.5f) ? playerAttackClip2 : playerAttackClip;
        if (clip == null) return;
        _attackSource.clip   = clip;
        _attackSource.volume = playerAttackVolume;
        _attackSource.Play();
    }

    public void PlayPlayerDash()
    {
        if (playerDashClip == null) return;
        _dashSource.clip   = playerDashClip;
        _dashSource.volume = playerDashVolume;
        _dashSource.Play();
    }

    public void PlayGrabItem()  => PlaySFX(grabItemClip, grabItemVolume);
    public void PlayPlayerTakeDmg() => PlaySFX(playerTakeDmgClip, playerTakeDmgVolume);

    public void PlayHitImpact()
    {
        var clip = (hitImpactClip2 != null && Random.value > 0.5f) ? hitImpactClip2 : hitImpactClip;
        PlaySFX(clip, hitImpactVolume);
    }

    // ── Slime ─────────────────────────────────────────────────────────────────

    public void PlaySlimeJump()
    {
        if (slimeJumpClip == null) return;
        if (_slimeJumpsActive >= MaxSimultaneousJumps) return;
        _slimeJumpsActive++;
        _sfx.PlayOneShot(slimeJumpClip, slimeJumpVolume);
        StartCoroutine(ReleaseJumpSlot(slimeJumpClip.length));
    }

    private IEnumerator ReleaseJumpSlot(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        _slimeJumpsActive = Mathf.Max(0, _slimeJumpsActive - 1);
    }
    public void PlaySlimeDeath() => PlaySFX(slimeDeathClip, slimeDeathVolume);

    // ── Skeleton ──────────────────────────────────────────────────────────────

    public void PlaySkeletonAttack() => PlaySFX(skeletonAttackClip, skeletonAttackVolume);

    public void PlaySkeletonDeath()
    {
        AudioClip[] clips = { skeletonDeathClip1, skeletonDeathClip2, skeletonDeathClip3, skeletonDeathClip4 };
        PlaySFX(PickRandom(clips), skeletonDeathVolume);
    }

    public void PlaySkeletonIdle()
    {
        AudioClip[] clips = { skeletonIdleClip1, skeletonIdleClip2, skeletonIdleClip3 };
        PlaySFX(PickRandom(clips), skeletonIdleVolume);
    }

    private AudioClip PickRandom(AudioClip[] clips)
    {
        // Filter out nulls so missing clips don't break anything
        var valid = System.Array.FindAll(clips, c => c != null);
        if (valid.Length == 0) return null;
        return valid[Random.Range(0, valid.Length)];
    }

    // ── Shroom ────────────────────────────────────────────────────────────────

    public void PlayShroomReveal() => PlaySFX(shroomRevealClip, shroomRevealVolume);
    public void PlayShroomGas()    => PlaySFX(shroomGasClip,    shroomGasVolume);
    public void PlayShroomDeath()  => PlaySFX(shroomDeathClip,  shroomDeathVolume);
    public void PlayShroomIdle()   => PlaySFX(shroomIdleClip,   shroomIdleVolume);

    // ── Boss ──────────────────────────────────────────────────────────────────

    public void PlayBossBarReveal()  => PlaySFX(bossBarRevealClip,  bossBarRevealVolume);
    public void PlayBossLetterBoom() => PlaySFX(bossLetterBoomClip, bossLetterBoomVolume);
    public void PlayBossTransform()  => PlaySFX(bossTransformClip,  bossTransformVolume);
    public void PlayBossReveal()    => PlaySFX(bossRevealClip,    bossRevealVolume);
    public void PlayBossPunch()     => PlaySFX(bossPunchClip,     bossPunchVolume);
    public void PlayBossDeath()     => PlaySFX(bossDeathClip,     bossDeathVolume);

    // ── Music ─────────────────────────────────────────────────────────────────

    public void PlayStartAreaAmbient()
    {
        if (startAreaAmbientClip == null || _ambient.clip == startAreaAmbientClip) return;
        _ambient.clip   = startAreaAmbientClip;
        _ambient.volume = startAreaAmbientVolume;
        _ambient.Play();
    }

    public void PlayCaveEntrance() => PlaySFX(caveEntranceClip, caveEntranceVolume);

    public void StopAmbient()
    {
        _ambient.Stop();
        _ambient.clip = null;
    }

    public void PlayStartAreaMusic() => SwitchMusic(startAreaMusic, startAreaVolume);
    public void PlayCombatMusic()    => SwitchMusic(combatAreaMusic, combatAreaVolume);
    public void PlayShopMusic()      => SwitchMusic(shopAreaMusic,   shopAreaVolume);
    public void PlayBossMusic()      => SwitchMusic(bossMusic,       bossMusicVolume);
    public void StopMusic()          => SwitchMusic(null, 0f);

    private void SwitchMusic(AudioClip clip, float targetVolume)
    {
        if (_music.clip == clip) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeSwitch(clip, targetVolume));
    }

    private IEnumerator FadeSwitch(AudioClip clip, float targetVolume)
    {
        float startVol = _music.volume;
        for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
        {
            _music.volume = Mathf.Lerp(startVol, 0f, t / musicFadeTime);
            yield return null;
        }
        _music.Stop();
        _music.volume = 0f;

        if (clip == null) { _music.clip = null; yield break; }

        _music.clip = clip;
        _music.Play();

        for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
        {
            _music.volume = Mathf.Lerp(0f, targetVolume, t / musicFadeTime);
            yield return null;
        }
        _music.volume = targetVolume;
    }
}
