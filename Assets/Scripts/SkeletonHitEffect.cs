using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class SkeletonHitEffect : MonoBehaviour, IDamageable
{
    [Header("Knock / Blink / Stun")]
    [SerializeField] float knockForce       = 6f;
    [SerializeField] float impulseDecayRate = 10f;
    [SerializeField] float blinkDur         = 0.3f;
    [SerializeField] float blinkFreq        = 0.05f;
    [SerializeField] float stunTime         = 0.3f;

    Health           _hp;
    SpriteRenderer   _sr;
    SkeletonFighter  _ai;
    EnemyAnimator    _enemyAnim;  
    Animator         _anim; //tive que pegar o ANIM, talvez de pra puxar o takehit direto no enemyanimator?
    Rigidbody2D      _rb;

    Vector2 _impulseVel;

    void Awake()
    {
        _hp        = GetComponent<Health>();
        _sr        = GetComponentInChildren<SpriteRenderer>(true);
        _ai        = GetComponent<SkeletonFighter>();
        _enemyAnim = GetComponent<EnemyAnimator>();          
        _anim      = GetComponentInChildren<Animator>(true);  
        _rb        = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_impulseVel != Vector2.zero)
        {
            _rb.velocity += _impulseVel;
            _impulseVel   = Vector2.Lerp(_impulseVel, Vector2.zero,
                                         Time.fixedDeltaTime * impulseDecayRate);
        }
    }

    public void TakeHit(int dmg, Vector2 dir)
    {
        if (_hp.IsDead) return;

        _impulseVel += dir.normalized * knockForce;
        _hp.TakeDamage(dmg);

        StartCoroutine(Blink());
        StartCoroutine(Stun());
    }

    IEnumerator Blink()
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

    IEnumerator Stun()
    {
        _ai.enabled = false;               // trava IA

        /* --- congela animação em Idle --- */
        if (_anim)
        {
            _anim.Play("Idle", 0, 0f);     // ajuste o nome se preciso
            _anim.enabled = false;
        }
        if (_enemyAnim) _enemyAnim.enabled = false;

        yield return new WaitForSeconds(stunTime);

        if (_enemyAnim) _enemyAnim.enabled = true;
        if (_anim)      _anim.enabled = true;
        _ai.enabled = true;
    }
}
