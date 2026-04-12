using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pig → Clone transformation sequence.
///
/// Timeline:
///   0.0s  Particles burst + pig sprite begins fading out
///   0.6s  Screen begins elegant fade to white (SmoothStep curve, Pokemon-style)
///   1.1s  Screen fully white — pig disabled, clone enabled (swap hidden under flash)
///   1.1s  Screen begins fading back out slowly
///   2.0s  Screen clear, clone fully visible — post-transform dialogue starts
/// </summary>
public class TransformationSequence : MonoBehaviour
{
    [Header("Actors")]
    [SerializeField] private GameObject      pigObject;
    [SerializeField] private SpriteRenderer  pigSprite;      // pig's SpriteRenderer for fade-out
    [SerializeField] private GameObject      cloneObject;    // disabled at scene start
    [SerializeField] private CloneAI         cloneAI;

    [Header("Dialogue")]
    [SerializeField] private DialogueData postTransformDialogue;
    [Tooltip("Index 0 = NG+,  1 = NG++,  2 = NG+++  (last slot reused for higher tiers)")]
    [SerializeField] private DialogueData[] ngPlusPostTransformDialogues;

    [Header("Particles")]
    [SerializeField] private ParticleSystem transformParticles; // auto-created if null
    [SerializeField] private Material       particleMaterial;   // assign URP Particles/Unlit material

    [Header("Screen Flash")]
    [SerializeField] private Image screenFlashImage; // full-screen white Image, alpha 0 at start; auto-created if null

    [Header("Timing")]
    [SerializeField] private float spriteFadeDuration = 0.9f;  // how long pig fades out
    [SerializeField] private float flashRiseStart     = 0.55f; // seconds after start before screen begins rising
    [SerializeField] private float flashRiseDuration  = 0.55f; // 0 → 1 alpha
    [SerializeField] private float flashHoldDuration  = 0.08f; // brief hold at full white
    [SerializeField] private float flashFallDuration  = 0.85f; // 1 → 0 alpha (slower = more elegant)
    [SerializeField] private float postSwapDelay      = 0.5f;  // pause after flash clears before dialogue

    // ── Unity ────────────────────────────────────────────────────────────────

    private PlayerInput _playerInput;

    private void Awake()
    {
        BuildParticlesIfNeeded();
        BuildFlashIfNeeded();

        if (cloneObject != null) cloneObject.SetActive(false);
    }

    private void Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) _playerInput = playerGO.GetComponent<PlayerInput>();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void StartTransformation()
    {
        StartCoroutine(TransformRoutine());
    }

    // ── Sequence ─────────────────────────────────────────────────────────────

    private IEnumerator TransformRoutine()
    {
        // Freeze all player input (blocks movement, dash, and attack)
        if (_playerInput != null) _playerInput.enabled = false;

        AudioManager.Instance?.PlayBossTransform();

        // Detach particles from this object so disabling the pig mid-sequence doesn't kill them
        transformParticles.transform.SetParent(null);

        // Position particles at pig
        if (pigObject != null)
            transformParticles.transform.position = pigObject.transform.position;

        // — Fire particles —
        transformParticles.Play();

        // — Fade pig sprite out (runs in parallel) —
        if (pigSprite != null)
            StartCoroutine(FadeSprite(pigSprite, 1f, 0f, spriteFadeDuration));

        // — Wait, then start the screen flash —
        yield return new WaitForSeconds(flashRiseStart);

        // Rise
        yield return StartCoroutine(FadeFlash(0f, 1f, flashRiseDuration));

        // — SWAP at peak white (completely hidden) —
        Vector3 spawnPos = pigObject != null ? pigObject.transform.position : transform.position;
        if (pigObject != null) pigObject.SetActive(false);

        if (cloneObject != null)
        {
            cloneObject.transform.position = spawnPos;
            cloneObject.SetActive(true);
        }

        // Brief hold at full white
        yield return new WaitForSeconds(flashHoldDuration);

        // Fall (slower — elegant reveal)
        yield return StartCoroutine(FadeFlash(1f, 0f, flashFallDuration));

        yield return new WaitForSeconds(postSwapDelay);

        // Unfreeze player — visual is done
        if (_playerInput != null) _playerInput.enabled = true;

        AudioManager.Instance?.StopAmbient();
        AudioManager.Instance?.PlayBossMusic();

        // — Post-transform dialogue —
        var postDialogue = NGPlusManager.PickDialogue(postTransformDialogue, ngPlusPostTransformDialogues);
        if (postDialogue != null && DialogueManager.Instance != null)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(postDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        // — Reveal HP bar and start fight —
        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.RevealBar();

        if (cloneAI != null)
            cloneAI.StartFight();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// Smooth (SmoothStep) fade of the screen flash image — no jarring linearity.
    private IEnumerator FadeFlash(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetFlashAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetFlashAlpha(to);
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = sr.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            sr.color = c;
            yield return null;
        }
        c.a = to;
        sr.color = c;
    }

    private void SetFlashAlpha(float a)
    {
        if (screenFlashImage == null) return;
        Color c = screenFlashImage.color;
        c.a = a;
        screenFlashImage.color = c;
    }

    // ── Auto-build defaults ──────────────────────────────────────────────────

    private void BuildParticlesIfNeeded()
    {
        if (transformParticles != null) return;

        var go = new GameObject("TransformParticles");
        go.transform.SetParent(transform);
        transformParticles = go.AddComponent<ParticleSystem>();
        transformParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main           = transformParticles.main;
        main.playOnAwake   = false;
        main.duration      = 1.2f;
        main.loop          = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(4f, 9f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
                                 new Color(1f, 0.95f, 0.4f),   // warm gold
                                 new Color(1f, 1f,   1f));     // white
        main.maxParticles  = 80;
        main.gravityModifier = -0.15f; // slight upward drift

        var emission = transformParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });

        var shape       = transformParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.5f;

        // Particles fade out over their lifetime
        var col = transformParticles.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var rend = transformParticles.GetComponent<ParticleSystemRenderer>();
        rend.sortingLayerName = "Default";
        rend.sortingOrder     = 32767;

        Material mat = particleMaterial;
        if (mat == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (s != null) mat = new Material(s);
        }
        if (mat != null) rend.material = mat;

        transformParticles.Stop();
    }

    private void BuildFlashIfNeeded()
    {
        if (screenFlashImage != null)
        {
            SetFlashAlpha(0f);
            return;
        }

        // Create a canvas + full-screen white image
        var canvasGO = new GameObject("FlashCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas             = canvasGO.AddComponent<Canvas>();
        canvas.renderMode      = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder    = 999;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imgGO = new GameObject("FlashImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        screenFlashImage = imgGO.AddComponent<Image>();
        screenFlashImage.color = new Color(1f, 1f, 1f, 0f);

        var rect = imgGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var raycaster = imgGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        screenFlashImage.raycastTarget = false;
    }
}
