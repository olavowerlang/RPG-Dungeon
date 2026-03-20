using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Bombshroom animation, sprite direction, death, loot and gold drops.
/// Attach to the visual child (same object as the Animator).
/// </summary>
public class BombshroomAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int WindUpTrigger = Animator.StringToHash("WindUp");
    private static readonly int ReleaseTrigger = Animator.StringToHash("Release");
    private static readonly int DirX = Animator.StringToHash("DirX");
    private static readonly int DirY = Animator.StringToHash("DirY");

    [Header("Visual")]
    [SerializeField] private Transform shroomVisual;
    [SerializeField] private float defaultScale = 1.5f;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimDuration = 1.2f;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private GameObject itemDropPrefab;

    [Header("XP & Gold")]
    [SerializeField] private float xpReward = 2f;
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 5;
    [SerializeField] private int maxGold = 12;

    private Animator _anim;
    private BombshroomAI _ai;
    private Health _health;
    private bool _deathTriggered;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ai = GetComponentInParent<BombshroomAI>();
        _health = GetComponentInParent<Health>();
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
            _ai.enabled = false;
            StartCoroutine(WaitForDeathAnim());
        }
    }

    private void UpdateDirection()
    {
        Vector2 dir = _ai.MoveDirection;
        _anim.SetFloat(DirX, dir.x);
        _anim.SetFloat(DirY, dir.y);

        if (shroomVisual != null && Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 sc = shroomVisual.localScale;
            sc.x = dir.x < 0 ? -Mathf.Abs(defaultScale) : Mathf.Abs(defaultScale);
            shroomVisual.localScale = sc;
        }
    }

    public void PlayWindUp() => _anim.SetTrigger(WindUpTrigger);
    public void PlayRelease() => _anim.SetTrigger(ReleaseTrigger);

    private IEnumerator WaitForDeathAnim()
    {
        yield return new WaitForSeconds(deathAnimDuration);

        XPManager xp = FindObjectOfType<XPManager>();
        if (xp != null) xp.GainXP(xpReward);

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
    }

    private void SpawnGold()
    {
        if (goldDropPrefab == null) return;
        int amount = Random.Range(minGold, maxGold + 1);
        var go = Instantiate(goldDropPrefab, _ai.transform.position + Vector3.left * 0.5f, Quaternion.identity);
        go.GetComponent<GoldDrop>()?.Init(amount);
    }
}
