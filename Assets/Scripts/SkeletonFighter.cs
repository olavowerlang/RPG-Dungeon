using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonFighter : MonoBehaviour
{
    public enum S { Approach, Orbit, DashPrep, DashMove, Hit, Cooldown }
    S _state = S.Approach;

    [Header("Dash Tracking")]
    [Range(0f, 1f)]
    [SerializeField] float dashTrackingFraction = 0.5f;

    [Header("Distances")]
    [SerializeField] float orbitRadius  = 3.0f;
    [SerializeField] float attackRange  = 1.2f;

    [Header("Times")]
    [SerializeField] Vector2 guardTimeRange = new (1.3f, 2.5f);
    [SerializeField] float dashPrepTime = 0.35f;
    [SerializeField] float cooldownTime = 0.7f;

    [Header("Speed")]
    [SerializeField] float approachSpeed = 1.4f;
    [SerializeField] float orbitSpeed    = 1.2f;
    [SerializeField] float dashSpeed     = 2.2f;

    [Header("Refs")]
    [SerializeField] GameObject swordHitbox;
    [SerializeField] EnemyAnimator enemyAnim;
    [SerializeField] Transform sfVisual;

    public Rigidbody2D Rb { get; private set; }
    Transform player;
    Vector2 dashTarget;
    float _timer, _guardTimer;

    float dashDuration;
    float dashElapsed;
    bool dashTargetLocked;

    public bool IsWalking => Rb.velocity != Vector2.zero;

    void Awake()
    {
        Rb     = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
        swordHitbox.SetActive(false);
        ResetGuardTimer();
    }

    void FixedUpdate()
    {
        Vector2 toPlayer = player.position - transform.position;
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
                _timer     -= Time.fixedDeltaTime;
                if (_timer <= 0f) EnterDashMove();
                break;

            case S.DashMove:
                {                 
                    Vector2 currentPos = (Vector2)transform.position;
                    Vector2 playerPos = (Vector2)player.position;

                    dashElapsed += Time.fixedDeltaTime;
                    if (!dashTargetLocked)
                    {
                        if (dashElapsed <= dashDuration * dashTrackingFraction)
                        {                          
                            dashTarget = playerPos;
                        }
                        else
                        {
                            Vector2 dir = (playerPos - currentPos).normalized;
                            dashTargetLocked = true;
                            dashTarget = playerPos - dir * attackRange;
                        }
                    }

                    Vector2 toTarget = dashTarget - currentPos;
                    Vector2 movement = toTarget.normalized * dashSpeed;
                    Vector2 futurePos = currentPos + movement * Time.fixedDeltaTime;
                    Rb.velocity = movement;

                    if (Vector2.Dot(toTarget, dashTarget - futurePos) <= 0f)
                        EnterHit();
                }
                break;



            case S.Hit:
                // vazio: espera AnimationEvent chamar OnAttackAnimationEnd()
                break;

            case S.Cooldown:
                Rb.velocity = Vector2.zero;
                _timer     -= Time.fixedDeltaTime;
                if (_timer <= 0f)
                {
                    if (dist <= attackRange)
                        EnterDashPrep();
                    else if (dist > orbitRadius)
                        EnterApproach();
                    else
                        EnterOrbit();
                }
                break;

        }

    
    }

    void EnterApproach()
    {
        _state = S.Approach;
    }

    void EnterOrbit()
    {
        _state = S.Orbit;
        ResetGuardTimer();
    }

    void EnterDashPrep()
    {
        _state = S.DashPrep;
        _timer = dashPrepTime;
    }

    void EnterDashMove()
    {
        _state = S.DashMove;

        float initialDist = Vector2.Distance(transform.position, player.position);
        dashDuration = initialDist / dashSpeed;
        dashElapsed = 0f;
        dashTargetLocked = false;

        dashTarget = player.position;
    }

    void EnterHit()
    {
        _state = S.Hit;
        swordHitbox.SetActive(true);
        enemyAnim.PlaySfAttack();
    }

    void EnterCooldown()
    {
        _state = S.Cooldown;
        _timer = cooldownTime;
        swordHitbox.SetActive(false);
    }

    void ResetGuardTimer() =>
        _guardTimer = Random.Range(guardTimeRange.x, guardTimeRange.y);

    public void OnAttackAnimationEnd()
    {
        if (_state == S.Hit)
            EnterCooldown();
    
    }

    public void DefineSfSpriteDirection()
    {
        bool faceLeft;
        
            faceLeft = player.position.x < transform.position.x;

        var sc = sfVisual.localScale;
        sc.x = faceLeft ? -1.75f : 1.75f;
        sfVisual.localScale = sc;
    }
}
