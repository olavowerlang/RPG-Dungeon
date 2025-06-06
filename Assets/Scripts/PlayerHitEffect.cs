using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class PlayerHitEffect : MonoBehaviour, IDamageable
{
    [SerializeField] float knockForce = 3f;
    [SerializeField] float invulnTime = 1f;
    [SerializeField] float blinkFreq  = 0.06f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Health _hp;
    private bool _invuln;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _hp = GetComponent<Health>();
    }

    public void TakeHit(int dmg, Vector2 dir)
    {
        if (_invuln || _hp.IsDead) return;

        _rb.velocity = Vector2.zero;
        _rb.AddForce(dir * knockForce, ForceMode2D.Impulse);

        _hp.TakeDamage(dmg);
        StartCoroutine(BlinkInvuln());
    }

    IEnumerator BlinkInvuln()
    {
        _invuln = true;
        float t = 0;
        while (t < invulnTime)
        {
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkFreq);
            t += blinkFreq;
        }
        _sr.enabled = true;
        _invuln = false;
    }
}