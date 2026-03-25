using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lives in the Cave scene (on the Canvas).
/// Called by CloneAI after the death dialogue and death animation finish.
/// Fades to black → shows white ending text → loads main scene in NG+ mode.
/// </summary>
public class EndingSequence : MonoBehaviour
{
    public static EndingSequence Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image            blackOverlay;   // full-screen black Image, alpha 0 at start
    [SerializeField] private TextMeshProUGUI  endingText;     // white text, alpha 0 at start

    [Header("Settings")]
    [SerializeField] private string mainSceneName   = "Main Scene";
    [SerializeField] private float  blackFadeTime   = 2f;    // how long to fade to full black
    [SerializeField] private float  textFadeInTime  = 2f;    // text fades in once screen is black
    [SerializeField] private float  displayTime     = 5f;    // how long the text stays visible

    private void Awake()
    {
        Instance = this;
        SetBlackAlpha(0f);
        if (endingText != null) endingText.alpha = 0f;
    }

    public void StartEnding()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // Snapshot stats and mark game cleared
        if (NGPlusManager.Instance == null)
            new GameObject("NGPlusManager").AddComponent<NGPlusManager>();

        NGPlusManager.Instance.SetGameCleared();

        // Fade screen to black
        float t = 0f;
        while (t < blackFadeTime)
        {
            t += Time.deltaTime;
            SetBlackAlpha(Mathf.SmoothStep(0f, 1f, t / blackFadeTime));
            yield return null;
        }
        SetBlackAlpha(1f);

        // Fade in the ending text
        if (endingText != null)
        {
            t = 0f;
            while (t < textFadeInTime)
            {
                t += Time.deltaTime;
                endingText.alpha = Mathf.Clamp01(t / textFadeInTime);
                yield return null;
            }
            endingText.alpha = 1f;
        }

        yield return new WaitForSeconds(displayTime);

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
