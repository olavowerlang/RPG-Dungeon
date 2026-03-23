using UnityEngine;

/// <summary>
/// Temporary test script — drop in scene, assign the Clone, hit Play.
/// Remove once the pig NPC trigger is built.
/// </summary>
public class BossFightStarter : MonoBehaviour
{
    [SerializeField] private CloneAI clone;

    private void Start()
    {
        if (clone == null) { Debug.LogError("BossFightStarter: CloneAI not assigned!"); return; }
        clone.StartFight();

        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.RevealBar();
    }
}
