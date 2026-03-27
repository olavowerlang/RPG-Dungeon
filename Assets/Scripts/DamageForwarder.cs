using UnityEngine;

/// <summary>
/// Place on a trigger collider child of an enemy.
/// Forwards incoming hits to the parent's IDamageable (e.g. GenericEnemyHitEffect).
/// Lets you decouple the damage detection collider from the physics collider.
/// </summary>
public class DamageForwarder : MonoBehaviour, IDamageable
{
    private IDamageable _target;

    private void Awake()
    {
        _target = transform.parent.GetComponent<IDamageable>();
    }

    public bool TakeHit(int damage, Vector2 direction, float knockback = 0f)
    {
        return _target != null && _target.TakeHit(damage, direction, knockback);
    }
}
