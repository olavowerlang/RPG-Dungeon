using System.Collections;
using UnityEngine;

/// <summary>
/// Handles MiniSlime animation, death, and loot. Same structure as SlimeAnimator.
/// Attach to the visual child.
/// </summary>
public class MiniSlimeAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int DirX = Animator.StringToHash("DirX");
    private static readonly int DirY = Animator.StringToHash("DirY");

    [Header("Visual")]
    [SerializeField] private Transform slimeVisual;
    [SerializeField] private float defaultScale = 1f;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimDuration = 0.8f;

    [Header("Gold")]
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 4;

    [Header("XP")]
    [SerializeField] private float xpReward = 0.5f;

    private Animator _anim;
    private MiniSlimeAI _ai;
    private Health _health;
    private bool _deathTriggered;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ai = GetComponentInParent<MiniSlimeAI>();
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

        if (slimeVisual != null && Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 sc = slimeVisual.localScale;
            sc.x = dir.x < 0 ? -Mathf.Abs(defaultScale) : Mathf.Abs(defaultScale);
            slimeVisual.localScale = sc;
        }
    }

    private IEnumerator WaitForDeathAnim()
    {
        yield return new WaitForSeconds(deathAnimDuration);

        XPManager xp = FindObjectOfType<XPManager>();
        if (xp != null) xp.GainXP(xpReward);

        if (goldDropPrefab != null)
        {
            int amount = Random.Range(minGold, maxGold + 1);
            var go = Instantiate(goldDropPrefab, _ai.transform.position, Quaternion.identity);
            go.GetComponent<GoldDrop>()?.Init(amount);
        }

        Destroy(_ai.gameObject);
    }
}
