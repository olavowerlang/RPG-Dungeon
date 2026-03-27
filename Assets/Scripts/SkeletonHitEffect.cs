using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class SkeletonHitEffect : MonoBehaviour, IDamageable
{
    [Header("Knock / Blink / Stun")] [SerializeField]
    private float knockForce = 6f;

    [SerializeField] private float impulseDecayRate = 10f;
    [SerializeField] private float blinkDur = 0.3f;
    [SerializeField] private float blinkFreq = 0.05f;
    [SerializeField] private float stunTime = 0.3f;

    private Health _hp;
    private SpriteRenderer _sr;
    private SkeletonFighter _ai;
    private EnemyAnimator _enemyAnim;
    private Animator _anim;
    private Rigidbody2D _rb;

    private Vector2 _impulseVel;

    private void Awake()
    {
        _hp        = GetComponent<Health>();
        _sr        = GetComponentInChildren<SpriteRenderer>(true);
        _ai        = GetComponent<SkeletonFighter>();
        _enemyAnim = GetComponent<EnemyAnimator>();
        _anim      = GetComponentInChildren<Animator>(true);
        _rb        = GetComponent<Rigidbody2D>();
    }

    public Vector2 KnockbackVelocity => _impulseVel;

    private void FixedUpdate()
    {
        if (_impulseVel == Vector2.zero) return;

        if (!_ai.enabled)
            _rb.velocity = _impulseVel;

        _impulseVel = Vector2.Lerp(_impulseVel, Vector2.zero,
                                   Time.fixedDeltaTime * impulseDecayRate);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_sr != null) _sr.enabled = true;
    }

    public bool TakeHit(int dmg, Vector2 dir, float knockback = 0f)
    {
        if (_hp.IsDead) return false;

        _impulseVel += dir.normalized * (knockForce + knockback);
        _hp.TakeDamage(dmg);

        if (_hp.IsDead) return true;

        StartCoroutine(Blink());
        StartCoroutine(Stun());
        return true;
    }

    private IEnumerator Blink()
    {
        float t = 0;
        while (t < blinkDur)
        {
            if (_sr == null) yield break;
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkFreq);
            t += blinkFreq;
        }
        if (_sr != null) _sr.enabled = true;
    }

    private IEnumerator Stun()
    {
        if (_ai.CurrentState == SkeletonFighter.S.Hit) yield break;

        _ai.enabled = false;

        if (_anim)
        {
            _anim.Play("SkeletonFighter_Idle", 0, 0f);
            _anim.enabled = false;
        }
        if (_enemyAnim) _enemyAnim.enabled = false;

        yield return new WaitForSeconds(stunTime);

        if (_enemyAnim) _enemyAnim.enabled = true;
        if (_anim)      _anim.enabled = true;
        _ai.enabled = true;
    }
}
