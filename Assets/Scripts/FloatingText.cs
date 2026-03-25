using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Spawns a world-space floating text at a position that drifts up and fades out.
/// Call FloatingText.Spawn("+10 Gold", position) from anywhere.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private float riseSpeed  = 1.2f;
    [SerializeField] private float duration   = 1.4f;
    [SerializeField] private float fontSize   = 4f;
    [SerializeField] private Color textColor  = Color.yellow;

    private TextMeshPro _tmp;

    public static void Spawn(string message, Vector3 worldPos)
    {
        var go  = new GameObject("FloatingText");
        go.transform.position = worldPos;
        var ft  = go.AddComponent<FloatingText>();
        ft.Init(message);
    }

    private void Init(string message)
    {
        _tmp                  = gameObject.AddComponent<TextMeshPro>();
        _tmp.text             = message;
        _tmp.fontSize         = fontSize;
        _tmp.color            = textColor;
        _tmp.alignment        = TextAlignmentOptions.Center;
        _tmp.fontStyle        = FontStyles.Bold;
        _tmp.outlineWidth     = 0.2f;
        _tmp.outlineColor     = Color.black;
        _tmp.enableWordWrapping = false;
        _tmp.rectTransform.sizeDelta = new Vector2(6f, 2f);
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Color c = _tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            c.a = Mathf.SmoothStep(1f, 0f, elapsed / duration);
            _tmp.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
