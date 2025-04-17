using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsWalking           = Animator.StringToHash("isWalking");
    private static readonly int LightAttackTrigger1 = Animator.StringToHash("LightAttackTrigger1");
    private static readonly int LightAttackTrigger2 = Animator.StringToHash("LightAttackTrigger2");
    private static readonly int LightAttackTrigger3 = Animator.StringToHash("LightAttackTrigger3");

    private bool _lightAttackDone  = false; 
    private bool _lightAttack2Done = false; 

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        _animator       = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _animator.SetBool(IsWalking, playerController.IsWalking());
        DefineSpriteDirection();
    }

    private void DefineSpriteDirection()
    {
        if (playerController.InputDirection.x != 0)
            _spriteRenderer.flipX = playerController.InputDirection.x < 0;
    }

    /* ------------------ COMBO ------------------ */

    public void TriggerLightAttack()
    {
       
        if (!_lightAttackDone && !_lightAttack2Done)
        {
            _animator.SetTrigger(LightAttackTrigger1);
        }
        
        else if (_lightAttackDone && !_lightAttack2Done)
        {
            _animator.SetTrigger(LightAttackTrigger2);
        }
       
        else if (_lightAttackDone && _lightAttack2Done)
        {
            _animator.SetTrigger(LightAttackTrigger3);

            // Reset imediato – impede retrigger
            _lightAttackDone  = false;
            _lightAttack2Done = false;
        }
    }

    /* ---------- Animation Events ---------- */

    // Janela para emendar golpe 2
    public void LightAttack2Window() => _lightAttackDone = true;

    // Golpe 1 terminou sem combo
    public void LightAttack1Ended()
    {
        _lightAttackDone  = false;
        _lightAttack2Done = false; // segurança
    }

    // Golpe 2 terminou sem combo
    public void LightAttack2Ended()
    {
        _lightAttackDone  = false;
        _lightAttack2Done = false; // obrigatório
    }

    // Janela para emendar golpe 3
    public void LightAttack3Window() => _lightAttack2Done = true;

    // Golpe 3 terminou
    public void LightAttack3Ended()
    {
        _lightAttackDone  = false;
        _lightAttack2Done = false;
    }
}
