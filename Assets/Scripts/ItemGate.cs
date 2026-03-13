using UnityEngine;

/// <summary>
/// Gate that requires the player to hand over specific items.
/// Plays different dialogue depending on whether the player has the items or not.
/// </summary>
public class ItemGate : ShopGate
{
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private int requiredQuantity = 1;
    [SerializeField] private DialogueData hasItemDialogue;     // played when items are delivered
    [SerializeField] private DialogueData missingItemDialogue; // played when items are missing

    private bool _cleared;

    public override bool IsCleared => _cleared;

    public override void Activate()
    {
        if (DialogueManager.Instance.IsInDialogue) return;

        if (InventoryManager.Instance.HasIngredient(requiredItem, requiredQuantity))
        {
            InventoryManager.Instance.RemoveItem(requiredItem, requiredQuantity);
            _cleared = true;
            if (hasItemDialogue != null)
                DialogueManager.Instance.StartDialogue(hasItemDialogue);
        }
        else
        {
            if (missingItemDialogue != null)
                DialogueManager.Instance.StartDialogue(missingItemDialogue);
        }
    }
}
