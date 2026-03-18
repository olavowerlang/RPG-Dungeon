using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class PlayerHitEffect : MonoBehaviour, IDamageable
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Health _hp;
    private bool _invuln;

    private PlayerController _player;
    private PlayerStats _stats;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _hp = GetComponent<Health>();
        _player = GetComponent<PlayerController>();
        _stats = GetComponent<PlayerStats>();
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();   // mata qualquer coroutine pendente
        _sr.enabled = true;    // garante Sprite visível no fim
        _invuln = false;       // reseta flag
    }

    public void TakeHit(int dmg, Vector2 dir, float knockback = 0f)
    {
        if (_invuln || _hp.IsDead) return;

        _rb.velocity = Vector2.zero;
        _player.ApplyAttackPush(dir, _stats.receivedKnockForce);

        _hp.TakeDamage(dmg);
        StartCoroutine(BlinkInvuln());
    }

    private IEnumerator BlinkInvuln()
    {
        _invuln = true;
        float t = 0;
        while (t < _stats.invulnTime)
        {
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(_stats.blinkFreq);
            t += _stats.blinkFreq;
        }
        _sr.enabled = true;
        _invuln = false;
    }
}