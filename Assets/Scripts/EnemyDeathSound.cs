using UnityEngine;

/// <summary>
/// Plays a death sound when Health.OnDeath fires.
/// Add to any enemy root and drag in the clip — no other wiring needed.
/// </summary>
public class EnemyDeathSound : MonoBehaviour
{
    [SerializeField] private AudioClip deathClip;

    private void Start()
    {
        var health = GetComponent<Health>();
        if (health != null)
            health.OnDeath += () => AudioManager.Instance?.PlaySFX(deathClip);
    }
}
