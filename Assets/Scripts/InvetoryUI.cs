using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Sword Slot")]
    [SerializeField] private ItemSlotUI swordSlotUI; // the single sword slot

    [Header("Ingredient Slots (10)")]
    [SerializeField] private List<ItemSlotUI> ingredientSlotUIs = new(); // 10 slots

    [Header("Fusion Area")]
    // Shows: [Sword icon] + [Selected ingredient icon] = [Result text]
    [SerializeField] private Image fusionSwordIcon;       // always shows sword
    [SerializeField] private Image fusionIngredientIcon;  // shows selected ingredient
    [SerializeField] private TextMeshProUGUI fusionResultText;    // e.g. "+2 Damage"
    [SerializeField] private Button fuseButton;            // "Fuse into Sword"

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 2f;

    private ItemData _selectedIngredient = null;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);

        if (feedbackText != null) feedbackText.enabled = false;
        ClearFusionPreview();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshAll;

        if (CraftingSystem.Instance != null)
            CraftingSystem.Instance.OnFuseSuccess += ShowFeedback;

        fuseButton.onClick.AddListener(OnFuseClicked);
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshAll;

        if (CraftingSystem.Instance != null)
            CraftingSystem.Instance.OnFuseSuccess -= ShowFeedback;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        bool isOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen) RefreshAll();
    }

    private void RefreshAll()
    {
        if (!inventoryPanel.activeSelf) return;

        RefreshSwordSlot();
        RefreshIngredientSlots();
        RefreshFusionPreview();
    }

    private void RefreshSwordSlot()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        if (inv.HasSword())
            swordSlotUI.SetSlot(new InventorySlot(inv.equippedSword, 1));
        else
            swordSlotUI.Clear();
    }

    private void RefreshIngredientSlots()
    {
        var slots = InventoryManager.Instance?.ingredientSlots;

        for (int i = 0; i < ingredientSlotUIs.Count; i++)
        {
            if (slots != null && i < slots.Count)
            {
                ingredientSlotUIs[i].SetSlot(slots[i]);
                int captured = i;
                if (slots[captured].item != null && slots[captured].item.itemType == ItemType.Consumable)
                    ingredientSlotUIs[i].SetClickCallback(() => UseConsumable(slots[captured].item));
                else
                    ingredientSlotUIs[i].SetClickCallback(() => SelectIngredient(slots[captured].item));
            }
            else
            {
                ingredientSlotUIs[i].Clear();
                ingredientSlotUIs[i].SetClickCallback(null);
            }
        }
    }

    private void UseConsumable(ItemData item)
    {
        if (item == null || item.itemType != ItemType.Consumable) return;
        var health = PlayerStats.Instance?.GetComponent<Health>();
        if (health == null) return;
        health.HealFull();
        InventoryManager.Instance.RemoveItem(item, 1);
    }

    // Called when player clicks an ingredient slot
    private void SelectIngredient(ItemData item)
    {
        _selectedIngredient = item;
        RefreshFusionPreview();
    }

    private void RefreshFusionPreview()
    {
        bool hasSword = InventoryManager.Instance != null && InventoryManager.Instance.HasSword();

        if (!hasSword || _selectedIngredient == null)
        {
            ClearFusionPreview();
            return;
        }

        // Show sword icon
        if (fusionSwordIcon != null)
        {
            fusionSwordIcon.sprite = InventoryManager.Instance.equippedSword.icon;
            fusionSwordIcon.enabled = true;
        }

        // Show selected ingredient icon
        if (fusionIngredientIcon != null)
        {
            fusionIngredientIcon.sprite = _selectedIngredient.icon;
            fusionIngredientIcon.enabled = true;
        }

        // Show result description if a recipe exists
        var recipe = CraftingSystem.Instance?.GetRecipeForIngredient(_selectedIngredient);
        if (fusionResultText != null)
            fusionResultText.text = recipe != null ? recipe.resultDescription : "No recipe";

        // Enable fuse button only if recipe exists and player has the ingredient
        bool canFuse = recipe != null &&
                       InventoryManager.Instance.HasIngredient(_selectedIngredient, recipe.quantity);
        fuseButton.interactable = canFuse;
    }

    private void ClearFusionPreview()
    {
        if (fusionSwordIcon != null) fusionSwordIcon.enabled = false;
        if (fusionIngredientIcon != null) fusionIngredientIcon.enabled = false;
        if (fusionResultText != null) fusionResultText.text = "";
        if (fuseButton != null) fuseButton.interactable = false;
    }

    private void OnFuseClicked()
    {
        if (_selectedIngredient == null) return;
        CraftingSystem.Instance?.TryFuse(_selectedIngredient);
        // Keep _selectedIngredient so player can keep fusing without re-clicking
        // RefreshAll (fired by OnInventoryChanged) will disable the button if item runs out
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
        StopAllCoroutines();
        StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        feedbackText.text = "Fused! " + message;
        feedbackText.enabled = true;
        yield return new WaitForSecondsRealtime(feedbackDuration);
        feedbackText.enabled = false;
    }
}