using System.Collections;
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
        if (priceText != null) priceText.text = item.price == 0 ? "Free" : $"{item.price}g";
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private Coroutine _feedbackCoroutine;

    private void OnBuyClicked()
    {
        bool success = _storeUI.OnPurchase(_item);
        if (!success)
        {
            ShowFeedback("Not enough gold!");
        }
        else
        {
            if (_item.isUnlock)
            {
                StartCoroutine(FeedbackThenDestroy("Unlocked!"));
                return;
            }
            ShowFeedback("Bought!");
        }
    }

    private void ShowFeedback(string message)
    {
        if (_feedbackCoroutine != null)
            StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackThenDestroy(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        buyButton.interactable = false;
        yield return new WaitForSecondsRealtime(2f);
        Destroy(gameObject);
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }
}
