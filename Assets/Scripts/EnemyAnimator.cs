using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Die = Animator.StringToHash("Die");

    [SerializeField] private DamageDealer[] _hitboxes;
    private Animator _anim;
    private SkeletonFighter _skeletonFighter;
    private SkeletonHitEffect _skeletonHitEffect;
    private Health _health;
    
    [SerializeField] private GameObject skullFighter;

    private bool _deathTriggered;
   

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
        _skeletonHitEffect = GetComponentInParent<SkeletonHitEffect>();
        _health = GetComponentInParent<Health>();
    }

    private void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking);

        /* Delega o flip para o próprio SkeletonFighter */
        _skeletonFighter.DefineSfSpriteDirection();

        if (_health.IsDead && !_deathTriggered)
        {
            _deathTriggered = true;

            _anim.SetTrigger(Die);

            _skeletonHitEffect.enabled = false;
            _skeletonFighter.enabled   = false;

            // desliga o único collider raiz
            skullFighter.GetComponent<Collider2D>().enabled = false;

            StartCoroutine(WaitForEnemyDeathAnim());
        }

       
    }
    
    //por enquanto so funciona pro SkeletonFighter, precisa ser modularizado later on
    private IEnumerator WaitForEnemyDeathAnim() 
    {
       
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).IsName("SkeletonFighter_Die"));

        // espera terminar (normalizedTime vai de 0-1)
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        Destroy(transform.root.gameObject);
    }
    
    /* Trigger de ataque disparado pela FSM */
    public void PlaySfAttack() => _anim.SetTrigger(SfAttackTrigger);
    
    /* ---------- Animation Events ---------- */

    public void EnableHitbox(int i) => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();
    public void OnAttackAnimationEnd() => _skeletonFighter.OnAttackAnimationEnd();



}