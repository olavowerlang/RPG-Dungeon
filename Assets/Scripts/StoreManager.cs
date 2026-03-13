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
        if (!GoldManager.Instance.SpendGold(item.price)) return false;
        if (item.itemData != null)
            InventoryManager.Instance.AddItem(item.itemData);
        return true;
    }
}
