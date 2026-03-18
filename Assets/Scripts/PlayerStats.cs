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
