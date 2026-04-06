using UnityEngine;

/// <summary>
/// Attach to every enemy prefab (NOT the player, NOT the clone).
/// In Start(), scales HP, contact damage, and all AI stats if this is a NG+ run.
/// Multiplier doubles each cleared run: NG+1=x2, NG+2=x4, etc.
/// </summary>
public class NGPlusEnemyScaler : MonoBehaviour
{
    private void Start()
    {
        if (NGPlusManager.Instance == null || !NGPlusManager.Instance.IsNGPlus) return;

        float m = NGPlusManager.Instance.EnemyStatMultiplier;

        var health = GetComponent<Health>();
        if (health != null)
            health.ScaleMaxHP(Mathf.RoundToInt(health.MaxHP * m));

        int ngCount = NGPlusManager.Instance.NGPlusCount;

        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null)
            contact.ScaleDamage(m, ngCount);

        foreach (var dealer in GetComponentsInChildren<DamageDealer>(true))
            dealer.Damage += Mathf.Min(ngCount - 1, 2);

        GetComponent<SlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<MiniSlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<SkeletonArcherAI>()?.ScaleForNGPlus(m);
        GetComponent<BombshroomAI>()?.ScaleForNGPlus(m);
    }
}
