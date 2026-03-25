using UnityEngine;

/// <summary>
/// Mini Slime spawned when a Slime dies. Chases player aggressively. No split on death.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MiniSlimeAI : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float detectionRadius = 14f; // large so mini slimes always chase
    [SerializeField] private float hopForce = 10f;
    [SerializeField] private float hopInterval = 1f;

    private Rigidbody2D _rb;
    private Transform _player;
    private Health _health;
    private GenericEnemyHitEffect _hitEffect;

    private float _hopTimer;
    private float _spawnWait = 0.5f;
    private bool _isDead;

    public bool IsMoving => _rb.velocity.magnitude > 0.3f;
    public Vector2 MoveDirection { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _health = GetComponent<Health>();
        _hitEffect = GetComponent<GenericEnemyHitEffect>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _hopTimer = Random.Range(0f, 0.3f); // stagger so twins don't sync
        _hitEffect?.GrantInvulnerability(0.5f);
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

        Vector2 knockback = _hitEffect != null ? _hitEffect.KnockbackVelocity : Vector2.zero;
        float dist = Vector2.Distance(transform.position, _player.position);

        if (_spawnWait > 0f)
        {
            _spawnWait -= Time.fixedDeltaTime;
            _rb.velocity = knockback;
            return;
        }

        if (dist > detectionRadius)
        {
            _rb.velocity = knockback;
            return;
        }

        _hopTimer -= Time.fixedDeltaTime;

        if (_hopTimer <= 0f)
        {
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            MoveDirection = dir;
            _rb.velocity = dir * hopForce + knockback;
            _hopTimer = hopInterval;
        }
        else
        {
            _rb.velocity = _rb.velocity * (1f - Time.fixedDeltaTime * 6f) + knockback;
        }
    }

    private void OnDeath()
    {
        _isDead = true;
    }
}
