using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawned by Bombshroom. Lingers on the ground and deals DoT to the player.
/// Attach to a trigger-collider GameObject. Set lifetime and tick damage in Inspector.
/// </summary>
public class ToxicGasCloud : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask playerLayer;

    private float _tickTimer;
    private readonly List<IDamageable> _targetsInCloud = new();

    private void Start()
    {
        _tickTimer = tickInterval;
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
        foreach (var target in _targetsInCloud)
            target.TakeHit(damagePerTick, Vector2.zero, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.TryGetComponent<IDamageable>(out var dmg))
            _targetsInCloud.Add(dmg);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
            _targetsInCloud.Remove(dmg);
    }
}
