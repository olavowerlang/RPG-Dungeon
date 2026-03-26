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

        // Override bindings at runtime so K=attack, L=dash
        // (index 0 = primary keyboard binding for each action)
        _playerInputActions.Player.LightAttack.ApplyBindingOverride(0, "<Keyboard>/k");
        _playerInputActions.Player.Dash.ApplyBindingOverride(0, "<Keyboard>/l");
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
        if (GameManager.Instance != null && !GameManager.HasStarted)
            return Vector2.zero;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue)
            return Vector2.zero;

        if (NPCInteractionPanel.IsOpen)
            return Vector2.zero;

        InputDirection = _playerInputActions.Player.Move.ReadValue<Vector2>();
        InputDirection = InputDirection.normalized;
        return InputDirection;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && !GameManager.HasStarted) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;
        if (_stats == null || !_stats.hasSword) return;
        playerCombat.LightAttack();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && !GameManager.HasStarted) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;
        if (_stats == null || !_stats.hasDash) return;
        _playerController.Dash();
    }
}
