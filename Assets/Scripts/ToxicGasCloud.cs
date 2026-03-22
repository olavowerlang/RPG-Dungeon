using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawned by Bombshroom. Lingers on the ground and deals DoT to the player.
/// Checks every child CircleCollider2D so ring-shaped death clouds work correctly.
/// </summary>
public class ToxicGasCloud : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask playerLayer;

    private float _tickTimer;
    private CircleCollider2D[] _cols;

    private void Start()
    {
        _cols = GetComponentsInChildren<CircleCollider2D>();
        _tickTimer = 0f;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            _tickTimer = tickInterval;
            DamageTargets();
        }
    }

    private void DamageTargets()
    {
        var damaged = new HashSet<IDamageable>();
        foreach (var col in _cols)
        {
            if (col == null) continue;
            float worldRadius = col.radius * col.transform.lossyScale.x;
            var hits = Physics2D.OverlapCircleAll(col.transform.position, worldRadius, playerLayer);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var target))
                    damaged.Add(target);
            }
        }
        foreach (var target in damaged)
            target.TakeHit(damagePerTick, Vector2.zero, 0f);
    }
}
