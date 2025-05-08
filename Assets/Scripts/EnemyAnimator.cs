using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private Animator _anim;
    private SkeletonFighter _skeletonFighter;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
    }

    private void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking());
    }
    
    public void PlaySfAttack()
        => _anim.SetTrigger(SfAttackTrigger);


}
