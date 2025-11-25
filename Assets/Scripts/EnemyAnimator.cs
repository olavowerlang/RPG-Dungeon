using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{//animator handling too much, not following separation of responsibility

    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Die = Animator.StringToHash("Die");

    [SerializeField] private DamageDealer[] _hitboxes;
    private Animator _anim;
    private SkeletonFighter _skeletonFighter;
    private SkeletonHitEffect _skeletonHitEffect;
    private Health _health;
    private Rigidbody2D _rb;

    [SerializeField] private GameObject skullFighter;

    private bool _enemyDeathTriggered;
   

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
        _skeletonHitEffect = GetComponentInParent<SkeletonHitEffect>();
        _health = GetComponentInParent<Health>();
        _rb = GetComponentInParent<Rigidbody2D>();

    }

    private void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking);

        /* Delega o flip para o próprio SkeletonFighter */
        if (!_enemyDeathTriggered)        
            _skeletonFighter.DefineSfSpriteDirection();

        if (_health.IsDead && !_enemyDeathTriggered)
        {
            _enemyDeathTriggered = true;
            _anim.applyRootMotion = false;
            _anim.SetTrigger(Die);
            _skeletonFighter.enabled = false;
            skullFighter.GetComponent<Collider2D>().enabled = false;

            StartCoroutine(FreezeAfterDeath());
            StartCoroutine(WaitForEnemyDeathAnim());
        }


    }

    //por enquanto so funciona pro SkeletonFighter, precisa ser modularizado later on
    /* ---------- NEEDS TO BE SEPARATADED FROM ANIM ---------- */

    private IEnumerator WaitForEnemyDeathAnim()
    {
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).IsName("SkeletonFighter_Die"));

        // wait for it to finish (normalizedTime goes from 0-1)
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        // --- ADD THIS HERE ---
        // Finds the XPManager in the scene (which is on the Player)
        XPManager playerXP = FindObjectOfType<XPManager>();

        // If found, give 5 XP
        if (playerXP != null)
        {
            playerXP.GainXP(5); // <-- The enemy gives the XP
        }
        // -------------------------

        Destroy(transform.root.gameObject);
    }

    private IEnumerator FreezeAfterDeath()
    {
        // 1) Deixa entrar o último impulso de física
        yield return new WaitForFixedUpdate();
        // 2) Dá um pequeno delay extra pra visibilizar melhor o knockback
        yield return new WaitForSeconds(0.25f);

        // 3) Desativa todos os colliders do inimigo pra não gerar mais repulsões
        foreach (var col in transform.root.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // 4) Desliga totalmente a simulação física
        _rb.simulated = false;
    }

    /* Trigger de ataque disparado pela FSM */
    public void PlaySfAttack() => _anim.SetTrigger(SfAttackTrigger);
    
    /* ---------- Animation Events ---------- */

    public void EnableHitbox(int i) => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();
    public void OnAttackAnimationEnd() => _skeletonFighter.OnAttackAnimationEnd();



}