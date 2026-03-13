using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject storePanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button closeButton;

    [Header("Items for Sale")]
    [SerializeField] private List<ShopItemData> itemsForSale;

    private void Start()
    {
        storePanel.SetActive(false);
        closeButton.onClick.AddListener(() => StoreManager.Instance.CloseStore());

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(GoldManager.Instance.Gold);
        }
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
    }

    public void Open()
    {
        RefreshSlots();
        UpdateGoldDisplay(GoldManager.Instance != null ? GoldManager.Instance.Gold : 0);
        storePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        storePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool OnPurchase(ShopItemData item)
    {
        return StoreManager.Instance.TryPurchase(item);
    }

    private void RefreshSlots()
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var item in itemsForSale)
        {
            var slot = Instantiate(itemSlotPrefab, itemContainer);
            slot.GetComponent<StoreSlot>().Setup(item, this);
        }
    }

    private void UpdateGoldDisplay(int amount)
    {
        if (goldText != null)
            goldText.text = amount.ToString();
    }
}
