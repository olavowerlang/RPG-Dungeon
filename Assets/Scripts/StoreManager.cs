using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    [SerializeField] private StoreUI storeUI;

    public bool IsStoreOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OpenStore()
    {
        IsStoreOpen = true;
        storeUI.Open();
    }

    public void CloseStore()
    {
        IsStoreOpen = false;
        storeUI.Close();
    }

    public bool TryPurchase(ShopItemData item)
    {
        if (item.isUnlock)
        {
            if (!GoldManager.Instance.SpendGold(item.price)) return false;
            if (PlayerStats.Instance != null && item.unlockType == UnlockType.Dash)
                PlayerStats.Instance.hasDash = true;
            storeUI.RefreshSlots();
            return true;
        }

        if (item.isDirectBuff)
        {
            if (!GoldManager.Instance.SpendGold(item.price)) return false;
            CraftingSystem.Instance.ApplyDirectBuff(item.directBuffType);
            return true;
        }

        if (item.itemData == null) return false;
        if (!GoldManager.Instance.SpendGold(item.price)) return false;
        if (!InventoryManager.Instance.AddItem(item.itemData))
        {
            GoldManager.Instance.AddGold(item.price); // refund — inventory full
            return false;
        }
        return true;
    }
}
