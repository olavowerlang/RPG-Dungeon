using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar for the Clone fight.
/// - Call RevealBar() when the pig reveals his intent.
/// - Bar fills left to right, then Y O U appear one letter at a time.
/// - Tracks clone Health automatically each frame.
/// - Call Hide() on clone death.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Health cloneHealth;
    [SerializeField] private CanvasGroup rootGroup;   // on the root panel so we can fade the whole thing

    [Header("Bar")]
    [SerializeField] private Image fillImage;         // Fill Method: Horizontal, Fill Origin: Left
    [SerializeField] private float revealDuration = 1.6f;

    [Header("Letters — Y O U")]
    [SerializeField] private TextMeshProUGUI letterY;
    [SerializeField] private TextMeshProUGUI letterO;
    [SerializeField] private TextMeshProUGUI letterU;
    [SerializeField] private float letterDelay  = 0.35f;  // gap between each letter
    [SerializeField] private float letterPunch  = 0.2f;   // seconds for scale punch animation

    [Header("Hide")]
    [SerializeField] private float hideDuration = 0.8f;

    private bool _revealed;

    private void Awake()
    {
        Instance = this;

        // Start fully hidden
        rootGroup.alpha          = 0f;
        fillImage.fillAmount     = 0f;
        SetLetterAlpha(letterY, 0f);
        SetLetterAlpha(letterO, 0f);
        SetLetterAlpha(letterU, 0f);
    }

    private void Update()
    {
        if (!_revealed || cloneHealth == null) return;
        fillImage.fillAmount = (float)cloneHealth.currentHp / cloneHealth.MaxHP;
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

    // ── Coroutines ───────────────────────────────────────────────────────────

    private IEnumerator RevealRoutine()
    {
        // 1. Fade in the panel
        yield return StartCoroutine(FadeGroup(rootGroup, 0f, 1f, 0.3f));

        // 2. Fill bar left to right
        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed          += Time.deltaTime;
            fillImage.fillAmount = Mathf.SmoothStep(0f, 1f, elapsed / revealDuration);
            yield return null;
        }
        fillImage.fillAmount = 1f;

        // 3. Letters appear one at a time with a scale punch
        yield return StartCoroutine(RevealLetter(letterY));
        yield return new WaitForSeconds(letterDelay);
        yield return StartCoroutine(RevealLetter(letterO));
        yield return new WaitForSeconds(letterDelay);
        yield return StartCoroutine(RevealLetter(letterU));
    }

    private IEnumerator RevealLetter(TextMeshProUGUI letter)
    {
        SetLetterAlpha(letter, 1f);
        letter.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < letterPunch)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / letterPunch;
            // Overshoot to 1.2 then settle at 1
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
            elapsed     += Time.deltaTime;
            group.alpha  = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private void SetLetterAlpha(TextMeshProUGUI tmp, float alpha)
    {
        Color c = tmp.color;
        c.a     = alpha;
        tmp.color = c;
    }
}
