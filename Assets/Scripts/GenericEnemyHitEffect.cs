using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable hit/knockback/blink component for all new enemies.
/// Implements IDamageable. Place on the enemy root.
/// </summary>
[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class GenericEnemyHitEffect : MonoBehaviour, IDamageable
{
    [Header("Knockback")]
    [SerializeField] private float knockForce = 5f;
    [SerializeField] private float impulseDecayRate = 8f;

    [Header("Blink")]
    [SerializeField] private float blinkDuration = 0.3f;
    [SerializeField] private float blinkFrequency = 0.05f;

    private Health _hp;
    private SpriteRenderer _sr;

    private Vector2 _impulseVel;
    private float _invulTimer;

    public Vector2 KnockbackVelocity => _impulseVel;
    public bool IsInvulnerable => _invulTimer > 0f;

    public event Action OnHitTaken;

    [Header("Death Particles")]
    [SerializeField] private int      deathParticleCount    = 50;
    [SerializeField] private float    deathParticleSpeed    = 5f;
    [SerializeField] private Material deathParticleMaterial; // assign URP Particles/Unlit material

    private static int      _staticParticleCount    = 50;
    private static float    _staticParticleSpeed    = 5f;
    private static Material _staticParticleMaterial = null;

    private void Awake()
    {
        _hp = GetComponent<Health>();
        _sr = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void SpawnDeathParticles()
    {
        _staticParticleCount    = deathParticleCount;
        _staticParticleSpeed    = deathParticleSpeed;
        _staticParticleMaterial = deathParticleMaterial;
        SpawnDeathParticlesAt(transform.position);
    }

    public static void SpawnDeathParticlesAt(Vector3 position, int sortingOrder = 32767)
    {
        var go  = new GameObject("DeathFX");
        go.transform.position = position;
        var ps  = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main            = ps.main;
        main.playOnAwake    = false;
        main.duration       = 0.4f;
        main.loop           = false;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(_staticParticleSpeed * 0.6f, _staticParticleSpeed * 1.4f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.gravityModifier = 0.25f;
        main.maxParticles   = _staticParticleCount;
        main.startColor     = new ParticleSystem.MinMaxGradient(
            new Color(0.3f, 0.7f, 1f),   // soft blue
            new Color(0.8f, 0.95f, 1f));  // near-white cyan

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, _staticParticleCount) });

        var shape       = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.15f;

        // Fade + shrink over lifetime
        var col     = ps.colorOverLifetime;
        col.enabled = true;
        var grad    = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var size           = ps.sizeOverLifetime;
        size.enabled       = true;
        var sizeCurve      = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        size.size          = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        Material mat = _staticParticleMaterial;
        if (mat == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (s != null) mat = new Material(s);
        }

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.sortingLayerName = "Default";
        rend.sortingOrder     = sortingOrder;
        if (mat != null) rend.material = mat;

        ps.Play();
        Destroy(go, 2f);
    }

    private void FixedUpdate()
    {
        if (_invulTimer > 0f) _invulTimer -= Time.fixedDeltaTime;
        if (_impulseVel == Vector2.zero) return;
        _impulseVel = Vector2.Lerp(_impulseVel, Vector2.zero,
            Time.fixedDeltaTime * impulseDecayRate);
    }

    public void GrantInvulnerability(float duration)
    {
        _invulTimer = duration;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_sr != null) _sr.enabled = true;
    }

    public void TakeHit(int dmg, Vector2 dir, float knockback = 0f)
    {
        if (_hp.IsDead) return;
        if (_invulTimer > 0f) return;

        _impulseVel += dir.normalized * (knockForce + knockback);
        _hp.TakeDamage(dmg);

        OnHitTaken?.Invoke();
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        float elapsed = 0f;
        while (elapsed < blinkDuration)
        {
            if (_sr == null) yield break;
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkFrequency);
            elapsed += blinkFrequency;
        }
        if (_sr != null) _sr.enabled = true;
    }
}
