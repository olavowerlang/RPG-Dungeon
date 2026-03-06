using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData item;
    [Range(0f, 1f)] public float weight = 1f; // relative weight, not individual chance
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootEntry> entries = new();

    [Range(0f, 1f)]
    public float dropChance = 0.8f; // overall chance of dropping anything at all

    // Returns exactly 1 random item, or null if nothing drops
    public ItemData Roll()
    {
        if (entries.Count == 0) return null;

        // First check overall drop chance
        if (Random.value > dropChance) return null;

        // Pick one item based on weights
        float totalWeight = 0f;
        foreach (var entry in entries)
            totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.item;
        }

        return entries[entries.Count - 1].item;
    }
}