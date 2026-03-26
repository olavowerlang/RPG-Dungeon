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

        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null)
            contact.ScaleDamage(m);

        GetComponent<SlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<MiniSlimeAI>()?.ScaleForNGPlus(m);
        GetComponent<SkeletonArcherAI>()?.ScaleForNGPlus(m);
        GetComponent<BombshroomAI>()?.ScaleForNGPlus(m);
    }
}
