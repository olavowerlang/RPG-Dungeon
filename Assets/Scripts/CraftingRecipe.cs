using UnityEngine;

public enum BuffType { Damage, DashSpeed, Knockback, MoveSpeed, MaxHP }

// Each recipe = one ingredient fused into the sword
[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Info")]
    public string recipeName;           // e.g. "Iron Forge"
    public string resultDescription;    // e.g. "+2 Damage"

    [Header("Ingredient needed")]
    public ItemData ingredient;
    public int quantity = 1;

    [Header("Buff applied to sword")]
    public BuffType buffType;
    public float buffValue;
}