using UnityEngine;

/// <summary>
/// DEBUG ONLY — drop this on any GameObject in the scene to start in a specific NG+ tier.
/// Remove the GameObject (or this component) before shipping.
/// </summary>
public class DebugNGPlusSkip : MonoBehaviour
{
    [Tooltip("1 = NG+,  2 = NG++,  3 = NG+++,  etc.  Set to 0 to disable.")]
    [SerializeField] private int targetTier = 1;

    private void Awake()
    {
        if (targetTier < 1) return;

        // If NGPlusManager doesn't exist yet, create it
        if (NGPlusManager.Instance == null)
            new GameObject("NGPlusManager").AddComponent<NGPlusManager>();

        NGPlusManager.Instance.DebugForceNGPlusTier(targetTier);
    }
}
