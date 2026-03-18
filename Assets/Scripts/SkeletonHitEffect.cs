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
    private Animator _anim; //tive que pegar o ANIM, talvez de pra puxar o takehit direto no enemyanimator?
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

        // When AI is disabled (stunned), apply directly — nobody else is setting velocity
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


    public void TakeHit(int dmg, Vector2 dir, float knockback = 0f)
    {
        if (_hp.IsDead) return;

        _impulseVel += dir.normalized * (knockForce + knockback);
        _hp.TakeDamage(dmg);
        
        if (_hp.IsDead) return;

        StartCoroutine(Blink());
        StartCoroutine(Stun());
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
        // don't interrupt the skeleton mid-attack — let it finish its swing
        if (_ai.CurrentState == SkeletonFighter.S.Hit) yield break;

        _ai.enabled = false;

        /* --- congela animação em Idle --- */
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
