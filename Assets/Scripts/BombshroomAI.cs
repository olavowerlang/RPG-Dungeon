using UnityEngine;

/// <summary>
/// Bombshroom enemy AI.
/// Patrols → detects player → approaches → winds up → releases toxic gas → cooldown → repeat.
/// On death, releases a final larger gas burst.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BombshroomAI : MonoBehaviour
{
    public enum S { Disguised, PopOut, Patrol, Approach, WindUp, Cooldown }
    private S _state = S.Patrol;
    public S CurrentState => _state;

    [Header("Ambush")]
    public bool isAmbush = false;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float ambushDetectionRadius = 3f;
    [SerializeField] private float gasRange = 4f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float waypointThreshold = 0.4f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Gas Attack")]
    [SerializeField] private float cooldownTime = 4f;
    [SerializeField] private GameObject gasCloudPrefab;

    [Header("Death Burst")]
    [SerializeField] private GameObject deathGasCloudPrefab;

    [Header("Refs")]
    [SerializeField] private BombshroomAnimator bombshroomAnimator;

    private Rigidbody2D _rb;
    private Transform _player;
    private Health _health;
    private GenericEnemyHitEffect _hitEffect;

    private Vector2 _spawnPoint;
    private Vector2 _patrolTarget;
    private float _patrolWaitTimer;
    private float _patrolMoveTimer;
    private float _timer;
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
        if (isAmbush) _state = S.Disguised;
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

        MoveDirection = Vector2.zero;
        float dist = Vector2.Distance(transform.position, _player.position);
        Vector2 knockback = _hitEffect != null ? _hitEffect.KnockbackVelocity : Vector2.zero;

        switch (_state)
        {
            case S.Disguised:
                _rb.velocity = Vector2.zero;
                if (dist <= ambushDetectionRadius)
                {
                    _state = S.PopOut;
                    bombshroomAnimator?.PlayPopOut();
                }
                break;

            case S.PopOut:
                _rb.velocity = Vector2.zero;
                break;

            case S.Patrol:
                if (dist <= detectionRadius) { _state = S.Approach; break; }
                UpdatePatrol(knockback);
                break;

            case S.Approach:
                if (dist > detectionRadius * 1.3f) { _state = S.Patrol; break; }
                if (dist <= gasRange) { EnterWindUp(); break; }
                Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                MoveDirection = dir;
                _rb.velocity = dir * moveSpeed + knockback;
                break;

            case S.WindUp:
                _rb.velocity = knockback;
                break;

            case S.Cooldown:
                _rb.velocity = knockback;
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f)
                    _state = dist <= detectionRadius ? S.Approach : S.Patrol;
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
        _patrolMoveTimer += Time.fixedDeltaTime;

        if (toWaypoint.magnitude <= waypointThreshold || _patrolMoveTimer >= 3f)
        {
            _patrolWaitTimer = patrolWaitTime;
            PickNewPatrolTarget();
            return;
        }

        MoveDirection = toWaypoint.normalized;
        _rb.velocity = MoveDirection * patrolSpeed + knockback;
    }

    public void OnPopOutComplete()
    {
        _state = S.Approach;
    }

    private void EnterWindUp()
    {
        _state = S.WindUp;
        _rb.velocity = Vector2.zero;
        bombshroomAnimator?.PlayWindUp();
    }

    // Called by animation event at end of WindUp animation
    public void OnWindUpComplete()
    {
        if (gasCloudPrefab != null)
        {
            Vector2 spawnPos = ((Vector2)transform.position + (Vector2)_player.position) * 0.5f;
            Instantiate(gasCloudPrefab, spawnPos, Quaternion.identity);
        }

        _state = S.Cooldown;
        _timer = cooldownTime;
    }

    private void OnDeath()
    {
        if (_isDead) return;
        _isDead = true;
    }

    public void SpawnDeathGas()
    {
        if (deathGasCloudPrefab != null)
            Instantiate(deathGasCloudPrefab, transform.position, Quaternion.identity);
    }

    private void PickNewPatrolTarget()
    {
        _patrolTarget = _spawnPoint + Random.insideUnitCircle * patrolRadius;
        _patrolMoveTimer = 0f;
    }
}
