using UnityEngine;

/// <summary>
/// Skeleton Archer AI. Keeps distance from player, strafes, fires arrows on an interval.
/// Backs away if player gets too close.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonArcherAI : MonoBehaviour
{
    public enum S { Patrol, Combat, Shoot, Cooldown }
    private S _state = S.Patrol;
    public S CurrentState => _state;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 12f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float patrolWaitTime = 1.5f;
    [SerializeField] private float waypointThreshold = 0.4f;

    [Header("Combat Movement")]
    [SerializeField] private float preferredRange = 8f;  // ideal distance from player
    [SerializeField] private float tooCloseRange = 5f;   // run away if closer than this
    [SerializeField] private float repositionSpeed = 4f;

    [Header("Shooting")]
    [SerializeField] private float shootInterval = 2.5f;
    [SerializeField] private float shootWindUpTime = 0.4f;
    [SerializeField] private float cooldownTime = 0.8f;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Refs")]
    [SerializeField] private SkeletonArcherAnimator archerAnimator;

    private Rigidbody2D _rb;
    private Transform _player;
    private Health _health;
    private GenericEnemyHitEffect _hitEffect;

    private Vector2 _spawnPoint;
    private Vector2 _patrolTarget;
    private float _patrolWaitTimer;
    private float _shootTimer;
    private float _stateTimer;
    private bool _isDead;

    public bool IsMoving => _rb.velocity.magnitude > 0.2f;
    public Vector2 MoveDirection { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _health = GetComponent<Health>();
        _hitEffect = GetComponent<GenericEnemyHitEffect>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _spawnPoint = transform.position;
        PickNewPatrolTarget();
        _shootTimer = shootInterval;
    }

    private void OnEnable()
    {
        if (_health != null) _health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (_health != null) _health.OnDeath -= OnDeath;
    }

    private void FixedUpdate()
    {
        if (_isDead || _player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        Vector2 toPlayer = (Vector2)(_player.position - transform.position);
        Vector2 knockback = _hitEffect != null ? _hitEffect.KnockbackVelocity : Vector2.zero;

        switch (_state)
        {
            case S.Patrol:
                if (dist <= detectionRadius)
                {
                    _state = S.Combat;
                    _shootTimer = shootInterval;
                    break;
                }
                UpdatePatrol(knockback);
                break;

            case S.Combat:
                if (dist > detectionRadius * 1.3f) { _state = S.Patrol; break; }

                _shootTimer -= Time.fixedDeltaTime;
                if (_shootTimer <= 0f) { EnterShoot(); break; }

                UpdateCombatMovement(dist, toPlayer, knockback);
                break;

            case S.Shoot:
                _rb.velocity = knockback;
                _stateTimer -= Time.fixedDeltaTime;
                if (_stateTimer <= 0f)
                {
                    FireArrow(toPlayer.normalized);
                    EnterCooldown();
                }
                break;

            case S.Cooldown:
                _rb.velocity = knockback;
                _stateTimer -= Time.fixedDeltaTime;
                if (_stateTimer <= 0f) _state = S.Combat;
                break;
        }
    }

    private void UpdatePatrol(Vector2 knockback)
    {
        if (_patrolWaitTimer > 0f)
        {
            _patrolWaitTimer -= Time.fixedDeltaTime;
            _rb.velocity = knockback;
            return;
        }

        Vector2 toWaypoint = _patrolTarget - (Vector2)transform.position;
        if (toWaypoint.magnitude <= waypointThreshold)
        {
            _patrolWaitTimer = patrolWaitTime;
            PickNewPatrolTarget();
            return;
        }

        MoveDirection = toWaypoint.normalized;
        _rb.velocity = MoveDirection * patrolSpeed + knockback;
    }

    private void UpdateCombatMovement(float dist, Vector2 toPlayer, Vector2 knockback)
    {
        if (dist < tooCloseRange)
        {
            // Back away from player
            MoveDirection = -toPlayer.normalized;
            _rb.velocity = MoveDirection * repositionSpeed + knockback;
        }
        else if (dist > preferredRange)
        {
            // Approach to preferred range
            MoveDirection = toPlayer.normalized;
            _rb.velocity = MoveDirection * repositionSpeed + knockback;
        }
        else
        {
            // Strafe sideways to avoid being a sitting duck
            Vector2 strafe = new Vector2(-toPlayer.y, toPlayer.x).normalized;
            MoveDirection = strafe;
            _rb.velocity = strafe * (repositionSpeed * 0.5f) + knockback;
        }
    }

    private void EnterShoot()
    {
        _state = S.Shoot;
        _stateTimer = shootWindUpTime;
        archerAnimator?.PlayShoot();
    }

    private void FireArrow(Vector2 direction)
    {
        if (arrowPrefab == null) return;
        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        var arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<ArrowProjectile>()?.Init(direction);
    }

    private void EnterCooldown()
    {
        _state = S.Cooldown;
        _stateTimer = cooldownTime;
        _shootTimer = shootInterval;
    }

    private void OnDeath()
    {
        _isDead = true;
    }

    private void PickNewPatrolTarget()
    {
        _patrolTarget = _spawnPoint + Random.insideUnitCircle * patrolRadius;
    }

    public void ScaleForNGPlus(float m)
    {
        detectionRadius  *= m;
        patrolRadius     *= m;
        patrolSpeed      *= m;
        waypointThreshold *= m;
        preferredRange   *= m;
        tooCloseRange    *= m;
        repositionSpeed  *= m;
        shootInterval    *= m;
        shootWindUpTime  *= m;
        // patrolWaitTime and cooldownTime intentionally excluded
    }
}
