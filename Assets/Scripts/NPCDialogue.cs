using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private GameObject interactionPrompt; // "[E] Falar" — objeto filho do NPC

    private bool _playerInRange;

    private void Update()
    {
        bool canInteract = _playerInRange && !DialogueManager.Instance.IsInDialogue;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (canInteract && Input.GetKeyDown(KeyCode.E))
            DialogueManager.Instance.StartDialogue(dialogueData);
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
