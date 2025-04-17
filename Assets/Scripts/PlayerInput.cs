using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Vector2 InputDirection { get; set; }
    
    private PlayerInputActions _playerInputActions;
    
    [SerializeField] private PlayerCombat playerCombat;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();

    }
    
    private void OnEnable()
    {
        _playerInputActions.Player.Enable();
        
        _playerInputActions.Player.LightAttack.performed += OnLightAttack;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.LightAttack.performed -= OnLightAttack;
        
        _playerInputActions.Player.Disable();
    }

    public Vector2 GetInputDirection()
    {
        InputDirection = _playerInputActions.Player.Move.ReadValue<Vector2>();
        
        InputDirection = InputDirection.normalized;
        
        return InputDirection;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        playerCombat.LightAttack();
    }
    
}
