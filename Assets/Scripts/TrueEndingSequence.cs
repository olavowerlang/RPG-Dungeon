using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Plays when the player frees Humberto in NG+++.
/// Plays the farewell dialogue, fades to black, shows ending text, loads Main Menu.
/// Does NOT call SetGameCleared — the loop ends here.
/// </summary>
public class TrueEndingSequence : MonoBehaviour
{
    public static TrueEndingSequence Instance { get; private set; }

    [Header("Dialogue")]
    [SerializeField] private DialogueData farewellDialogue;

    [Header("UI")]
    [SerializeField] private GameObject      endingPanel;
    [SerializeField] private Image           blackOverlay;
    [SerializeField] private TextMeshProUGUI endingText;

    [Header("Settings")]
    [SerializeField] private string mainSceneName  = "Main Scene";
    [SerializeField] private float  blackFadeTime  = 2f;
    [SerializeField] private float  textFadeInTime = 2f;
    [SerializeField] private float  displayTime    = 6f;

    private void Awake()
    {
        Instance = this;
        SetBlackAlpha(0f);
        if (endingText != null) endingText.alpha = 0f;
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    public void StartEnding()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // Farewell dialogue before fading
        if (farewellDialogue != null && DialogueManager.Instance != null)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(farewellDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (endingPanel != null) endingPanel.SetActive(true);

        AudioManager.Instance?.StopMusic();

        // Fade to black
        float t = 0f;
        while (t < blackFadeTime)
        {
            t += Time.unscaledDeltaTime;
            SetBlackAlpha(Mathf.SmoothStep(0f, 1f, t / blackFadeTime));
            yield return null;
        }
        SetBlackAlpha(1f);

        // Fade in ending text
        if (endingText != null)
        {
            t = 0f;
            while (t < textFadeInTime)
            {
                t += Time.unscaledDeltaTime;
                endingText.alpha = Mathf.Clamp01(t / textFadeInTime);
                yield return null;
            }
            endingText.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(displayTime);

        // Destroy NGPlusManager so Main Scene starts a completely fresh session
        if (NGPlusManager.Instance != null)
            Destroy(NGPlusManager.Instance.gameObject);

        UIManager.ResetHudUnlocked();
        SceneManager.LoadScene(mainSceneName);
    }

    private void SetBlackAlpha(float a)
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }
}
