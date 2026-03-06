using UnityEngine;

public enum ItemType { Material, Consumable }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
    public int goldValue;
}