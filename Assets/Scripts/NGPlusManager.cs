using UnityEngine;

/// <summary>
/// Persists across scene loads. Tracks game-cleared state, NG+ flag,
/// and carries over player stats between runs.
/// Add to the first scene — it will survive all scene transitions.
/// </summary>
public class NGPlusManager : MonoBehaviour
{
    public static NGPlusManager Instance { get; private set; }

    // Always starts at 1 — doubled by SetGameCleared each run (NG+1=2x, NG+2=4x, etc.)
    public float EnemyStatMultiplier { get; private set; } = 1f;

    // 1 = NG+1, 2 = NG+2, etc.
    public int NGPlusCount { get; private set; } = 0;

    // ── Flags ────────────────────────────────────────────────────────────────
    public bool GameCleared { get; private set; }
    public bool IsNGPlus    { get; private set; }

    // ── Carried-over player stats ─────────────────────────────────────────────
    public float CarriedSpeed          { get; private set; }
    public float CarriedDashForce      { get; private set; }
    public int   CarriedDamage         { get; private set; }
    public float CarriedKnockback      { get; private set; }
    public bool  CarriedHasSword       { get; private set; }
    public bool  CarriedHasDash        { get; private set; }
    public float CarriedMaxDashStamina { get; private set; }
    public int   CarriedMaxHP          { get; private set; }
    public int   CarriedCurrentHP      { get; private set; }
    public int   CarriedGold           { get; private set; }

    private bool _hasTransitionSnapshot;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call before any scene transition to snapshot current player stats so they survive the load.
    /// </summary>
    public void SnapshotForTransition()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        CarriedSpeed          = ps.speed;
        CarriedDashForce      = ps.dashForce;
        CarriedDamage         = ps.damage;
        CarriedKnockback      = ps.knockbackForce;
        CarriedHasSword       = ps.hasSword;
        CarriedHasDash        = ps.hasDash;
        CarriedMaxDashStamina = ps.maxDashStamina;

        var health = ps.GetComponent<Health>();
        CarriedMaxHP     = health != null ? health.MaxHP     : 3;
        CarriedCurrentHP = health != null ? health.currentHp : CarriedMaxHP;

        if (GoldManager.Instance != null)
            CarriedGold = GoldManager.Instance.Gold;

        _hasTransitionSnapshot = true;
    }

    /// <summary>
    /// Call when the clone dies. Snapshots current player state and marks the run cleared.
    /// </summary>
    public void SetGameCleared()
    {
        GameCleared           = true;
        IsNGPlus              = true;
        EnemyStatMultiplier  *= 2f; // NG+1=2x, NG+2=4x, etc.
        NGPlusCount++;

        PigShopkeeper.ResetAll();
        NPCDialogue.ResetZone1();

        var ps = PlayerStats.Instance;
        if (ps != null)
        {
            CarriedSpeed          = ps.speed;
            CarriedDashForce      = ps.dashForce;
            CarriedDamage         = ps.damage;
            CarriedKnockback      = ps.knockbackForce;
            CarriedHasSword       = ps.hasSword;
            CarriedHasDash        = ps.hasDash;
            CarriedMaxDashStamina = ps.maxDashStamina;

            var health = ps.GetComponent<Health>();
            CarriedMaxHP     = health != null ? health.MaxHP : 3;
            CarriedCurrentHP = CarriedMaxHP; // full HP at the start of a new NG+ run
        }

        if (GoldManager.Instance != null)
            CarriedGold = GoldManager.Instance.Gold;
    }

    /// <summary>
    /// Call when the player dies in NG+. Kicks them back to a normal run.
    /// </summary>
    public void ExitNGPlus()
    {
        IsNGPlus = false;
    }

    /// <summary>
    /// Apply carried-over stats to a freshly loaded player. Call from NGPlusCarryOver.Start().
    /// </summary>
    public void ApplyCarryOver(PlayerStats ps)
    {
        if ((!IsNGPlus && !_hasTransitionSnapshot) || ps == null) return;
        _hasTransitionSnapshot = false;

        ps.speed          = CarriedSpeed;
        ps.dashForce      = CarriedDashForce;
        ps.damage         = CarriedDamage;
        ps.knockbackForce = CarriedKnockback;
        ps.hasSword       = CarriedHasSword;
        ps.hasDash        = CarriedHasDash;
        ps.maxDashStamina = CarriedMaxDashStamina;
        ps.PushToDealers();

        var health = ps.GetComponent<Health>();
        if (health != null)
        {
            health.ScaleMaxHP(CarriedMaxHP);
            health.currentHp = Mathf.Clamp(CarriedCurrentHP, 1, CarriedMaxHP);
        }

        if (GoldManager.Instance != null)
            GoldManager.Instance.SetGold(CarriedGold);
    }
}
