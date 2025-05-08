using UnityEngine;

public class SkeletonFighter : Enemy
{
    private enum State { Guard, Attack, Cooldown }
    private State _state = State.Guard;

    private bool _isWalking;
    
    [Header("Guard")]
    [SerializeField] private float guardRadius = 2.5f;
    [SerializeField] private float guardSpeed  = 1.6f;
    [SerializeField] private Vector2 guardTime = new Vector2(1.5f, 3f);

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private GameObject swordHitbox; 

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime  = 0.9f;

    private EnemyAnimator _enemyAnim;

    // ── Internos ──
    private float _timer;      
    private float _atkTimer;   
    public Rigidbody2D _SfRb {get ; private set;}
    private Transform _player;

    protected override void Awake()
    {
        _enemyAnim = GetComponentInChildren<EnemyAnimator>();
        _SfRb = GetComponent<Rigidbody2D>();
        _player = GameObject.FindWithTag("Player").transform;
        
        swordHitbox.SetActive(false);
        
        // prepara o primeiro Guard
        _timer  = Random.Range(guardTime.x, guardTime.y);
    }

    private void FixedUpdate()
    {
         _isWalking = _SfRb.velocity != Vector2.zero;
        
        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;

        switch (_state)
        {
            // ── GUARD ──
            case State.Guard:
                // aproxima ou orbita
                if (toPlayer.sqrMagnitude > guardRadius * guardRadius)
                    _SfRb.velocity = toPlayer.normalized * guardSpeed;
                else
                {
                    Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                    _SfRb.velocity = tangent * guardSpeed;
                }

                // entra em Attack se estiver no alcance ou o timer zerar
                _timer -= Time.fixedDeltaTime;
                if (toPlayer.sqrMagnitude <= attackRange * attackRange || _timer <= 0f)
                    StartAttack();
                break;

            // ── ATTACK ──
            case State.Attack:
                _SfRb.velocity = Vector2.zero;
                _atkTimer -= Time.fixedDeltaTime;
                if (_atkTimer <= 0f)
                {
                    swordHitbox.SetActive(false);
                    _state = State.Cooldown;
                    _timer = cooldownTime;
                }
                break;

            // ── COOLDOWN ──
            case State.Cooldown:
                _SfRb.velocity = Vector2.zero;
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f)
                {
                    // se ainda em range, ataca de novo; senão, volta a Guard
                    if (toPlayer.sqrMagnitude <= attackRange * attackRange)
                        StartAttack();
                    else
                        EnterGuard();
                }
                break;
        }
    }

    private void StartAttack()
    {
        _state = State.Attack;
        _atkTimer = attackDuration;
        _enemyAnim.PlaySfAttack();
        swordHitbox.SetActive(true);
        
    }

    private void EnterGuard()
    {
        _state = State.Guard;
        _timer = Random.Range(guardTime.x, guardTime.y);
    }
    
    public bool IsWalking()
    {
        return _isWalking;
    }
}
