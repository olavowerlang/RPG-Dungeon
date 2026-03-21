using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonFighter : MonoBehaviour
{
    public enum S { Patrol, Approach, Orbit, DashPrep, DashMove, Hit, Cooldown }
    S _state = S.Patrol;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float waypointReachThreshold = 0.3f;
    [SerializeField] private float patrolWaitTime = 1.5f;
    private Vector2 _spawnPoint;
    private Vector2 _patrolTarget;
    private float _patrolWaitTimer;

    [Header("Dash Tracking")]
    [Range(0f, 1f)]
    [SerializeField] private float dashTrackingFraction = 0.5f;

    [Header("Distances")]
    [SerializeField] private float orbitRadius = 10f;
    [SerializeField] private float attackRange = 2f;

    [Header("Times")]
    [SerializeField] private Vector2 guardTimeRange = new (3f, 6f);
    [SerializeField] private float dashPrepTime = 0.35f;
    [SerializeField] private float cooldownTime = 0.7f;

    [Header("Speed")] [SerializeField] private float approachSpeed;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float dashSpeed;

    [Header("Refs")]
    [SerializeField] private EnemyAnimator enemyAnim;
    [SerializeField] private Transform sfVisual;

    private Rigidbody2D Rb { get; set; }
    private Transform _player;
    private Health _health;
    private SkeletonHitEffect _hitEffect;
    public S CurrentState => _state;
    private Vector2 _dashTarget;
    private float _timer, _guardTimer;

    private float _dashDuration;
    private float _dashElapsed;
    private bool _dashTargetLocked;
    private float _hitTimer;

    public bool IsWalking => Rb.velocity != Vector2.zero;
    public Vector2 MoveDirection { get; private set; }
    public Vector2 LastDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _hitEffect = GetComponent<SkeletonHitEffect>();
        _player = GameObject.FindWithTag("Player").transform;
        _spawnPoint = transform.position;
        PickNewPatrolTarget();
        ResetGuardTimer();
        
        dashSpeed = Random.Range(18f, 25f);
        orbitSpeed = Random.Range(8f, 15f);
        approachSpeed = Random.Range(8f, 15f);
        patrolSpeed = approachSpeed;
        orbitRadius = Random.Range(7f, 14f);
        cooldownTime = Random.Range(0.3f, 1f);
        
    }

    private void FixedUpdate()
    {
        Vector2 toPlayer = _player.position - transform.position;
        float dist = toPlayer.magnitude;

        // Always track player direction except when locked in attack
        if (_state != S.Hit)
            UpdateFacing(toPlayer.normalized);

        switch (_state)
        {
            case S.Patrol:
                if (dist <= detectionRadius)
                {
                    EnterApproach();
                    break;
                }
                if (_patrolWaitTimer > 0f)
                {
                    _patrolWaitTimer -= Time.fixedDeltaTime;
                    Rb.velocity = _hitEffect.KnockbackVelocity;
                    break;
                }
                Vector2 toWaypoint = _patrolTarget - (Vector2)transform.position;
                if (toWaypoint.magnitude <= waypointReachThreshold)
                {
                    _patrolWaitTimer = patrolWaitTime;
                    PickNewPatrolTarget();
                    break;
                }
                MoveDirection = toWaypoint.normalized;
                Rb.velocity = MoveDirection * patrolSpeed + _hitEffect.KnockbackVelocity;
                break;

            case S.Approach:
                if (dist > orbitRadius)
                {
                    MoveDirection = toPlayer.normalized;
                    Rb.velocity = MoveDirection * approachSpeed + _hitEffect.KnockbackVelocity;
                }
                else
                    EnterOrbit();
                break;

            case S.Orbit:
                MoveDirection = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                Rb.velocity = MoveDirection * orbitSpeed + _hitEffect.KnockbackVelocity;
                _guardTimer -= Time.fixedDeltaTime;
                if (dist <= attackRange || _guardTimer <= 0f)
                    EnterDashPrep();
                break;

            case S.DashPrep:
                Rb.velocity = _hitEffect.KnockbackVelocity;
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f) EnterDashMove();
                break;

            case S.DashMove:
                {                 
                    Vector2 currentPos = (Vector2)transform.position;
                    Vector2 playerPos = (Vector2)_player.position;

                    _dashElapsed += Time.fixedDeltaTime;
                    if (!_dashTargetLocked)
                    {
                        if (_dashElapsed <= _dashDuration * dashTrackingFraction)
                        {                          
                            _dashTarget = playerPos;
                        }
                        else
                        {
                            Vector2 dir = (playerPos - currentPos).normalized;
                            _dashTargetLocked = true;
                            _dashTarget = playerPos - dir * attackRange;
                        }
                    }

                    Vector2 toTarget = _dashTarget - currentPos;
                    Vector2 movement = toTarget.normalized * dashSpeed;
                    MoveDirection = toTarget.normalized;
                    Vector2 futurePos = currentPos + movement * Time.fixedDeltaTime;
                    Rb.velocity = movement + _hitEffect.KnockbackVelocity;

                    if (Vector2.Dot(toTarget, _dashTarget - futurePos) <= 0f)
                        EnterHit();
                }
                break;



            case S.Hit:
                _hitTimer += Time.fixedDeltaTime;
                if (_hitTimer > 1.5f) EnterCooldown(); // failsafe: animation event missed
                break;

            case S.Cooldown:
                Rb.velocity = _hitEffect.KnockbackVelocity;
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f)
                {
                    if (dist <= attackRange)
                        EnterHit();
                    else if (dist > detectionRadius)
                        EnterPatrol();
                    else if (dist > orbitRadius)
                        EnterApproach();
                    else
                        EnterOrbit();
                }
                break;

        }

    
    }

    private void EnterPatrol()
    {
        _state = S.Patrol;
    }

    private void EnterApproach()
    {
        _state = S.Approach;
    }

    private void EnterOrbit()
    {
        _state = S.Orbit;
        ResetGuardTimer();
    }

    private void EnterDashPrep()
    {
        _state = S.DashPrep;
        _timer = dashPrepTime;
    }

    private void EnterDashMove()
    {
        _state = S.DashMove;

        var initialDist = Vector2.Distance(transform.position, _player.position);
        _dashDuration = initialDist / dashSpeed;
        _dashElapsed = 0f;
        _dashTargetLocked = false;

        _dashTarget = _player.position;
    }

    private void EnterHit()
    {
        if (enemyAnim == null || (_health != null && _health.IsDead)) return;
        _state = S.Hit;
        _hitTimer = 0f;
        UpdateFacing((_player.position - transform.position).normalized);
        enemyAnim.PlaySfAttack();
    }

    private void EnterCooldown()
    {
        _state = S.Cooldown;
        _timer = cooldownTime;
    }

    private void PickNewPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        _patrolTarget = _spawnPoint + offset;
    }

    private void ResetGuardTimer() =>
        _guardTimer = Random.Range(guardTimeRange.x, guardTimeRange.y);

    private void UpdateFacing(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        LastDirection = dir;
    }

    public void OnAttackAnimationEnd()
    {
        if (_state == S.Hit)
            EnterCooldown();
    }

    //Precisa ser modularizado
    public void DefineSfSpriteDirection()
    {
        if (_state == S.Hit) return; // keep locked during attack

        float dx = _player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.05f) return; // nearly directly above/below

        var sc = sfVisual.localScale;
        sc.x = dx < 0f ? -1.75f : 1.75f;
        sfVisual.localScale = sc;
    }
}
