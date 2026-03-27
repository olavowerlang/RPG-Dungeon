using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar for the Clone fight.
/// - Call RevealBar() when the pig reveals his intent.
/// - Bar fills left to right via anchorMax.x (works regardless of Image type).
/// - Tracks clone Health automatically each frame.
/// - Call Hide() on clone death.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Health cloneHealth;
    [SerializeField] private CanvasGroup rootGroup;

    [Header("Bar")]
    [SerializeField] private RectTransform fillRect;   // the fill bar RectTransform (anchorMin.x=0, anchorMax.x driven by code)
    [SerializeField] private float revealDuration = 1.8f;

    [Header("Letters — Y O U")]
    [SerializeField] private TextMeshProUGUI letterY;
    [SerializeField] private TextMeshProUGUI letterO;
    [SerializeField] private TextMeshProUGUI letterU;
    [SerializeField] private float letterDelay = 0.35f;
    [SerializeField] private float letterPunch = 0.2f;

    [Header("Hide")]
    [SerializeField] private float hideDuration = 0.8f;

    [Header("Freeze During Reveal")]
    [SerializeField] private CloneAI cloneAI;

    private bool _revealed;
    private bool _trackingHP;
    private PlayerInput _playerInput;

    private void Awake()
    {
        Instance = this;
        if (revealDuration > 2f) revealDuration = 1.8f;

        rootGroup.alpha = 0f;
        SetFill(0f);
        SetLetterAlpha(letterY, 0f);
        SetLetterAlpha(letterO, 0f);
        SetLetterAlpha(letterU, 0f);
    }

    private void Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) _playerInput = playerGO.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!_trackingHP || cloneHealth == null) return;
        SetFill((float)cloneHealth.currentHp / cloneHealth.MaxHP);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void RevealBar()
    {
        if (_revealed) return;
        _revealed = true;
        StartCoroutine(RevealRoutine());
    }

    public void Hide()
    {
        StartCoroutine(HideRoutine());
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Drives the fill bar by stretching its right anchor from 0 to 1.
    // Works regardless of Image type — no fillAmount dependency.
    private void SetFill(float t)
    {
        if (fillRect == null) return;
        Vector2 max = fillRect.anchorMax;
        max.x = Mathf.Clamp01(t);
        fillRect.anchorMax = max;
    }

    // ── Coroutines ───────────────────────────────────────────────────────────

    private IEnumerator RevealRoutine()
    {
        if (_playerInput != null) _playerInput.enabled = false;
        if (cloneAI != null) cloneAI.enabled = false;

        yield return StartCoroutine(FadeGroup(rootGroup, 0f, 1f, 0.3f));
        AudioManager.Instance?.PlayBossBarFill();

        SetFill(0f);
        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            SetFill(Mathf.SmoothStep(0f, 1f, elapsed / revealDuration));
            yield return null;
        }
        SetFill(1f);

        yield return StartCoroutine(RevealLetter(letterY));
        yield return new WaitForSeconds(letterDelay);
        yield return StartCoroutine(RevealLetter(letterO));
        yield return new WaitForSeconds(letterDelay);
        yield return StartCoroutine(RevealLetter(letterU));

        if (_playerInput != null) _playerInput.enabled = true;
        if (cloneAI != null) cloneAI.enabled = true;

        _trackingHP = true;
    }

    private IEnumerator RevealLetter(TextMeshProUGUI letter)
    {
        AudioManager.Instance?.PlayBossLetterBoom();
        SetLetterAlpha(letter, 1f);
        letter.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < letterPunch)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / letterPunch;
            float scale = t < 0.6f
                ? Mathf.Lerp(0f, 1.2f, t / 0.6f)
                : Mathf.Lerp(1.2f, 1f, (t - 0.6f) / 0.4f);
            letter.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        letter.transform.localScale = Vector3.one;
    }

    private IEnumerator HideRoutine()
    {
        yield return StartCoroutine(FadeGroup(rootGroup, rootGroup.alpha, 0f, hideDuration));
        gameObject.SetActive(false);
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private void SetLetterAlpha(TextMeshProUGUI tmp, float alpha)
    {
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }
}
