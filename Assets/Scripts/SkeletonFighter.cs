using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonFighter : MonoBehaviour
{
    public enum S { Approach, Orbit, DashPrep, DashMove, Hit, Cooldown }
    S _state = S.Approach;

    [Header("Dash Tracking")]
    [Range(0f, 1f)]
    [SerializeField] private float dashTrackingFraction = 0.5f;

    [Header("Distances")]
    [SerializeField] private float orbitRadius = 3.0f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Times")]
    [SerializeField] private Vector2 guardTimeRange = new (1.3f, 2.5f);
    [SerializeField] private float dashPrepTime = 0.35f;
    [SerializeField] private float cooldownTime = 0.7f;

    [Header("Speed")]
    [SerializeField] private float approachSpeed = 1.4f;
    [SerializeField] private float orbitSpeed = 1.2f;
    [SerializeField] private float dashSpeed = 2.2f;

    [Header("Refs")]
    [SerializeField] private EnemyAnimator enemyAnim;
    [SerializeField] private Transform sfVisual;

    private Rigidbody2D Rb { get; set; }
    private Transform _player;
    private Vector2 _dashTarget;
    private float _timer, _guardTimer;

    private float _dashDuration;
    private float _dashElapsed;
    private bool _dashTargetLocked;

    public bool IsWalking => Rb.velocity != Vector2.zero;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        _player = GameObject.FindWithTag("Player").transform;
        ResetGuardTimer();
    }

    private void FixedUpdate()
    {
        Vector2 toPlayer = _player.position - transform.position;
        float dist = toPlayer.magnitude;

        switch (_state)
        {
            case S.Approach:
                if (dist > orbitRadius)
                    Rb.velocity = toPlayer.normalized * approachSpeed;
                else
                    EnterOrbit();
                break;

            case S.Orbit:
                Rb.velocity = new Vector2(-toPlayer.y, toPlayer.x).normalized * orbitSpeed;
                _guardTimer -= Time.fixedDeltaTime;
                if (dist <= attackRange || _guardTimer <= 0f)
                    EnterDashPrep();
                break;

            case S.DashPrep:
                Rb.velocity = Vector2.zero;
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
                    Vector2 futurePos = currentPos + movement * Time.fixedDeltaTime;
                    Rb.velocity = movement;

                    if (Vector2.Dot(toTarget, _dashTarget - futurePos) <= 0f)
                        EnterHit();
                }
                break;



            case S.Hit:
                // vazio: espera AnimationEvent chamar OnAttackAnimationEnd()
                break;

            case S.Cooldown:
                Rb.velocity = Vector2.zero;
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f)
                {
                    if (dist <= attackRange)
                        EnterHit();
                    else if (dist > orbitRadius)
                        EnterApproach();
                    else
                        EnterOrbit();
                }
                break;

        }

    
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
        _state = S.Hit;
        enemyAnim.PlaySfAttack();
    }

    private void EnterCooldown()
    {
        _state = S.Cooldown;
        _timer = cooldownTime;
    }

    private void ResetGuardTimer() =>
        _guardTimer = Random.Range(guardTimeRange.x, guardTimeRange.y);

    public void OnAttackAnimationEnd()
    {
        if (_state == S.Hit)
            EnterCooldown();
    }

    public void DefineSfSpriteDirection()
    {
        var faceLeft = _player.position.x < transform.position.x;
        
        var sc = sfVisual.localScale;
        sc.x = faceLeft ? -1.75f : 1.75f;
        sfVisual.localScale = sc;
    }
}
