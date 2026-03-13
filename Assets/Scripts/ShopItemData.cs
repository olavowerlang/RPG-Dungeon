using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;
    public ItemData itemData; // item added to player inventory on purchase
}
