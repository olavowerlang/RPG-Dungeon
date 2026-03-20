using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Skeleton Archer animation, sprite direction, death, loot and gold drops.
/// Attach to the visual child (same object as the Animator).
/// </summary>
public class SkeletonArcherAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int DirX = Animator.StringToHash("DirX");
    private static readonly int DirY = Animator.StringToHash("DirY");

    [Header("Visual")]
    [SerializeField] private Transform archerVisual;
    [SerializeField] private float defaultScale = 1.5f;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimDuration = 1.3f;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private GameObject itemDropPrefab;

    [Header("XP & Gold")]
    [SerializeField] private float xpReward = 3f;
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 8;
    [SerializeField] private int maxGold = 18;

    private Animator _anim;
    private SkeletonArcherAI _ai;
    private Health _health;
    private bool _deathTriggered;
    private Transform _player;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ai = GetComponentInParent<SkeletonArcherAI>();
        _health = GetComponentInParent<Health>();
        _player = GameObject.FindWithTag("Player")?.transform;
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
        // In combat, always face the player; in patrol, face movement direction
        Vector2 faceDir;
        if (_ai.CurrentState == SkeletonArcherAI.S.Patrol)
            faceDir = _ai.MoveDirection;
        else if (_player != null)
            faceDir = ((Vector2)(_player.position - _ai.transform.position)).normalized;
        else
            faceDir = _ai.MoveDirection;

        _anim.SetFloat(DirX, faceDir.x);
        _anim.SetFloat(DirY, faceDir.y);

        if (archerVisual != null && Mathf.Abs(faceDir.x) > 0.01f)
        {
            Vector3 sc = archerVisual.localScale;
            sc.x = faceDir.x < 0 ? -Mathf.Abs(defaultScale) : Mathf.Abs(defaultScale);
            archerVisual.localScale = sc;
        }
    }

    public void PlayShoot() => _anim.SetTrigger(ShootTrigger);

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
