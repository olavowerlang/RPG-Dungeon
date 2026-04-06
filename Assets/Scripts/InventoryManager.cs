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

    // Sword slot � separate from ingredients
    public ItemData equippedSword { get; private set; } = null;

    // Ingredient slots only (max 10)
    public List<InventorySlot> ingredientSlots = new();

    public event Action OnInventoryChanged;

    // TEMP: assign the Sword ScriptableObject here in the Inspector
    // Remove this when the NPC pig gives the sword instead
    [SerializeField] private ItemData _startingSword;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_startingSword != null)
            EquipSword(_startingSword);
    }

    public void EquipSword(ItemData sword)
    {
        if (equippedSword != null) return;
        equippedSword = sword;
        OnInventoryChanged?.Invoke();
    }

    public bool HasSword() => equippedSword != null;

    public bool AddItem(ItemData item, int quantity = 1)
    {
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

    public void ClearInventory()
    {
        ingredientSlots.Clear();
        equippedSword = null;
        OnInventoryChanged?.Invoke();
    }
}