using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{//animator handling too much, not following separation of responsibility

    private static readonly int SfAttackTrigger = Animator.StringToHash("SFAttackTrigger");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Die = Animator.StringToHash("Die");

    [SerializeField] private DamageDealer[] _hitboxes;
    private Animator _anim;
    private SkeletonFighter _skeletonFighter;
    private SkeletonHitEffect _skeletonHitEffect;
    private Health _health;
    private Rigidbody2D _rb;

    [SerializeField] private GameObject skullFighter;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private GameObject itemDropPrefab;

    [Header("Gold Drop")]
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int minGold = 5;
    [SerializeField] private int maxGold = 15;

    private bool _enemyDeathTriggered;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _skeletonFighter = GetComponentInParent<SkeletonFighter>();
        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
        _skeletonHitEffect = GetComponentInParent<SkeletonHitEffect>();
        _health = GetComponentInParent<Health>();
        _rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Update()
    {
        bool inHit = _skeletonFighter.CurrentState == SkeletonFighter.S.Hit;
        _anim.SetBool(IsWalking, !inHit && _skeletonFighter.IsWalking);

        /* Delega o flip para o próprio SkeletonFighter */
        if (!_enemyDeathTriggered)
            _skeletonFighter.DefineSfSpriteDirection();

        if (_health.IsDead && !_enemyDeathTriggered)
        {
            _enemyDeathTriggered = true;
            _anim.applyRootMotion = false;
            _anim.SetTrigger(Die);
            _skeletonFighter.enabled = false;
            skullFighter.GetComponent<Collider2D>().enabled = false;

            StartCoroutine(FreezeAfterDeath());
            StartCoroutine(WaitForEnemyDeathAnim());
        }
    }

    //por enquanto so funciona pro SkeletonFighter, precisa ser modularizado later on
    /* ---------- NEEDS TO BE SEPARATED FROM ANIM ---------- */

    private IEnumerator WaitForEnemyDeathAnim()
    {
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).IsName("SkeletonFighter_Die"));

        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        // Give XP
        XPManager playerXP = FindObjectOfType<XPManager>();
        if (playerXP != null)
            playerXP.GainXP(5);

        // Drop loot and gold
        SpawnLoot();
        SpawnGold();

        Destroy(_skeletonFighter.gameObject);
    }

    private void SpawnLoot()
    {
        if (lootTable == null || itemDropPrefab == null) return;

        ItemData drop = lootTable.Roll();
        if (drop == null) return;

        // Spawn slightly offset so it's visible
        Vector3 spawnPos = _skeletonFighter.transform.position + new Vector3(0.5f, 0f, 0f);
        GameObject dropGO = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);

        ItemDrop itemDrop = dropGO.GetComponent<ItemDrop>();
        if (itemDrop != null)
            itemDrop.Init(drop);
    }

    private void SpawnGold()
    {
        if (goldDropPrefab == null) return;
        int amount = Random.Range(minGold, maxGold + 1);
        Vector3 pos = _skeletonFighter.transform.position + new Vector3(-0.5f, 0.3f, 0f);
        GameObject go = Instantiate(goldDropPrefab, pos, Quaternion.identity);
        go.GetComponent<GoldDrop>()?.Init(amount);
    }

    private IEnumerator FreezeAfterDeath()
    {
        // 1) Deixa entrar o último impulso de física
        yield return new WaitForFixedUpdate();
        // 2) Dá um pequeno delay extra pra visibilizar melhor o knockback
        yield return new WaitForSeconds(0.25f);

        // 3) Desativa todos os colliders do inimigo pra não gerar mais repulsões
        foreach (var col in _skeletonFighter.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // 4) Desliga totalmente a simulação física
        _rb.simulated = false;
    }

    /* Trigger de ataque disparado pela FSM */
    public void PlaySfAttack() => _anim.SetTrigger(SfAttackTrigger);

    /* ---------- Animation Events ---------- */

    public void EnableHitbox(int i) => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();
    public void OnAttackAnimationEnd() => _skeletonFighter.OnAttackAnimationEnd();
}