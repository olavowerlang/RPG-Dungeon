using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private Animator _anim;
    private SkeletonFighter _skeletonFighter;
    [SerializeField] private GameObject swordHitbox;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
    }

    void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking);

        /* Delega o flip para o próprio SkeletonFighter */
        _skeletonFighter.DefineSfSpriteDirection();
    }

    public void EnableSwordHitbox()
    {
        swordHitbox.SetActive(true);
    }

    public void DisableSwordHitbox()
    {
        swordHitbox.SetActive(false);
    }


    /* Trigger de ataque disparado pela FSM */
    public void PlaySfAttack() => _anim.SetTrigger(SfAttackTrigger);

    /* Evento da animação (último frame do clipe) */
    public void OnAttackAnimationEnd() => _skeletonFighter.OnAttackAnimationEnd();
}