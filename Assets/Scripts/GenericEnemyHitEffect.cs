using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable hit/knockback/blink component for all new enemies.
/// Implements IDamageable. Place on the enemy root.
/// </summary>
[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class GenericEnemyHitEffect : MonoBehaviour, IDamageable
{
    [Header("Knockback")]
    [SerializeField] private float knockForce = 5f;
    [SerializeField] private float impulseDecayRate = 8f;

    [Header("Blink")]
    [SerializeField] private float blinkDuration = 0.3f;
    [SerializeField] private float blinkFrequency = 0.05f;

    private Health _hp;
    private SpriteRenderer _sr;

    private Vector2 _impulseVel;
    private float _invulTimer;

    /// <summary>AI scripts add this to their velocity each FixedUpdate.</summary>
    public Vector2 KnockbackVelocity => _impulseVel;
    public bool IsInvulnerable => _invulTimer > 0f;

    /// <summary>Fired after damage is applied (and entity is still alive).</summary>
    public event Action OnHitTaken;

    private void Awake()
    {
        _hp = GetComponent<Health>();
        _sr = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void FixedUpdate()
    {
        if (_invulTimer > 0f) _invulTimer -= Time.fixedDeltaTime;
        if (_impulseVel == Vector2.zero) return;
        _impulseVel = Vector2.Lerp(_impulseVel, Vector2.zero,
            Time.fixedDeltaTime * impulseDecayRate);
    }

    public void GrantInvulnerability(float duration)
    {
        _invulTimer = duration;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_sr != null) _sr.enabled = true;
    }

    public void TakeHit(int dmg, Vector2 dir, float knockback = 0f)
    {
        if (_hp.IsDead) return;
        if (_invulTimer > 0f) return;

        _impulseVel += dir.normalized * (knockForce + knockback);
        _hp.TakeDamage(dmg);

        if (_hp.IsDead) return;

        OnHitTaken?.Invoke();
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        float elapsed = 0f;
        while (elapsed < blinkDuration)
        {
            if (_sr == null) yield break;
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkFrequency);
            elapsed += blinkFrequency;
        }
        if (_sr != null) _sr.enabled = true;
    }
}
