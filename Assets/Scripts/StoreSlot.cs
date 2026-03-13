using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private ShopItemData _item;
    private StoreUI _storeUI;

    public void Setup(ShopItemData item, StoreUI storeUI)
    {
        _item = item;
        _storeUI = storeUI;

        if (icon != null && item.icon != null) icon.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.displayName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (priceText != null) priceText.text = $"{item.price}g";
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        bool success = _storeUI.OnPurchase(_item);
        if (!success && feedbackText != null)
        {
            feedbackText.text = "Not enough gold!";
            feedbackText.gameObject.SetActive(true);
        }
        else if (success && feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }
}
