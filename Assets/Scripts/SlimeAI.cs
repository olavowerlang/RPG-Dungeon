using UnityEngine;

/// <summary>
/// Slime enemy AI. Patrols until player detected, then hops toward them.
/// On death, spawns 2 MiniSlimes.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SlimeAI : MonoBehaviour
{
    public enum S { Patrol, Chase }
    private S _state = S.Patrol;
    public S CurrentState => _state;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 8f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointThreshold = 0.4f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Hop")]
    [SerializeField] private float hopForce = 12f;
    [SerializeField] private float hopInterval = 1.2f; // seconds between hops

    [Header("Split on Death")]
    [SerializeField] private GameObject miniSlimePrefab;

    private Rigidbody2D _rb;
    private Transform _player;
    private Health _health;
    private GenericEnemyHitEffect _hitEffect;

    private Vector2 _spawnPoint;
    private Vector2 _patrolTarget;
    private float _patrolWaitTimer;
    private float _hopTimer;
    private bool _isDead;

    public bool IsMoving => _rb.velocity.magnitude > 0.3f;
    public Vector2 MoveDirection { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _hitEffect = GetComponent<GenericEnemyHitEffect>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _spawnPoint = transform.position;
        PickNewPatrolTarget();
        _hopTimer = 0f; // hop immediately on detection
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
        Vector2 knockback = _hitEffect != null ? _hitEffect.KnockbackVelocity : Vector2.zero;

        switch (_state)
        {
            case S.Patrol:
                if (dist <= detectionRadius)
                {
                    _state = S.Chase;
                    _hopTimer = 0f; // hop immediately
                    break;
                }
                UpdatePatrol(knockback);
                break;

            case S.Chase:
                if (dist > detectionRadius * 1.5f)
                {
                    _state = S.Patrol;
                    break;
                }
                UpdateChase(knockback);
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

    private void UpdateChase(Vector2 knockback)
    {
        _hopTimer -= Time.fixedDeltaTime;

        if (_hopTimer <= 0f)
        {
            Vector2 hopDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            MoveDirection = hopDir;
            _rb.velocity = hopDir * hopForce + knockback;
            _hopTimer = hopInterval;
        }
        else
        {
            // Decelerate between hops
            _rb.velocity = _rb.velocity * (1f - Time.fixedDeltaTime * 6f) + knockback;
        }
    }

    private void OnDeath()
    {
        if (_isDead) return;
        _isDead = true;

        if (miniSlimePrefab != null)
        {
            Instantiate(miniSlimePrefab, (Vector2)transform.position + new Vector2(-0.5f, 0.3f), Quaternion.identity);
            Instantiate(miniSlimePrefab, (Vector2)transform.position + new Vector2(0.5f, 0.3f), Quaternion.identity);
        }
    }

    private void PickNewPatrolTarget()
    {
        _patrolTarget = _spawnPoint + Random.insideUnitCircle * patrolRadius;
    }
}
