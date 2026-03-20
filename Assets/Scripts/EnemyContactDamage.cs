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

    private float _nextHitTime;

    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);
    private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    private void TryDamage(Collider2D other)
    {
        if (Time.time < _nextHitTime) return;
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (!other.TryGetComponent<IDamageable>(out var target)) return;

        Vector2 dir = (other.transform.position - transform.position).normalized;
        target.TakeHit(damage, dir, knockback);
        _nextHitTime = Time.time + hitCooldown;
    }
}
