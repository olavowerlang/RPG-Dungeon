using UnityEngine;

/// <summary>
/// Deals damage to the player on contact with a cooldown.
/// Place on the enemy root (or any collider child).
/// </summary>
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockback = 3f;
    [SerializeField] private float hitCooldown = 1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool useCollision = false;

    private float _nextHitTime;

    private void OnTriggerEnter2D(Collider2D other) { if (!useCollision) TryDamage(other); }
    private void OnTriggerStay2D(Collider2D other)  { if (!useCollision) TryDamage(other); }
    private void OnCollisionEnter2D(Collision2D col) { if (useCollision) TryDamage(col.collider); }
    private void OnCollisionStay2D(Collision2D col)  { if (useCollision) TryDamage(col.collider); }

    /// <summary>
    /// Scales enemy stats for NG+.
    /// Knockback multiplies by the stat multiplier as usual.
    /// Damage uses additive bonus: +0 at NG+1, +1 at NG+2, +2 at NG+3+, capped at +2.
    /// </summary>
    public void ScaleDamage(float multiplier, int ngPlusCount)
    {
        damage    += Mathf.Min(ngPlusCount - 1, 2);
        knockback *= multiplier;
    }

    private void TryDamage(Collider2D other)
    {
        if (GetComponent<CloneAI>() != null) return;
        if (Time.time < _nextHitTime) return;
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (!other.TryGetComponent<IDamageable>(out var target)) return;

        Vector2 dir = (other.transform.position - transform.position).normalized;
        target.TakeHit(damage, dir, knockback);
        _nextHitTime = Time.time + hitCooldown;
    }
}
