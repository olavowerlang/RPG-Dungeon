using System.Collections;
using UnityEngine;

/// <summary>
/// Hides all SpriteRenderers on this object for a set delay, then reveals them.
/// </summary>
public class DelayedReveal : MonoBehaviour
{
    public void Reveal(float delay)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;

        StartCoroutine(RevealAfter(delay));
    }

    private IEnumerator RevealAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = true;
        Destroy(this);
    }
}
