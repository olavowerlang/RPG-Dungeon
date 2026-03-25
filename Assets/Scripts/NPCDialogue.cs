using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData repeatDialogue;
    [SerializeField] private DialogueData ngPlusMainDialogue;    // shown instead of main in NG+
    [SerializeField] private DialogueData ngPlusRepeatDialogue;  // shown instead of repeat in NG+
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private bool unlockSwordOnComplete;
    [SerializeField] private ItemData swordItem;

    private bool _playerInRange;
    private bool _mainDone;

    private bool IsNGPlus => NGPlusManager.Instance != null && NGPlusManager.Instance.GameCleared;

    private void Update()
    {
        bool canInteract = _playerInRange && !DialogueManager.Instance.IsInDialogue;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            if (!_mainDone)
            {
                DialogueData d = IsNGPlus && ngPlusMainDialogue != null ? ngPlusMainDialogue : mainDialogue;
                DialogueManager.Instance.StartDialogue(d, () =>
                {
                    _mainDone = true;
                    if (unlockSwordOnComplete && PlayerStats.Instance != null)
                    {
                        PlayerStats.Instance.hasSword = true;
                        if (swordItem != null)
                            InventoryManager.Instance.EquipSword(swordItem);
                    }
                });
            }
            else
            {
                DialogueData d = IsNGPlus && ngPlusRepeatDialogue != null ? ngPlusRepeatDialogue : repeatDialogue;
                DialogueManager.Instance.StartDialogue(d);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}
