using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Bombshroom animation, sprite direction, death, loot and gold drops.
/// Attach to the visual child (same object as the Animator).
/// </summary>
public class BombshroomAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsDisguised = Animator.StringToHash("isDisguised");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int WindUpTrigger = Animator.StringToHash("WindUp");
    private static readonly int ReleaseTrigger = Animator.StringToHash("Release");
    private static readonly int PopOutTrigger = Animator.StringToHash("PopOut");
    private static readonly int DirX = Animator.StringToHash("DirX");
    private static readonly int DirY = Animator.StringToHash("DirY");

    [Header("Visual")]
    [SerializeField] private Transform shroomVisual;
    [SerializeField] private float defaultScale = 1.5f;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemData guaranteedDrop; // always drops (e.g. Mushroom)

    [Header("XP & Gold")]
    [SerializeField] private float xpReward = 2f;
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 5;
    [SerializeField] private int maxGold = 12;

    private Animator _anim;
    private BombshroomAI _ai;
    private Health _health;
    private Rigidbody2D _rb;
    private GenericEnemyHitEffect _hitEffect;
    private bool _deathTriggered;
    private Vector2 _lastDir = Vector2.down;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ai = GetComponentInParent<BombshroomAI>();
        _health = GetComponentInParent<Health>();
        _rb = GetComponentInParent<Rigidbody2D>();
        _hitEffect = GetComponentInParent<GenericEnemyHitEffect>();
    }

    private void Start()
    {
        if (_ai.isAmbush)
        {
            _anim.SetBool(IsDisguised, true);
            _anim.Play("Disguised");
        }
    }

    private void Update()
    {
        if (_deathTriggered) return;

        _anim.SetBool(IsMoving, _ai.MoveDirection.magnitude > 0.01f);
        UpdateDirection();
        CheckAnimationCompletion();

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

    private void CheckAnimationCompletion()
    {
        var info = _anim.GetCurrentAnimatorStateInfo(0);
        if (_ai.CurrentState == BombshroomAI.S.WindUp &&
            info.IsName("WindUp") && info.normalizedTime >= 1f)
        {
            OnWindUpComplete();
        }
        else if (_ai.CurrentState == BombshroomAI.S.PopOut &&
                 (info.IsName("PopOut") && info.normalizedTime >= 1f || info.IsName("Idle")))
        {
            OnPopOutComplete();
        }
    }

    private void UpdateDirection()
    {
        Vector2 dir = _ai.MoveDirection;

        if (dir.magnitude > 0.01f)
            _lastDir = dir;

        if (Mathf.Abs(_lastDir.x) >= Mathf.Abs(_lastDir.y))
        {
            _anim.SetFloat(DirX, 1f);
            _anim.SetFloat(DirY, 0f);
        }
        else
        {
            _anim.SetFloat(DirX, 0f);
            _anim.SetFloat(DirY, _lastDir.y > 0f ? 1f : -1f);
        }

        if (shroomVisual != null && Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 sc = shroomVisual.localScale;
            sc.x = dir.x < 0 ? -Mathf.Abs(sc.x) : Mathf.Abs(sc.x);
            shroomVisual.localScale = sc;
        }
    }

    public void PlayWindUp() => _anim.Play("WindUp");
    public void PlayPopOut()
    {
        _anim.SetBool(IsDisguised, false);
        _anim.Play("PopOut");
    }

    public void OnPopOutComplete() => _ai.OnPopOutComplete();
    public void OnWindUpComplete() => _ai.OnWindUpComplete();

    // Animation event — place on the exact frame the shroom pops up visually
    public void OnRevealFrame() => AudioManager.Instance?.PlayShroomReveal();

    // Animation event — place on the frame the gas is released (attack anim)
    public void OnGasFrame() => AudioManager.Instance?.PlayShroomGas();

    // Animation event — place on the frame the death sound should hit
    public void OnDeathFrame() => AudioManager.Instance?.PlayShroomDeath();

    // Animation event — place at the end of death anim for the death gas burst
    public void OnDeathGasFrame() => AudioManager.Instance?.PlayShroomDeathGas();

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
            _anim.GetCurrentAnimatorStateInfo(0).IsName("Death"));
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        XPManager xp = FindObjectOfType<XPManager>();
        if (xp != null) xp.GainXP(xpReward);

        SpawnLoot();
        SpawnGold();
        _ai.SpawnDeathGas();
        var runner = new GameObject("ShroomParticleRunner").AddComponent<ShroomParticleRunner>();
        runner.Run(_ai.transform.position);

        Destroy(_ai.gameObject);
    }

    private void SpawnLoot()
    {
        if (itemDropPrefab == null) return;

        // Guaranteed drop (e.g. Mushroom)
        if (guaranteedDrop != null)
        {
            var go = Instantiate(itemDropPrefab, _ai.transform.position + Vector3.up * 0.4f, Quaternion.identity);
            go.transform.localScale = Vector3.one * 1.5f;
            go.GetComponent<ItemDrop>()?.Init(guaranteedDrop);
            go.AddComponent<DelayedReveal>().Reveal(0.45f);
        }

        // Random drop from loot table
        if (lootTable != null)
        {
            ItemData drop = lootTable.Roll();
            if (drop != null)
            {
                var go = Instantiate(itemDropPrefab, _ai.transform.position + Vector3.right * 0.5f, Quaternion.identity);
                go.transform.localScale = Vector3.one * 1.5f;
                go.GetComponent<ItemDrop>()?.Init(drop);
                go.AddComponent<DelayedReveal>().Reveal(0.45f);
            }
        }
    }

    private void SpawnGold()
    {
        if (goldDropPrefab == null) return;
        int amount = Random.Range(minGold, maxGold + 1);
        var go = Instantiate(goldDropPrefab, _ai.transform.position + Vector3.left * 0.5f, Quaternion.identity);
        go.GetComponent<GoldDrop>()?.Init(amount);
        go.AddComponent<DelayedReveal>().Reveal(0.45f);
    }

}
