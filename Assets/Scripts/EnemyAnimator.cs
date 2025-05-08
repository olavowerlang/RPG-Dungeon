using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private Animator _anim;
    private SkeletonFighter _skeletonFighter;

    [SerializeField] private Transform SfVisual;
    public Vector2 LastMovementDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
    }

    private void Update()
    {
        _anim.SetBool(IsWalking, _skeletonFighter.IsWalking());
        DefineEnemySpriteDirection(SfVisual, _skeletonFighter._SfRb);
    }

    private void DefineEnemySpriteDirection(Transform visual, Rigidbody2D _rb)
    {
        if (_rb.velocity != Vector2.zero)
        {
            LastMovementDirection = _rb.velocity;

            float dirX = LastMovementDirection.x;

            bool faceLeft = dirX < 0;

            Vector3 scale = visual.localScale;
            scale.x = faceLeft ? -1.75f : 1.75f;
            visual.localScale = scale;
        }
    }

    public void PlaySfAttack()
        => _anim.SetTrigger(SfAttackTrigger);

}
