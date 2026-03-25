using UnityEngine;

/// <summary>
/// Attach to every enemy prefab (NOT the player).
/// In Start(), scales HP and contact damage if this is a NG+ run.
/// </summary>
public class NGPlusEnemyScaler : MonoBehaviour
{
    private void Start()
    {
        if (NGPlusManager.Instance == null || !NGPlusManager.Instance.IsNGPlus) return;

        float mult = NGPlusManager.Instance.EnemyStatMultiplier;

        var health = GetComponent<Health>();
        if (health != null)
            health.ScaleMaxHP(Mathf.RoundToInt(health.MaxHP * mult));

        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null)
            contact.ScaleDamage(mult);
    }
}
