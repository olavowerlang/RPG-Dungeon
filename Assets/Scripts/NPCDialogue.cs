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

    // Set when the zone 1 NPC (unlockSwordOnComplete) finishes their dialogue.
    // Checked by Zone1ExitGate in NG+1.
    public static bool Zone1MainDone { get; private set; }
    public static void ResetZone1() { Zone1MainDone = false; }

    private bool _playerInRange;
    private bool _mainDone;
    private bool _wasInDialogue;
    private float _cooldown;

    private bool IsNGPlus => NGPlusManager.Instance != null && NGPlusManager.Instance.GameCleared;

    private void Update()
    {
        bool inDialogue = DialogueManager.Instance.IsInDialogue;

        // Cooldown prevents same E press that closes dialogue from reopening it
        if (_wasInDialogue && !inDialogue) _cooldown = 0.15f;
        _wasInDialogue = inDialogue;
        _cooldown -= Time.deltaTime;

        bool canInteract = _playerInRange && !inDialogue && _cooldown <= 0f;

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
                    if (unlockSwordOnComplete)
                    {
                        Zone1MainDone = true;
                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.hasSword = true;
                            if (swordItem != null)
                                InventoryManager.Instance.EquipSword(swordItem);
                        }
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
