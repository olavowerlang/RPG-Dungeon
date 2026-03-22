using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Vector2 InputDirection { get; set; }
    
    private PlayerInputActions _playerInputActions;
    
    private PlayerController _playerController;
    private PlayerStats _stats;

    [SerializeField] private PlayerCombat playerCombat;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerController = GetComponent<PlayerController>();
        _stats = GetComponent<PlayerStats>();
    }
    
    private void OnEnable()
    {
        if (_playerInputActions == null) _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        
        _playerInputActions.Player.LightAttack.performed += OnLightAttack;

        _playerInputActions.Player.Dash.performed += OnDash;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.LightAttack.performed -= OnLightAttack;
        
        _playerInputActions.Player.Dash.performed -= OnDash;
        
        _playerInputActions.Player.Disable();
    }

    public Vector2 GetInputDirection()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue)
            return Vector2.zero;

        InputDirection = _playerInputActions.Player.Move.ReadValue<Vector2>();

        InputDirection = InputDirection.normalized;

        return InputDirection;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;
        if (_stats == null || !_stats.hasSword) return;
        playerCombat.LightAttack();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;
        if (_stats == null || !_stats.hasDash) return;
        _playerController.Dash();
    }
}
