using UnityEngine;

/// <summary>
/// Single source of truth for all player tunable values.
/// Attach to the Player root. All other components read from here.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("— Unlocks —")]
    public bool hasSword = false;
    public bool hasDash = false;

    [Header("— Upgradable Stats —")]
    public float speed = 10f;
    public float dashForce = 60f;
    public int damage = 1;
    public float knockbackForce = 0f;

    [Header("Dash Stamina")]
    public float maxDashStamina = 3f;
    public float dashStaminaCost = 1f;
    public float dashStaminaRegenTime = 1f; // seconds per unit

    [Header("Movement (Fixed)")]
    public float attackPushForce = 6f;

    [Header("Combat (Outgoing)")]
    [SerializeField] private DamageDealer[] damageDealers;

    [Header("On Hit (Received)")]
    public float receivedKnockForce = 3f;
    public float invulnTime = 1f;
    public float blinkFreq = 0.06f;

    [Header("Progression")]
    public int startingXPLimit = 10;
    public int damagePerLevel = 1;

    // ── Display System ────────────────────────────────────────────────────────
    // Baseline: the behind-the-scenes value that equals display "1".
    // Step: the behind-the-scenes increment that equals display "+1".
    // These must match the increment values in CraftingSystem.
    // Gameplay code never reads these — they are used only by StatsUI + ResetBuffs.

    [Header("— Display: Baselines (actual value that shows as 1) —")]
    public float baseSpeed     = 10f;
    public float baseDashForce = 60f;
    public int   baseDamage    = 1;
    public float baseKnockback = 0f;
    public int   baseMaxHP     = 3;
    [Tooltip("Not shown in stats UI — used only for death reset")]
    public float baseMaxDashStamina = 3f;

    [Header("— Display: Steps (actual increment that adds +1 to display) —")]
    [Tooltip("Must match CraftingSystem.speedIncrement")]
    public float displaySpeedStep      = 2.5f;
    [Tooltip("Must match CraftingSystem.dashIncrement")]
    public float displayDashStep       = 5f;
    [Tooltip("Must match CraftingSystem.knockbackIncrement")]
    public float displayKnockbackStep  = 3.5f;
    [Tooltip("Must match CraftingSystem.damageIncrement")]
    public int   displayStrengthStep   = 1;
    // maxHP is always +1 per heart container, no step needed

    // ── Computed display values (read only by StatsUI) ────────────────────────
    public int DisplayStrength  => Mathf.Max(1, 1 + (damage        - baseDamage)  / Mathf.Max(1, displayStrengthStep));
    public int DisplaySpeed     => Mathf.Max(1, 1 + Mathf.RoundToInt((speed          - baseSpeed)     / Mathf.Max(0.001f, displaySpeedStep)));
    public int DisplayDash      => Mathf.Max(1, 1 + Mathf.RoundToInt((dashForce      - baseDashForce)  / Mathf.Max(0.001f, displayDashStep)));
    public int DisplayKnockback => Mathf.Max(1, 1 + Mathf.RoundToInt((knockbackForce - baseKnockback)  / Mathf.Max(0.001f, displayKnockbackStep)));

    private void Awake()
    {
        Instance = this;
        PushToDealers();
    }

    private void OnValidate() => PushToDealers();

    public void PushToDealers()
    {
        if (damageDealers == null) return;
        foreach (var dd in damageDealers)
        {
            if (dd == null) continue;
            dd.Damage = damage;
            dd.knockbackForce = knockbackForce;
        }
    }

    public DamageDealer[] GetDamageDealers() => damageDealers;

    public void AddDamage(int amount)
    {
        damage += amount;
        PushToDealers();
    }
}
