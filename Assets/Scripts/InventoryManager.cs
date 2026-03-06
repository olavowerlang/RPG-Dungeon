using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public const int MaxIngredientSlots = 10;

    // Sword slot — separate from ingredients
    public ItemData equippedSword { get; private set; } = null;

    // Ingredient slots only (max 10)
    public List<InventorySlot> ingredientSlots = new();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called by the NPC pig to give the sword
    public void EquipSword(ItemData sword)
    {
        if (equippedSword != null) return; // already has sword
        equippedSword = sword;
        OnInventoryChanged?.Invoke();
    }

    public bool HasSword() => equippedSword != null;

    // Add ingredient to inventory
    public bool AddItem(ItemData item, int quantity = 1)
    {
        // Stack if already exists
        foreach (var slot in ingredientSlots)
        {
            if (slot.item == item)
            {
                slot.quantity += quantity;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        if (ingredientSlots.Count >= MaxIngredientSlots)
        {
            Debug.Log("Ingredient slots full!");
            return false;
        }

        ingredientSlots.Add(new InventorySlot(item, quantity));
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        for (int i = 0; i < ingredientSlots.Count; i++)
        {
            if (ingredientSlots[i].item == item)
            {
                ingredientSlots[i].quantity -= quantity;
                if (ingredientSlots[i].quantity <= 0)
                    ingredientSlots.RemoveAt(i);

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log($"Item {item.itemName} not found.");
        return false;
    }

    public bool HasIngredient(ItemData item, int quantity = 1)
    {
        foreach (var slot in ingredientSlots)
        {
            if (slot.item == item && slot.quantity >= quantity)
                return true;
        }
        return false;
    }

    // Called when player dies
    public void ClearInventory()
    {
        ingredientSlots.Clear();
        // Sword is kept on death? Or lost? Currently: kept.
        OnInventoryChanged?.Invoke();
    }
}