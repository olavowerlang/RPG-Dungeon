using System.Collections;
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
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private Image      blackOverlay;

    [Header("Settings")]
    [SerializeField] private string cutsceneSceneName = "GoodEnding";
    [SerializeField] private string mainSceneName     = "Main Scene";
    [SerializeField] private float  blackFadeTime     = 2f;

    private void Awake()
    {
        Instance = this;
        SetBlackAlpha(0f);
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

        // Reset session state before leaving
        if (NGPlusManager.Instance != null)
            Destroy(NGPlusManager.Instance.gameObject);
        UIManager.ResetHudUnlocked();

        // Load cutscene scene — screen is already black so transition is seamless
        SceneManager.LoadScene(cutsceneSceneName);
    }

    private void SetBlackAlpha(float a)
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }
}
