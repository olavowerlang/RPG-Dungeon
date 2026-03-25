using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Slime animation, sprite direction, death VFX, loot and gold drops.
/// Attach to the visual child (same object as the Animator component).
/// </summary>
public class SlimeAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int DirX = Animator.StringToHash("DirX");
    private static readonly int DirY = Animator.StringToHash("DirY");

    [Header("Visual")]
    [SerializeField] private Transform slimeVisual;
    [SerializeField] private float defaultScale = 1.5f;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private GameObject itemDropPrefab;

    [Header("XP & Gold")]
    [SerializeField] private float xpReward = 1f;
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 2;
    [SerializeField] private int maxGold = 8;

    private Animator _anim;
    private SlimeAI _ai;
    private Health _health;
    private Rigidbody2D _rb;
    private GenericEnemyHitEffect _hitEffect;
    private bool _deathTriggered;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ai = GetComponentInParent<SlimeAI>();
        _health = GetComponentInParent<Health>();
        _rb = GetComponentInParent<Rigidbody2D>();
        _hitEffect = GetComponentInParent<GenericEnemyHitEffect>();
    }

    private void Update()
    {
        if (_deathTriggered) return;

        _anim.SetBool(IsMoving, _ai.IsMoving);
        UpdateDirection();

        if (_health.IsDead)
        {
            _deathTriggered = true;
            _anim.SetTrigger(DieTrigger);
            if (_hitEffect != null) _rb.velocity = _hitEffect.KnockbackVelocity;
            _ai.enabled = false;
            StartCoroutine(FreezeAfterDeath());
            StartCoroutine(WaitForDeathAnim());
        }
    }

    private void UpdateDirection()
    {
        Vector2 dir = _ai.MoveDirection;
        _anim.SetFloat(DirX, dir.x);
        _anim.SetFloat(DirY, dir.y);

        if (slimeVisual != null && Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 sc = slimeVisual.localScale;
            sc.x = dir.x < 0 ? -Mathf.Abs(defaultScale) : Mathf.Abs(defaultScale);
            slimeVisual.localScale = sc;
        }
    }

    private IEnumerator FreezeAfterDeath()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.25f);

        foreach (var col in _ai.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        _rb.simulated = false;
    }

    private IEnumerator WaitForDeathAnim()
    {
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).IsName("Slime_Death"));

        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        XPManager xp = FindObjectOfType<XPManager>();
        if (xp != null) xp.GainXP(xpReward);

        GenericEnemyHitEffect.SpawnDeathParticlesAt(_ai.transform.position);
        SpawnLoot();
        SpawnGold();

        Destroy(_ai.gameObject);
    }

    private void SpawnLoot()
    {
        if (lootTable == null || itemDropPrefab == null) return;
        ItemData drop = lootTable.Roll();
        if (drop == null) return;
        var go = Instantiate(itemDropPrefab, _ai.transform.position + Vector3.right * 0.5f, Quaternion.identity);
        go.GetComponent<ItemDrop>()?.Init(drop);
        go.AddComponent<DelayedReveal>().Reveal(0.05f);
    }

    private void SpawnGold()
    {
        if (goldDropPrefab == null) return;
        int amount = Random.Range(minGold, maxGold + 1);
        var go = Instantiate(goldDropPrefab, _ai.transform.position + Vector3.left * 0.5f, Quaternion.identity);
        go.GetComponent<GoldDrop>()?.Init(amount);
        go.AddComponent<DelayedReveal>().Reveal(0.05f);
    }
}
