using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    public int currentHp;
    public bool IsDead { get; private set; }

    // Fired when this entity dies — InventoryManager and CraftingSystem listen if it's the player
    public event Action OnDeath;

    private void Awake() => currentHp = maxHP;

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        if (currentHp <= 0) Die();
    }

    public void AddMaxHP(int amount)
    {
        maxHP = Mathf.Min(maxHP + amount, 10);
        currentHp = Mathf.Min(currentHp + amount, maxHP);
    }

    public void Die()
    {
        if (IsDead) return; // prevent double-firing
        IsDead = true;
        OnDeath?.Invoke();
    }
}