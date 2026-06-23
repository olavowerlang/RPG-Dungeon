using UnityEngine;

/// <summary>
/// Attach to every enemy prefab (NOT the player, NOT the clone).
/// In Start(), scales HP, contact damage, and all AI stats if this is a NG+ run.
/// Multiplier scales linearly: NG+1=x2, NG+2=x3, NG+3=x4, etc.
/// </summary>
public class NGPlusEnemyScaler : MonoBehaviour
{
    private void Start()
    {
        int ngCount = NGPlusManager.Instance?.NGPlusCount ?? 0;
        int dmgBonus = 1 + Mathf.Max(0, ngCount - 3);

        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null)
            contact.ScaleDamage(1f, ngCount);

        foreach (var dealer in GetComponentsInChildren<DamageDealer>(true))
            dealer.Damage = dmgBonus;

        if (NGPlusManager.Instance == null || !NGPlusManager.Instance.IsNGPlus) return;

        float m = NGPlusManager.Instance.EnemyStatMultiplier;

        var health = GetComponent<Health>();
        if (health != null)
            health.ScaleMaxHP(Mathf.RoundToInt(health.MaxHP * m));

        GetComponent<SkeletonFighter>()?.ScaleForNGPlus(m);
        GetComponent<SlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<MiniSlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<SkeletonArcherAI>()?.ScaleForNGPlus(m);
        GetComponent<BombshroomAI>()?.ScaleForNGPlus(m);
    }
}
