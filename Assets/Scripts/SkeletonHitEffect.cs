using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class SkeletonHitEffect : MonoBehaviour, IDamageable
{
    [SerializeField] private float knockForce = 5f;
    [SerializeField] private float blinkDur   = 0.3f;
    [SerializeField] private float blinkFreq  = 0.05f;
    [SerializeField] private float stunTime   = 0.3f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Health _hp;
    private SkeletonFighter _sf;            // seu script de IA

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _hp = GetComponent<Health>();
        _sf = GetComponent<SkeletonFighter>();
    }

    public void TakeHit(int dmg, Vector2 dir)
    {
        if (_hp.IsDead) return;

        _rb.velocity = Vector2.zero;
        _rb.AddForce(dir * knockForce, ForceMode2D.Impulse);

        _hp.TakeDamage(dmg);

        StartCoroutine(Blink());
        StartCoroutine(Stun());
    }

    private IEnumerator Blink()
    {
        float t = 0;
        while (t < blinkDur)
        {
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkFreq);
            t += blinkFreq;
        }
        _sr.enabled = true;
    }

    private IEnumerator Stun()
    {
        _sf.enabled = false;
        yield return new WaitForSeconds(stunTime);
        _sf.enabled = true;
    }
}