using UnityEngine;

/// <summary>
/// Persists across scene loads. Tracks game-cleared state, NG+ flag,
/// and carries over player stats between runs.
/// Add to the first scene — it will survive all scene transitions.
/// </summary>
public class NGPlusManager : MonoBehaviour
{
    public static NGPlusManager Instance { get; private set; }

    [Header("NG+ Enemy Scaling")]
    public float EnemyStatMultiplier = 2f;

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
    public int   CarriedGold           { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call when the clone dies. Snapshots current player state and marks the run cleared.
    /// </summary>
    public void SetGameCleared()
    {
        GameCleared = true;
        IsNGPlus    = true;

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
            CarriedMaxHP = health != null ? health.MaxHP : 3;
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
        if (!IsNGPlus || ps == null) return;

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
            health.ScaleMaxHP(CarriedMaxHP);

        if (GoldManager.Instance != null)
            GoldManager.Instance.SetGold(CarriedGold);
    }
}
