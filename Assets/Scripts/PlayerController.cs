using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private readonly float _impulseDecayRate = 10f;

    private Rigidbody2D _rb;
    private PlayerInput _input;
    private PlayerStats _stats;
    private bool _isWalking;
    public Vector2 InputDirection { get; private set; }
    public Vector2 LastMovementDirection { get; private set; } = Vector2.right;

    private Vector2 _impulseVelocity = Vector2.zero;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        _stats = GetComponent<PlayerStats>();
    }

    private void FixedUpdate()
    {
        InputDirection = _input.GetInputDirection();

        if (InputDirection != Vector2.zero)
            LastMovementDirection = InputDirection;

        var inputVelocity = new Vector2(InputDirection.x * _stats.speed, InputDirection.y * _stats.speed);

        _rb.velocity = inputVelocity + _impulseVelocity;

        _isWalking = inputVelocity != Vector2.zero;

        _impulseVelocity = Vector2.Lerp(_impulseVelocity, Vector2.zero, Time.fixedDeltaTime * _impulseDecayRate);
    }

    public Vector2 GetAttackDirection()
    {
        if (InputDirection == Vector2.zero)
            return LastMovementDirection;
        return InputDirection;
    }

    public bool IsWalking() => _isWalking;

    public void ApplyAttackPush(Vector2 direction, float pushForce)
    {
        direction = direction.normalized;
        _impulseVelocity += direction * pushForce;
    }

    public void Dash()
    {
        _impulseVelocity += LastMovementDirection * _stats.dashForce;
    }
}