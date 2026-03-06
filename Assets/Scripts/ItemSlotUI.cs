using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button button; // optional — assign if slot is clickable

    private Action _onClickCallback;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => _onClickCallback?.Invoke());
    }

    public void SetSlot(InventorySlot slot)
    {
        if (slot == null || slot.item == null) { Clear(); return; }

        iconImage.sprite = slot.item.icon;
        iconImage.enabled = true;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
    }

    public void SetClickCallback(Action callback)
    {
        _onClickCallback = callback;
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        quantityText.text = "";
        _onClickCallback = null;
    }
}