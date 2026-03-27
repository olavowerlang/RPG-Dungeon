using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(GenericEnemyHitEffect))]
public class CloneAI : MonoBehaviour
{
    public enum State
    {
        Idle, MirrorStance, ComboApproach, DashStrike,
        Swinging, Dodge, Cooldown, WallEscape, Phase2Talk, Dead
    }

    private State _state = State.Idle;
    public State CurrentState => _state;

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
    [SerializeField] private float retreatDuration    = 0.6f;
    [SerializeField] private float preferredDistance  = 3.5f; // ideal gap to maintain
    [SerializeField] private float rushSpeedThreshold = 2.5f; // player must move this fast to trigger retreat

    [Header("Patience (seconds before attacking)")]
    [SerializeField] private float patienceMin       = 0.8f;
    [SerializeField] private float patienceMax       = 1.8f;
    [SerializeField] private float phase2PatienceMin = 0.5f;
    [SerializeField] private float phase2PatienceMax = 1.2f;

    [Header("Cooldown")]
    [SerializeField] private float cooldownMin       = 0.6f;
    [SerializeField] private float cooldownMax       = 1.0f;
    [SerializeField] private float phase2CooldownMin = 0.3f;
    [SerializeField] private float phase2CooldownMax = 0.55f;

    [Header("Dodge")]
    [SerializeField] private float dodgeChance       = 0.35f;
    [SerializeField] private float phase2DodgeChance = 0.55f;
    [SerializeField] private float dodgeCooldown     = 2.5f;
    [SerializeField] private float dodgeRange        = 4f;
    [SerializeField] private float dodgeForce        = 14f;
    [SerializeField] private float dodgeDuration     = 0.35f;

    [Header("Wall Escape")]
    [SerializeField] private float wallEscapeForce    = 18f;
    [SerializeField] private float wallEscapeDuration = 0.4f;

    [Header("Phase 2")]
    [SerializeField] private float phase2ExtraDashWeight = 0.3f;

    [Header("Dialogue")]
    [SerializeField] private DialogueData phase2Dialogue;
    [SerializeField] private DialogueData deathDialogue;

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

    // MirrorStance strafe
    private float   _strafeSide = 1f; // +1 or -1 — orbital direction, flipped on timer
    private float   _strafeFlipTimer;


    // Dodge cooldown
    private float _dodgeCooldownTimer;

    // Wall escape cooldown — prevents rapid re-entry that causes jitter
    private float _wallEscapeCooldown;

    // Feint / approach style
    private bool  _isFeinting;
    private float _feintStopDist;
    private bool  _curvedApproach;

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

        _rb.bodyType               = RigidbodyType2D.Dynamic;
        _rb.drag                   = 0f;
        _rb.gravityScale           = 0f;
        _rb.interpolation          = RigidbodyInterpolation2D.Interpolate;
        _rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        // Boss deals damage through attack hitboxes only, not body contact
        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null) contact.enabled = false;

        // Fallback: find CloneAnimator in children if Inspector reference is missing
        if (cloneAnimator == null)
            cloneAnimator = GetComponentInChildren<CloneAnimator>(true);
    }

    private CloneFightingLines _fightingLines;

    private void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) { Debug.LogError("CloneAI: No GameObject tagged 'Player' found!"); return; }
        _player         = playerObj.transform;
        _playerRb       = _player.GetComponent<Rigidbody2D>();
        _playerAnimator = _player.GetComponentInChildren<Animator>();

        _fightingLines = GetComponent<CloneFightingLines>();
        _health.OnDeath += OnDeath;
        _hitEffect.OnHitTaken += OnCloneHit;

        // NG+ health scaling (stats are mirrored from player so no AI tuning needed)
        if (NGPlusManager.Instance != null && NGPlusManager.Instance.IsNGPlus)
            _health.ScaleMaxHP(Mathf.RoundToInt(_health.MaxHP * NGPlusManager.Instance.EnemyStatMultiplier));

        // Cinemachine SmartUpdate defaults to FixedUpdate for physics targets, making the
        // camera jump at 50Hz while Rigidbody2D Interpolate renders at 60fps → clone jitters
        // relative to camera. LateUpdate makes the camera read interpolated positions → smooth.
        var brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain != null) brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;

        // Prevent the physics engine from generating separation impulses between clone
        // and player bodies — that constant push/override cycle is the main jitter source.
        IgnorePlayerColliders();
    }

    private void IgnorePlayerColliders()
    {
        // Use InChildren so child-object colliders are included on both sides
        var myColliders = GetComponentsInChildren<Collider2D>(true);
        if (myColliders.Length == 0) return;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        var playerColliders = playerObj.GetComponentsInChildren<Collider2D>(true);
        foreach (var pc in playerColliders)
        {
            if (pc.isTrigger) continue; // keep hitbox triggers working
            foreach (var mc in myColliders)
            {
                if (mc.isTrigger) continue;
                Physics2D.IgnoreCollision(mc, pc, true);
            }
        }
    }

    // Called by TransformationSequence instead of StartFight directly,
    // so stats are read after NGPlusCarryOver has applied them.
    private void SnapshotPlayerStats()
    {
        var ps           = PlayerStats.Instance;
        if (ps == null) return;
        _approachSpeed   = ps.speed;
        _dashForce       = ps.dashForce;
        _attackPushForce = ps.attackPushForce;
    }


    private void OnDestroy()
    {
        if (_health    != null) _health.OnDeath         -= OnDeath;
        if (_hitEffect != null) _hitEffect.OnHitTaken   -= OnCloneHit;
    }

    private void OnCloneHit()
    {
        Vector2 imp = _hitEffect.LastHitImpulse;
        if (imp.magnitude > 6f) imp = imp.normalized * 6f;
        _impulseVelocity += imp;
        AudioManager.Instance?.PlayHitImpact();
    }

    private void FixedUpdate()
    {
        if (_state == State.Dead || _state == State.Idle) return;

        // Phase 2 trigger
        if (!_phase2Triggered && _health.currentHp <= _health.MaxHP / 2)
        {
            _phase2Triggered = true;
            EnterPhase2Talk();
            return;
        }


        // Wall escape — interrupts most states, but not if recently escaped (prevents jitter loop)
        if (_wallEscapeCooldown <= 0f &&
            _state != State.Swinging  && _state != State.Dodge &&
            _state != State.Phase2Talk && _state != State.WallEscape &&
            _state != State.Idle)
        {
            if (IsNearWall())
            {
                EnterWallEscape();
                return;
            }
        }

        // Decay timers
        _impulseVelocity = Vector2.Lerp(_impulseVelocity, Vector2.zero, Time.fixedDeltaTime * ImpulseDecay);
        if (_impulseVelocity.magnitude < 0.05f) _impulseVelocity = Vector2.zero;
        _dodgeCooldownTimer    = Mathf.Max(0f, _dodgeCooldownTimer    - Time.fixedDeltaTime);
        _wallEscapeCooldown    = Mathf.Max(0f, _wallEscapeCooldown    - Time.fixedDeltaTime);

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

        UpdateAnimator();
    }

    // ── State Updates ────────────────────────────────────────────────────────

    private void UpdateMirrorStance(Vector2 toPlayer, float dist)
    {
        // If player is in attack range, don't wait — commit immediately
        if (dist <= attackRange)
        {
            ChooseAttack(toPlayer);
            return;
        }

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

        _faceDir = toPlayer.normalized;

        // Only retreat if player is actually rushing (speed check prevents slow-walk triggering it)
        // and clone isn't already far away
        if (!_isRetreating && _playerRb != null)
        {
            float playerSpeed = _playerRb.velocity.magnitude;
            float rushDot     = Vector2.Dot(_playerRb.velocity.normalized, toPlayer.normalized);
            if (playerSpeed > rushSpeedThreshold && rushDot > 0.5f && dist < preferredDistance * 1.5f)
            {
                _isRetreating = true;
                _retreatTimer = retreatDuration;
            }
        }

        Vector2 velocity;

        if (_isRetreating)
        {
            _retreatTimer -= Time.fixedDeltaTime;
            if (_retreatTimer <= 0f)
            {
                _isRetreating = false;
                ChooseAttack(toPlayer);
                return;
            }
            velocity = -toPlayer.normalized * (_approachSpeed * 0.45f);
        }
        else
        {
            // Spring distance controller — no discrete zones, no boundary oscillation.
            // Radial component smoothly pulls clone toward preferredDistance.
            // Lateral component orbits the player. Combined: a stable spiral-to-orbit.
            float distError   = dist - preferredDistance;
            float radialSpeed = Mathf.Clamp(distError * 3f, -_approachSpeed * 0.65f, _approachSpeed * 0.65f);

            _strafeFlipTimer -= Time.fixedDeltaTime;
            if (_strafeFlipTimer <= 0f)
            {
                _strafeSide      = Random.value > 0.5f ? 1f : -1f;
                _strafeFlipTimer = Random.Range(1.5f, 3f);
            }
            Vector2 perpDir = new Vector2(-toPlayer.normalized.y, toPlayer.normalized.x);
            velocity = perpDir * (_strafeSide * _approachSpeed * 0.28f)
                     + toPlayer.normalized * radialSpeed;
        }

        _rb.velocity = velocity + _impulseVelocity;
    }

    private void UpdateComboApproach(Vector2 toPlayer, float dist)
    {
        if (_isFeinting && dist <= _feintStopDist)
        {
            // Feint complete — disengage back to stance without attacking
            _isFeinting = false;
            EnterMirrorStance();
            return;
        }

        if (!_isFeinting && dist <= attackRange)
        {
            EnterSwinging(toPlayer);
            return;
        }

        Vector2 dir;
        if (_curvedApproach)
        {
            Vector2 perp = new Vector2(-toPlayer.normalized.y, toPlayer.normalized.x) * _strafeSide;
            dir = (toPlayer.normalized + perp * 0.35f).normalized;
        }
        else
        {
            dir = toPlayer.normalized;
        }

        _faceDir     = toPlayer.normalized;
        _rb.velocity = dir * _approachSpeed + _impulseVelocity;
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

        if (cloneAnimator == null) { EnterCooldown(); return; }

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
        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        _rb.velocity     = -toPlayer.normalized * (_approachSpeed * 0.25f) + _impulseVelocity;
        _faceDir         = toPlayer.normalized;
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

    public void StartFight()
    {
        SnapshotPlayerStats();
        _fightingLines?.StartLines();
        EnterMirrorStance();
    }

    private void EnterMirrorStance()
    {
        _state           = State.MirrorStance;
        _isRetreating    = false;
        _strafeFlipTimer = 0f;
        float pMin     = _isPhase2 ? phase2PatienceMin : patienceMin;
        float pMax     = _isPhase2 ? phase2PatienceMax : patienceMax;
        _patienceTimer = Random.Range(pMin, pMax);
    }

    private void EnterComboApproach()
    {
        _state         = State.ComboApproach;
        _curvedApproach = Random.value < 0.5f;
        _isFeinting     = !_isPhase2 && Random.value < 0.12f;
        _feintStopDist  = attackRange + Random.Range(0.8f, 1.6f);
    }

    private void EnterDashStrike(Vector2 toPlayer)
    {
        _state           = State.DashStrike;
        _faceDir         = toPlayer.normalized;
        _impulseVelocity = toPlayer.normalized * _dashForce;
        AudioManager.Instance?.PlayPlayerDash();
    }

    private void EnterSwinging(Vector2 toPlayer)
    {
        _state            = State.Swinging;
        _attack2Triggered = false;
        _faceDir          = toPlayer.normalized;
        _impulseVelocity  = toPlayer.normalized * _attackPushForce;
        AudioManager.Instance?.PlayPlayerAttack();
        cloneAnimator?.TriggerAttack1();
    }

    private void EnterDodge()
    {
        _state               = State.Dodge;
        _timer               = dodgeDuration;
        _dodgeCooldownTimer  = dodgeCooldown;

        Vector2 escDir       = GetBestDodgeDiagonal();
        _faceDir         = -escDir;
        _impulseVelocity = escDir * dodgeForce;
    }

    private void EnterCooldown()
    {
        _state = State.Cooldown;
        float cMin   = _isPhase2 ? phase2CooldownMin : cooldownMin;
        float cMax   = _isPhase2 ? phase2CooldownMax : cooldownMax;
        _timer       = Random.Range(cMin, cMax);
    }

    private void EnterWallEscape()
    {
        _state               = State.WallEscape;
        _timer               = wallEscapeDuration;
        _wallEscapeCooldown  = 2f;
        Vector2 open         = GetMostOpenDirection();
        _faceDir             = open;
        _impulseVelocity     = open * wallEscapeForce;
    }

    private void EnterPhase2Talk()
    {
        _state               = State.Phase2Talk;
        _impulseVelocity     = Vector2.zero;
        _rb.velocity         = Vector2.zero;
        _fightingLines?.StopLines();
        StartCoroutine(Phase2TalkRoutine());
    }

    private IEnumerator Phase2TalkRoutine()
    {
        // Camera focuses on clone
        bool cameraReady = false;
        if (BossCameraFocus.Instance != null)
            BossCameraFocus.Instance.Focus(() => cameraReady = true);
        else
            cameraReady = true;

        yield return new WaitUntil(() => cameraReady);

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

        // Camera returns to player
        bool cameraBack = false;
        if (BossCameraFocus.Instance != null)
            BossCameraFocus.Instance.Unfocus(() => cameraBack = true);
        else
            cameraBack = true;

        yield return new WaitUntil(() => cameraBack);

        _isPhase2 = true;
        _fightingLines?.StartLines();
        EnterMirrorStance();
    }

    [Header("Death Animation")]
    [SerializeField] private float  deathAnimDuration = 1.5f; // how long the death anim plays before ending starts
    [SerializeField] private string mainSceneName     = "Main Scene"; // used only if EndingSequence is absent

    private void OnDeath()
    {
        _state               = State.Dead;
        _impulseVelocity     = Vector2.zero;
        _rb.velocity         = Vector2.zero;
        _rb.bodyType         = RigidbodyType2D.Kinematic; // prevent physics from drifting corpse
        cloneAnimator?.SetWalking(false);

        // Prevent dead body from dealing contact damage or blocking movement
        var contact = GetComponent<EnemyContactDamage>();
        if (contact != null) contact.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        _fightingLines?.StopLines();

        // Lock player movement for the death / ending sequence
        var playerInput = _player?.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        // Clear any open dialogue so VictoryRoutine doesn't hang on WaitUntil
        DialogueManager.Instance?.ForceEnd();

        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.Hide();

        StopAllCoroutines(); // kills Phase2TalkRoutine if it's mid-dialogue so it can't call EnterMirrorStance
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        // Brief pause so clone just stands still before speaking
        yield return new WaitForSecondsRealtime(1f);

        if (deathDialogue != null && DialogueManager.Instance != null)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(deathDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        // Death animation plays AFTER the last dialogue line is dismissed
        AudioManager.Instance?.StopMusic();
        cloneAnimator?.TriggerDeath();
        // Wait for death anim to finish, with a hard timeout so it never hangs
        // if the Animator state name doesn't match "Player_Death"
        float deathWait = 0f;
        float deathTimeout = deathAnimDuration + 2f;
        while ((cloneAnimator == null || !cloneAnimator.IsDeathAnimDone()) && deathWait < deathTimeout)
        {
            deathWait += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.1f);

        if (EndingSequence.Instance != null)
        {
            EndingSequence.Instance.StartEnding();
        }
        else
        {
            // Fallback: no EndingSequence in scene — mark cleared and load main dungeon directly
            if (NGPlusManager.Instance != null)
                NGPlusManager.Instance.SetGameCleared();
            else
                new GameObject("NGPlusManager").AddComponent<NGPlusManager>().SetGameCleared();

            SceneManager.LoadScene(mainSceneName);
        }
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
        if (cloneAnimator == null) return;
        bool moving = _state == State.MirrorStance  ||
                      _state == State.ComboApproach ||
                      _state == State.DashStrike    ||
                      _state == State.WallEscape    ||
                      _state == State.Dodge         ||
                      _state == State.Cooldown;
        cloneAnimator.SetWalking(moving);
        if (_faceDir != Vector2.zero)
            cloneAnimator.SetDirection(_faceDir);
    }
}
