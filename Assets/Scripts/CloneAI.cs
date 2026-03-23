using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(GenericEnemyHitEffect))]
public class CloneAI : MonoBehaviour
{
    public enum State
    {
        Idle, MirrorStance, ComboApproach, DashStrike,
        Swinging, Dodge, Cooldown, WallEscape, Phase2Talk, Dead
    }

    private State _state = State.Idle;

    // ── References ───────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private CloneAnimator cloneAnimator;

    // Player attack state detection — set these to the exact Animator state names
    [SerializeField] private string playerAttackState1 = "LightAttack1";
    [SerializeField] private string playerAttackState2 = "LightAttack2";

    [Header("Layers")]
    [SerializeField] private LayerMask wallLayer;

    // ── Tuning ───────────────────────────────────────────────────────────────

    [Header("Distances")]
    [SerializeField] private float attackRange   = 2.5f;
    [SerializeField] private float farDistance   = 6f;
    [SerializeField] private float wallThreshold = 1.5f;

    [Header("Mirror Stance")]
    [SerializeField] private float retreatDuration = 1f;

    [Header("Patience (seconds before attacking)")]
    [SerializeField] private float patienceMin       = 1f;
    [SerializeField] private float patienceMax       = 2.5f;
    [SerializeField] private float phase2PatienceMin = 2f;
    [SerializeField] private float phase2PatienceMax = 4f;

    [Header("Cooldown")]
    [SerializeField] private float cooldownMin       = 0.8f;
    [SerializeField] private float cooldownMax       = 1.2f;
    [SerializeField] private float phase2CooldownMin = 0.5f;
    [SerializeField] private float phase2CooldownMax = 0.7f;

    [Header("Dodge")]
    [SerializeField] private float dodgeChance       = 0.28f;
    [SerializeField] private float phase2DodgeChance = 0.45f;
    [SerializeField] private float dodgeCooldown     = 4f;
    [SerializeField] private float dodgeRange        = 4f;
    [SerializeField] private float dodgeForce        = 14f;
    [SerializeField] private float dodgeDuration     = 0.35f;

    [Header("Wall Escape")]
    [SerializeField] private float wallEscapeForce    = 18f;
    [SerializeField] private float wallEscapeDuration = 0.4f;

    [Header("Phase 2")]
    [SerializeField] private float phase2ExtraDashWeight = 0.2f;

    [Header("Dialogue")]
    [SerializeField] private DialogueData phase2Dialogue;

    // ── Private state ────────────────────────────────────────────────────────

    private Rigidbody2D        _rb;
    private Health             _health;
    private GenericEnemyHitEffect _hitEffect;
    private Transform          _player;
    private Rigidbody2D        _playerRb;
    private Animator           _playerAnimator;

    // Cloned from PlayerStats on Awake
    private float _approachSpeed;
    private float _dashForce;
    private float _attackPushForce;

    private bool _isPhase2;
    private bool _phase2Triggered;

    // Generic timer used by multiple states
    private float _timer;

    // MirrorStance
    private float _patienceTimer;
    private float _retreatTimer;
    private bool  _isRetreating;

    // Dodge cooldown
    private float _dodgeCooldownTimer;

    // Swinging
    private bool _attack2Triggered;

    // Impulse (clone-controlled velocity on top of knockback)
    private Vector2 _impulseVelocity;
    private const float ImpulseDecay = 9f;

    // Facing — used by animator
    private Vector2 _faceDir = Vector2.right;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _health    = GetComponent<Health>();
        _hitEffect = GetComponent<GenericEnemyHitEffect>();

        // Direct velocity control — drag fights us every frame, interpolation prevents visual stutter
        _rb.drag          = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Boss deals damage through attack hitboxes only, not body contact
        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null) contact.enabled = false;


    }

    private void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) { Debug.LogError("CloneAI: No GameObject tagged 'Player' found!"); return; }
        _player         = playerObj.transform;
        _playerRb       = _player.GetComponent<Rigidbody2D>();
        _playerAnimator = _player.GetComponentInChildren<Animator>();

        // Clone movement stats from player
        var ps           = PlayerStats.Instance;
        _approachSpeed   = ps.speed;
        _dashForce       = ps.dashForce;
        _attackPushForce = ps.attackPushForce;

        _health.OnDeath += OnDeath;
    }


    private void OnDestroy()
    {
        if (_health != null) _health.OnDeath -= OnDeath;
    }

    private void FixedUpdate()
    {
        if (_state == State.Dead) return;

        // Phase 2 trigger
        if (!_phase2Triggered && _health.currentHp <= _health.MaxHP / 2)
        {
            _phase2Triggered = true;
            EnterPhase2Talk();
            return;
        }


        // Wall escape — interrupts most states
        if (_state != State.Swinging  && _state != State.Dodge &&
            _state != State.Phase2Talk && _state != State.WallEscape &&
            _state != State.Idle)
        {
            if (IsNearWall())
            {
                EnterWallEscape();
                return;
            }
        }

        // Decay impulse and dodge cooldown
        _impulseVelocity    = Vector2.Lerp(_impulseVelocity, Vector2.zero, Time.fixedDeltaTime * ImpulseDecay);
        _dodgeCooldownTimer = Mathf.Max(0f, _dodgeCooldownTimer - Time.fixedDeltaTime);

        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        float   dist     = toPlayer.magnitude;

        switch (_state)
        {
            case State.Idle:         break;
            case State.MirrorStance: UpdateMirrorStance(toPlayer, dist); break;
            case State.ComboApproach:UpdateComboApproach(toPlayer, dist); break;
            case State.DashStrike:   UpdateDashStrike(toPlayer, dist);   break;
            case State.Swinging:     UpdateSwinging(toPlayer);           break;
            case State.Dodge:        UpdateDodge();                      break;
            case State.Cooldown:     UpdateCooldown();                   break;
            case State.WallEscape:   UpdateWallEscape();                 break;
        }

        // Push knockback from hit effect into final velocity
        _rb.velocity += _hitEffect.KnockbackVelocity;

        UpdateAnimator();
    }

    // ── State Updates ────────────────────────────────────────────────────────

    private void UpdateMirrorStance(Vector2 toPlayer, float dist)
    {
        _patienceTimer -= Time.fixedDeltaTime;
        if (_patienceTimer <= 0f)
        {
            ChooseAttack(toPlayer);
            return;
        }

        if (ShouldDodge(dist))
        {
            EnterDodge();
            return;
        }

        // Read player rush — retreat briefly then commit
        float rushDot = Vector2.Dot(_playerRb.velocity.normalized, -toPlayer.normalized);
        if (rushDot > 0.5f && !_isRetreating)
        {
            _isRetreating = true;
            _retreatTimer = retreatDuration;
        }

        Vector2 velocity;
        _faceDir = toPlayer.normalized;

        if (_isRetreating)
        {
            _retreatTimer -= Time.fixedDeltaTime;
            if (_retreatTimer <= 0f)
            {
                _isRetreating = false;
                ChooseAttack(toPlayer);
                return;
            }
            velocity = -toPlayer.normalized * (_approachSpeed * 0.55f);
        }
        else
        {
            velocity = toPlayer.normalized * (_approachSpeed * 0.5f);
        }

        _rb.velocity = velocity + _impulseVelocity;
    }

    private void UpdateComboApproach(Vector2 toPlayer, float dist)
    {
        if (dist <= attackRange)
        {
            EnterSwinging(toPlayer);
            return;
        }

        _faceDir     = toPlayer.normalized;
        _rb.velocity = toPlayer.normalized * _approachSpeed + _impulseVelocity;
    }

    private void UpdateDashStrike(Vector2 toPlayer, float dist)
    {
        if (dist <= attackRange)
        {
            EnterSwinging(toPlayer);
            return;
        }

        _rb.velocity = _impulseVelocity;

        // Impulse exhausted and still not in range — give up
        if (_impulseVelocity.magnitude < 1.5f)
            EnterCooldown();
    }

    private void UpdateSwinging(Vector2 toPlayer)
    {
        _rb.velocity = _impulseVelocity;

        // Wait for animator to open the combo window, then fire attack 2
        if (!_attack2Triggered && cloneAnimator.Attack2WindowOpen)
        {
            _attack2Triggered = true;
            // Second lunge — slightly weaker than first
            _impulseVelocity += toPlayer.normalized * (_attackPushForce * 0.6f);
            cloneAnimator.TriggerAttack2();
        }

        if (cloneAnimator.ComboFinished)
        {
            cloneAnimator.ResetComboFinished();
            EnterCooldown();
        }
    }

    private void UpdateDodge()
    {
        _rb.velocity = _impulseVelocity;
        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0f)
        {
            _impulseVelocity = Vector2.zero;
            EnterMirrorStance();
        }
    }

    private void UpdateCooldown()
    {
        _rb.velocity = _impulseVelocity;
        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0f)
            EnterMirrorStance();
    }

    private void UpdateWallEscape()
    {
        _rb.velocity = _impulseVelocity;
        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0f)
        {
            _impulseVelocity = Vector2.zero;
            EnterMirrorStance();
        }
    }

    // ── Enter States ─────────────────────────────────────────────────────────

    public void StartFight() => EnterMirrorStance();

    private void EnterMirrorStance()
    {
        _state        = State.MirrorStance;
        _isRetreating = false;
        float pMin    = _isPhase2 ? phase2PatienceMin : patienceMin;
        float pMax    = _isPhase2 ? phase2PatienceMax : patienceMax;
        _patienceTimer = Random.Range(pMin, pMax);
    }

    private void EnterComboApproach()
    {
        _state = State.ComboApproach;
    }

    private void EnterDashStrike(Vector2 toPlayer)
    {
        _state           = State.DashStrike;
        _faceDir         = toPlayer.normalized;
        _impulseVelocity = toPlayer.normalized * _dashForce;
    }

    private void EnterSwinging(Vector2 toPlayer)
    {
        _state            = State.Swinging;
        _attack2Triggered = false;
        _faceDir          = toPlayer.normalized;
        _impulseVelocity  = toPlayer.normalized * _attackPushForce;
        cloneAnimator.TriggerAttack1();
    }

    private void EnterDodge()
    {
        _state              = State.Dodge;
        _timer              = dodgeDuration;
        _dodgeCooldownTimer = dodgeCooldown;

        Vector2 escDir   = GetBestDodgeDiagonal();
        _faceDir         = -escDir; // still face player while dodging
        _impulseVelocity = escDir * dodgeForce;
    }

    private void EnterCooldown()
    {
        _state       = State.Cooldown;
        _rb.velocity = Vector2.zero;
        float cMin   = _isPhase2 ? phase2CooldownMin : cooldownMin;
        float cMax   = _isPhase2 ? phase2CooldownMax : cooldownMax;
        _timer       = Random.Range(cMin, cMax);
    }

    private void EnterWallEscape()
    {
        _state           = State.WallEscape;
        _timer           = wallEscapeDuration;
        Vector2 open     = GetMostOpenDirection();
        _faceDir         = open;
        _impulseVelocity = open * wallEscapeForce;
    }

    private void EnterPhase2Talk()
    {
        _state       = State.Phase2Talk;
        _rb.velocity = Vector2.zero;
        _impulseVelocity = Vector2.zero;
        StartCoroutine(Phase2TalkRoutine());
    }

    private IEnumerator Phase2TalkRoutine()
    {
        if (phase2Dialogue != null && DialogueManager.Instance != null)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(phase2Dialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        _isPhase2 = true;
        EnterMirrorStance();
    }

    private void OnDeath()
    {
        _state           = State.Dead;
        _rb.velocity     = Vector2.zero;
        _impulseVelocity = Vector2.zero;

        // Prevent dead body from dealing contact damage or blocking movement
        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null) contact.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        cloneAnimator.TriggerDeath();

        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.Hide();

        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        // Wait for death animation to play out before showing victory
        yield return new WaitForSeconds(2f);
        UIManager.Instance.ShowVictory();
    }

    // ── Decision Logic ───────────────────────────────────────────────────────

    private void ChooseAttack(Vector2 toPlayer)
    {
        float dashWeight = _isPhase2 ? 0.4f + phase2ExtraDashWeight : 0.4f;

        // Player retreating → punish with dash
        if (_playerRb != null)
        {
            float retreatDot = Vector2.Dot(_playerRb.velocity.normalized, -toPlayer.normalized);
            if (retreatDot > 0.4f)
                dashWeight = Mathf.Min(dashWeight + 0.3f, 0.85f);
        }

        if (Random.value < dashWeight)
            EnterDashStrike(toPlayer);
        else
            EnterComboApproach();
    }

    private bool ShouldDodge(float dist)
    {
        if (_dodgeCooldownTimer > 0f) return false;
        if (dist > dodgeRange)        return false;
        if (!IsPlayerAttacking())     return false;
        float chance = _isPhase2 ? phase2DodgeChance : dodgeChance;
        return Random.value < chance;
    }

    private bool IsPlayerAttacking()
    {
        if (_playerAnimator == null) return false;
        var info = _playerAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(playerAttackState1) || info.IsName(playerAttackState2);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Only tilemap geometry counts as a wall
    private static bool IsWall(RaycastHit2D hit) =>
        hit.collider != null &&
        (hit.collider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null ||
         hit.collider.GetComponent<CompositeCollider2D>() != null);

    private bool IsNearWall()
    {
        Vector2[] dirs =
        {
            Vector2.right, Vector2.left, Vector2.up, Vector2.down,
            new Vector2( 1,  1).normalized, new Vector2(-1,  1).normalized,
            new Vector2( 1, -1).normalized, new Vector2(-1, -1).normalized
        };
        foreach (var d in dirs)
            if (IsWall(Physics2D.Raycast(transform.position, d, wallThreshold, wallLayer)))
                return true;
        return false;
    }

    private Vector2 GetMostOpenDirection()
    {
        Vector2[] dirs =
        {
            Vector2.right, Vector2.left, Vector2.up, Vector2.down,
            new Vector2( 1,  1).normalized, new Vector2(-1,  1).normalized,
            new Vector2( 1, -1).normalized, new Vector2(-1, -1).normalized
        };
        Vector2 best    = Vector2.right;
        float   maxDist = 0f;
        foreach (var d in dirs)
        {
            var hit = Physics2D.Raycast(transform.position, d, 10f, wallLayer);
            float dist = IsWall(hit) ? hit.distance : 10f;
            if (dist > maxDist) { maxDist = dist; best = d; }
        }
        return best;
    }

    private Vector2 GetBestDodgeDiagonal()
    {
        Vector2 toPlayer = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Vector2[] diags  =
        {
            new Vector2( 1,  1).normalized, new Vector2(-1,  1).normalized,
            new Vector2( 1, -1).normalized, new Vector2(-1, -1).normalized
        };
        Vector2 best      = diags[0];
        float   bestScore = float.MinValue;
        foreach (var d in diags)
        {
            var hit       = Physics2D.Raycast(transform.position, d, 10f, wallLayer);
            float clearance = IsWall(hit) ? hit.distance : 10f;
            float awayDot   = Vector2.Dot(d, -toPlayer);
            float score     = clearance + awayDot * 3f;
            if (score > bestScore) { bestScore = score; best = d; }
        }
        return best;
    }

    // ── Animator ─────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        bool moving = _state == State.MirrorStance  ||
                      _state == State.ComboApproach ||
                      _state == State.DashStrike    ||
                      _state == State.WallEscape    ||
                      _state == State.Dodge;
        cloneAnimator.SetWalking(moving);
        if (_faceDir != Vector2.zero)
            cloneAnimator.SetDirection(_faceDir);
    }
}
