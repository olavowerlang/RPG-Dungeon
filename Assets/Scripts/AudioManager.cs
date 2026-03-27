using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private AudioClip            combatAreaMusic;
    [SerializeField, Range(0f,1f)] private float  combatAreaVolume = 0.4f;

    [SerializeField] private AudioClip            shopAreaMusic;
    [SerializeField, Range(0f,1f)] private float  shopAreaVolume   = 0.35f;

    [SerializeField] private AudioClip            bossMusic;
    [SerializeField, Range(0f,1f)] private float  bossMusicVolume  = 0.5f;

    [SerializeField] private float musicFadeTime = 0.6f;

    // ── UI SFX ────────────────────────────────────────────────────────────────

    [Header("UI")]
    [SerializeField] private AudioClip            genericClickClip;
    [SerializeField, Range(0f,1f)] private float  genericClickVolume   = 0.7f;

    [SerializeField] private AudioClip            fusionClip;
    [SerializeField, Range(0f,1f)] private float  fusionVolume         = 1f;

    [SerializeField] private AudioClip            gameOverClip;
    [SerializeField, Range(0f,1f)] private float  gameOverVolume        = 1f;

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
    [SerializeField, Range(0f,1f)] private float  playerAttackVolume  = 0.5f;

    [SerializeField] private AudioClip            playerAttackClip2;
    [SerializeField, Range(0f,1f)] private float  playerAttackVolume2 = 0.5f;

    [SerializeField] private AudioClip            playerTakeDmgClip;
    [SerializeField, Range(0f,1f)] private float  playerTakeDmgVolume = 0.8f;

    [SerializeField] private AudioClip            playerDashClip;
    [SerializeField, Range(0f,1f)] private float  playerDashVolume   = 0.6f;

    [SerializeField] private AudioClip            grabItemClip;
    [SerializeField, Range(0f,1f)] private float  grabItemVolume     = 0.8f;

    [SerializeField] private AudioClip            hitImpactClip;
    [SerializeField, Range(0f,1f)] private float  hitImpactVolume    = 0.8f;

    // ── Slime SFX ─────────────────────────────────────────────────────────────

    [Header("Slime")]
    [SerializeField] private AudioClip            slimeJumpClip;
    [SerializeField, Range(0f,1f)] private float  slimeJumpVolume      = 1f;
    [SerializeField, Range(0f,1f)] private float  miniSlimeJumpVolume  = 1f;

    [SerializeField] private AudioClip            slimeDeathClip;
    [SerializeField, Range(0f,1f)] private float  slimeDeathVolume     = 1f;
    [SerializeField, Range(0f,1f)] private float  miniSlimeDeathVolume = 1f;

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
    [SerializeField, Range(0f,1f)] private float  shroomGasVolume       = 1f;

    [SerializeField] private AudioClip            shroomDeathGasClip;
    [SerializeField, Range(0f,1f)] private float  shroomDeathGasVolume  = 1f;

    [SerializeField] private AudioClip            shroomDeathClip;
    [SerializeField, Range(0f,1f)] private float  shroomDeathVolume     = 1f;

    [SerializeField] private AudioClip            shroomIdleClip;
    [SerializeField, Range(0f,1f)] private float  shroomIdleVolume   = 0.35f;

    // ── Boss / Clone SFX ──────────────────────────────────────────────────────

    [Header("Boss / Clone")]
    [SerializeField] private AudioClip            playerCloneDeathClip;
    [SerializeField, Range(0f,1f)] private float  playerCloneDeathVolume = 1f;

    [SerializeField] private AudioClip            bossLetterBoomClip;
    [SerializeField, Range(0f,1f)] private float  bossLetterBoomVolume = 1f;

    [SerializeField] private AudioClip            bossTransformClip;
    [SerializeField, Range(0f,1f)] private float  bossTransformVolume = 1f;

    [SerializeField] private AudioClip            bossBarFillClip;
    [SerializeField, Range(0f,1f)] private float  bossBarFillVolume   = 1f;

    // ── Internal ──────────────────────────────────────────────────────────────

    // ── Ambient ───────────────────────────────────────────────────────────────

    [Header("Ambient")]
    [SerializeField] private AudioClip            startAreaAmbientClip;
    [SerializeField, Range(0f,1f)] private float  startAreaAmbientVolume = 0.3f;

    [SerializeField] private AudioClip            leavesRustlingClip;
    [SerializeField, Range(0f,1f)] private float  leavesRustlingVolume   = 0.25f;

    [SerializeField] private AudioClip            caveEntranceClip;
    [SerializeField, Range(0f,1f)] private float  caveEntranceVolume  = 0.8f;

    [SerializeField] private AudioClip            caveAmbientClip;
    [SerializeField, Range(0f,1f)] private float  caveAmbientVolume   = 0.3f;

    private AudioSource _music;
    private AudioSource _ambient;
    private AudioSource _ambient2;   // second zone-1 loop (leaves rustling)
    private AudioSource _sfx;
    private AudioSource _attackSource;
    private AudioSource _dashSource;
    private Coroutine   _fadeRoutine;
    private bool        _attackToggle;

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

        _ambient2             = gameObject.AddComponent<AudioSource>();
        _ambient2.loop        = true;
        _ambient2.playOnAwake = false;

        _sfx             = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;

        _attackSource             = gameObject.AddComponent<AudioSource>();
        _attackSource.playOnAwake = false;

        _dashSource             = gameObject.AddComponent<AudioSource>();
        _dashSource.playOnAwake = false;

    }

    // Called by Unity whenever a value changes in the Inspector (Edit AND Play mode)
    private void OnValidate()
    {
        if (_ambient  != null) _ambient.volume  = _ambient.clip == caveAmbientClip ? caveAmbientVolume : startAreaAmbientVolume;
        if (_ambient2 != null) _ambient2.volume = leavesRustlingVolume;

        if (_music != null)
        {
            if      (_music.clip == combatAreaMusic) _music.volume = combatAreaVolume;
            else if (_music.clip == shopAreaMusic)   _music.volume = shopAreaVolume;
            else if (_music.clip == bossMusic)       _music.volume = bossMusicVolume;
        }

    }


    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Preload all music clips so the first zone cross never causes a frame hitch
        foreach (var clip in new[] { combatAreaMusic, shopAreaMusic, bossMusic })
            if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();

        PlayStartAreaAmbient();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnemyAmbientSound.ResetCounters();
        StopMusic();
        _ambient.Stop();
        _ambient.clip = null;
        _ambient2.Stop();
        _ambient2.clip = null;
        // SceneAudioStarter in each scene handles playing the correct audio
    }

    // ── Generic SFX ───────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _sfx.PlayOneShot(clip, volume);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    public void PlayGenericClick()    => PlaySFX(genericClickClip,    genericClickVolume);
    public void PlayGameOver()        => PlaySFX(gameOverClip,        gameOverVolume);
    public void PlayFusion()          => PlaySFX(fusionClip,          fusionVolume);
    public void PlayGameStart() => PlaySFX(gameStartClip, gameStartVolume);
    public void PlayDialogueOpen()    => PlaySFX(dialogueOpenClip,    dialogueOpenVolume);
    public void PlayDialogueAdvance() => PlaySFX(dialogueAdvanceClip, dialogueAdvanceVolume);

    public void PlayConsumable()  => PlaySFX(consumableClip,  consumableVolume);
    public void PlayCoinReward()  => PlaySFX(coinRewardClip,  coinRewardVolume);
    public void PlayDashUnlock()  => PlaySFX(dashUnlockClip,  dashUnlockVolume);

    public void PlayShopBuy()   => PlaySFX(shopBuyClip,   shopBuyVolume);
    public void PlayShopError() => PlaySFX(shopErrorClip, shopErrorVolume);

    // ── Player ────────────────────────────────────────────────────────────────

    public void PlayPlayerAttack()
    {
        _attackToggle = !_attackToggle;
        bool useSecond = _attackToggle && playerAttackClip2 != null;
        _attackSource.clip   = useSecond ? playerAttackClip2 : playerAttackClip;
        _attackSource.volume = useSecond ? playerAttackVolume2 : playerAttackVolume;
        if (_attackSource.clip == null) return;
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

    public void PlayHitImpact() => PlaySFX(hitImpactClip, hitImpactVolume);

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
    public void PlayMiniSlimeJump()
    {
        if (slimeJumpClip == null) return;
        if (_slimeJumpsActive >= MaxSimultaneousJumps) return;
        _slimeJumpsActive++;
        _sfx.PlayOneShot(slimeJumpClip, miniSlimeJumpVolume);
        StartCoroutine(ReleaseJumpSlot(slimeJumpClip.length));
    }
    public void PlaySlimeDeath()     => PlaySFX(slimeDeathClip, slimeDeathVolume);
    public void PlayMiniSlimeDeath() => PlaySFX(slimeDeathClip, miniSlimeDeathVolume);

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

    public void PlayShroomReveal()   => PlaySFX(shroomRevealClip,   shroomRevealVolume);
    public void PlayShroomGas()      => PlaySFX(shroomGasClip,      shroomGasVolume);
    public void PlayShroomDeathGas() => PlaySFX(shroomDeathGasClip, shroomDeathGasVolume);
    public void PlayShroomDeath()    => PlaySFX(shroomDeathClip,    shroomDeathVolume);
    public void PlayShroomIdle()     => PlaySFX(shroomIdleClip,     shroomIdleVolume);

    // ── Boss ──────────────────────────────────────────────────────────────────

    public void PlayPlayerCloneDeath() => PlaySFX(playerCloneDeathClip, playerCloneDeathVolume);
    public void PlayBossLetterBoom() => PlaySFX(bossLetterBoomClip, bossLetterBoomVolume);
    public void PlayBossTransform()  => PlaySFX(bossTransformClip,  bossTransformVolume);
    public void PlayBossBarFill()    => PlaySFX(bossBarFillClip, bossBarFillVolume);

    // ── Music ─────────────────────────────────────────────────────────────────

    public void PlayStartAreaAmbient()
    {
        if (startAreaAmbientClip != null && _ambient.clip != startAreaAmbientClip)
        {
            _ambient.clip   = startAreaAmbientClip;
            _ambient.volume = startAreaAmbientVolume;
            _ambient.Play();
        }

        if (leavesRustlingClip != null && _ambient2.clip != leavesRustlingClip)
        {
            _ambient2.clip   = leavesRustlingClip;
            _ambient2.volume = leavesRustlingVolume;
            _ambient2.Play();
        }
    }

    public void PlayCaveEntrance()
    {
        PlaySFX(caveEntranceClip, caveEntranceVolume);
        if (caveAmbientClip == null || _ambient.clip == caveAmbientClip) return;
        _ambient.clip   = caveAmbientClip;
        _ambient.volume = caveAmbientVolume;
        _ambient.Play();
    }

    public void StopAmbient()
    {
        _ambient.Stop();
        _ambient.clip = null;
        _ambient2.Stop();
        _ambient2.clip = null;
    }

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
