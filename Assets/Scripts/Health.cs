using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    public int MaxHP => maxHP;
    public int currentHp;
    public bool IsDead { get; private set; }

    // Fired when this entity dies — InventoryManager and CraftingSystem listen if it's the player
    public event Action OnDeath;
    // Fired on any damage taken (before death check)
    public event Action OnHit;

    private void Awake() => currentHp = maxHP;

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        currentHp -= dmg;
        OnHit?.Invoke();
        if (currentHp <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHP);
    }

    public void HealFull()
    {
        currentHp = maxHP;
    }

    public void AddMaxHP(int amount)
    {
        maxHP = Mathf.Min(maxHP + amount, 10);
    }

    /// <summary>Sets max HP to an exact value and refills current HP. Used by NGPlusEnemyScaler.</summary>
    public void ScaleMaxHP(int newMax)
    {
        maxHP     = newMax;
        currentHp = newMax;
    }

    public void Die()
    {
        if (IsDead) return; // prevent double-firing
        IsDead = true;
        OnDeath?.Invoke();
    }
}