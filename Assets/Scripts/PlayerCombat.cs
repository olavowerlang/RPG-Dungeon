using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;
    private PlayerController _playerController;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public void LightAttack()
    {
        playerAnimator.TriggerLightAttack();
        
        Vector2 attackDirection = _playerController.GetAttackDirection();

        _playerController.ApplyAttackPush(attackDirection);
    }
}