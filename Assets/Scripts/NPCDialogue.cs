using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData repeatDialogue;
    [SerializeField] private GameObject interactionPrompt;

    private bool _playerInRange;
    private bool _mainDone;

    private void Update()
    {
        bool canInteract = _playerInRange && !DialogueManager.Instance.IsInDialogue;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            if (!_mainDone)
                DialogueManager.Instance.StartDialogue(mainDialogue, () => _mainDone = true);
            else
                DialogueManager.Instance.StartDialogue(repeatDialogue);
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
