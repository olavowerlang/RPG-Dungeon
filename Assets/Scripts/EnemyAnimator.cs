using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private Animator _anim;
    private SkeletonFighter _skeletonFighter;
    [SerializeField] private DamageDealer[] _hitboxes;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
    }

    void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking);

        /* Delega o flip para o próprio SkeletonFighter */
        _skeletonFighter.DefineSfSpriteDirection();
    }


    /* Trigger de ataque disparado pela FSM */
    public void PlaySfAttack() => _anim.SetTrigger(SfAttackTrigger);

    /* ---------- Animation Events ---------- */

    public void EnableHitbox(int i) => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();
    public void OnAttackAnimationEnd() => _skeletonFighter.OnAttackAnimationEnd();



}