using UnityEngine;

/// <summary>
/// Place a solid BoxCollider2D strip across the exit of Zone 6.
/// Blocks until the player is carrying 15 mushrooms (or has already completed the quest).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Zone6ExitGate : MonoBehaviour
{
    [SerializeField] private ItemData    mushroomItem;
    [SerializeField] private int         requiredCount  = 15;
    [SerializeField] private DialogueData blockedDialogue; // "you need 15 mushrooms to pass"

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        bool canPass = MushroomQuestPig.QuestDone ||
                       (InventoryManager.Instance != null &&
                        InventoryManager.Instance.HasIngredient(mushroomItem, requiredCount));

        if (!canPass)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                DialogueManager.Instance.StartDialogue(blockedDialogue);
            return;
        }

        GetComponent<BoxCollider2D>().enabled = false;
    }
}
