using UnityEngine;

/// <summary>
/// Drives the player Animator Controller on the Clone.
/// Sits on the same child GameObject as the Animator so animation events route here.
/// </summary>
public class CloneAnimator : MonoBehaviour
{
    private static readonly int IsWalkingHash      = Animator.StringToHash("isWalking");
    private static readonly int LightAttack1Hash   = Animator.StringToHash("LightAttackTrigger1");
    private static readonly int LightAttack2Hash   = Animator.StringToHash("LightAttackTrigger2");
    private static readonly int DieHash            = Animator.StringToHash("Die");
    private static readonly int DirXHash           = Animator.StringToHash("DirX");
    private static readonly int DirYHash           = Animator.StringToHash("DirY");

    [SerializeField] private Animator _animator;
    private DamageDealer[] _hitboxes;

    // Polled by CloneAI each frame
    public bool Attack2WindowOpen { get; private set; }
    public bool ComboFinished     { get; private set; }

    private void Awake()
    {
        // Only search if Inspector didn't assign it (so [SerializeField] assignment is respected)
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>(true);
        if (_animator == null) _animator = GetComponentInParent<Animator>(true);
        if (_animator == null) _animator = transform.root.GetComponentInChildren<Animator>(true);

        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
        DisableAllHitboxes();

        // Hitboxes inherit Player layer from the clone visual, but Player-vs-Player
        // triggers are disabled in the physics matrix. Set them to Enemy layer so
        // they can actually trigger against the player collider.
        int enemyLayer  = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        foreach (var dd in _hitboxes)
        {
            if (enemyLayer != -1)  dd.gameObject.layer = enemyLayer;
            if (playerLayer != -1) dd.AddHitLayer(playerLayer);
        }
    }

    // ── Called by CloneAI ────────────────────────────────────────────────────

    public void SetWalking(bool walking)
    {
        if (_animator == null) return;
        _animator.SetBool(IsWalkingHash, walking);
    }

    public void SetDirection(Vector2 dir)
    {
        if (_animator == null) return;
        _animator.SetFloat(DirXHash, Mathf.Abs(dir.x));
        _animator.SetFloat(DirYHash, dir.y);

        if (Mathf.Abs(dir.x) > 0.05f)
        {
            Vector3 s = transform.localScale;
            s.x = dir.x < 0f ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    public void TriggerAttack1()
    {
        if (_animator == null) return;
        Attack2WindowOpen = false;
        ComboFinished     = false;
        DisableAllHitboxes();
        _animator.SetTrigger(LightAttack1Hash);
    }

    public void TriggerAttack2()
    {
        if (_animator == null) return;
        DisableAllHitboxes();
        _animator.SetTrigger(LightAttack2Hash);
    }

    public void TriggerDeath()
    {
        if (_animator == null) return;
        _animator.SetTrigger(DieHash);
    }

    public bool IsDeathAnimDone()
    {
        if (_animator == null) return true; // no animator → don't hang forever
        var info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("Player_Death") && info.normalizedTime >= 0.95f;
    }

    public void ResetComboFinished() => ComboFinished = false;

    // ── Animation Events (fired by the Animator Controller) ──────────────────

    public void EnableHitbox(int i)  => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();

    public void LightAttack2Window() => Attack2WindowOpen = true;

    public void LightAttack1Ended()
    {
        Attack2WindowOpen = false;
        ComboFinished     = true;
        DisableAllHitboxes();
    }

    public void LightAttack2Ended()
    {
        Attack2WindowOpen = false;
        ComboFinished     = true;
        DisableAllHitboxes();
    }

    private void DisableAllHitboxes()
    {
        foreach (var h in _hitboxes) h.EndSwing();
    }
}
