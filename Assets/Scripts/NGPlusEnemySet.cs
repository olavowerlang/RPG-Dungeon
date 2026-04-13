using UnityEngine;

/// <summary>
/// Attach to a parent GameObject that has child enemy groups.
/// Activates the correct group based on the current NG+ tier.
/// All other groups are deactivated at Start.
/// </summary>
public class NGPlusEnemySet : MonoBehaviour
{
    [Tooltip("Enemies for a normal run")]
    [SerializeField] private GameObject baseEnemies;

    [Tooltip("Enemies for NG+ (index 0). Last filled slot is reused for higher tiers.")]
    [SerializeField] private GameObject[] ngPlusTiers; // [0]=NG+, [1]=NG++, [2]=NG+++, etc.

    private void Start()
    {
        int ngCount = NGPlusManager.Instance != null ? NGPlusManager.Instance.NGPlusCount : 0;

        // Pick the right tier group
        GameObject active = baseEnemies;
        if (ngCount >= 1 && ngPlusTiers != null && ngPlusTiers.Length > 0)
        {
            int i = Mathf.Min(ngCount - 1, ngPlusTiers.Length - 1);
            if (ngPlusTiers[i] != null)
                active = ngPlusTiers[i];
        }

        // Activate the chosen group, deactivate everything else
        if (baseEnemies != null) baseEnemies.SetActive(baseEnemies == active);
        if (ngPlusTiers != null)
            foreach (var tier in ngPlusTiers)
                if (tier != null) tier.SetActive(tier == active);
    }
}
