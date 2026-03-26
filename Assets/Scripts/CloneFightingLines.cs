using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Fires random quip lines on a World Space Canvas TMP element during the clone fight.
/// Assign the Text (TMP) element from the QuipLabel canvas child in the Inspector.
/// </summary>
public class CloneFightingLines : MonoBehaviour
{
    [System.Serializable]
    public class FightLine
    {
        [TextArea] public string text;
        [TextArea] public string followUp;
        public float            followUpDelay = 1f;
    }

    [Header("Lines")]
    [SerializeField] private FightLine[] lines = new FightLine[]
    {
        new FightLine { text = "Stop resisting, man! Please!" },
        new FightLine { text = "Let me kill you and I'll give you some upgrades!" },
        new FightLine { text = "This doesn't have to end like this!", followUp = "Oh wait it does actually!", followUpDelay = 1f },
        new FightLine { text = "Let's wrap this up, I'm getting hungry!" },
        new FightLine { text = "Does that tent of yours have a bed?" },
        new FightLine { text = "You're not that bad with that sword, I'm proud!" },
        new FightLine { text = "Hey look, there's a monster behind you! Look there, trust me!" },
        new FightLine { text = "I'm a wizard, why am I even using a sword?" },
        new FightLine { text = "Whew! Sure feels good walking on two legs again!" },
    };

    [Header("Timing")]
    [SerializeField] private float minInterval = 6f;
    [SerializeField] private float maxInterval = 13f;

    [Header("Text Element")]
    [SerializeField] private TextMeshProUGUI label;

    [Header("Display Timing")]
    [SerializeField] private float fadeInTime  = 0.25f;
    [SerializeField] private float holdTime    = 2.8f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private CloneAI _ai;
    private bool    _running;

    private void Awake()
    {
        _ai = GetComponent<CloneAI>();
        SetLabelAlpha(0f);
    }

    public void StartLines()
    {
        if (_running) return;
        _running = true;
        StartCoroutine(LineLoop());
    }

    public void StopLines()
    {
        _running = false;
        StopAllCoroutines();
        SetLabelAlpha(0f);
    }

    private IEnumerator LineLoop()
    {
        if (lines == null || lines.Length == 0) yield break;

        var shuffled = (FightLine[])lines.Clone();
        Shuffle(shuffled);
        int index = 0;

        while (_running)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            if (!_running) yield break;

            if (_ai != null && (_ai.CurrentState == CloneAI.State.Dead ||
                                 _ai.CurrentState == CloneAI.State.Phase2Talk))
                continue;

            if (index >= shuffled.Length)
            {
                Shuffle(shuffled);
                index = 0;
            }

            var line = shuffled[index++];
            yield return StartCoroutine(ShowLine(line.text));

            if (!string.IsNullOrEmpty(line.followUp))
            {
                yield return new WaitForSeconds(line.followUpDelay);
                yield return StartCoroutine(ShowLine(line.followUp));
            }
        }
    }

    private IEnumerator ShowLine(string text)
    {
        if (label == null) yield break;

        label.text = text;

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            SetLabelAlpha(Mathf.Clamp01(t / fadeInTime));
            yield return null;
        }
        SetLabelAlpha(1f);

        yield return new WaitForSeconds(holdTime);

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            SetLabelAlpha(1f - Mathf.Clamp01(t / fadeOutTime));
            yield return null;
        }
        SetLabelAlpha(0f);
    }

    private void SetLabelAlpha(float a)
    {
        if (label == null) return;
        Color c = label.color;
        c.a = a;
        label.color = c;
    }

    private void Shuffle(FightLine[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
