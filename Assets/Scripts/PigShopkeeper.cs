using UnityEngine;

/// <summary>
/// Full NPC logic for the pig shopkeeper (MainNPCInstance2).
/// Replaces NPCDialogue on that object.
///
/// E key    → always dialogue (main first, then repeat)
/// Click    → checks gates in order; first uncleared gate activates instead of store.
///            Once all gates are cleared, opens the store directly.
///
/// To add a future gate: add a ShopGate subclass component to this GameObject,
/// then drag it into the Gates array in the correct position.
/// </summary>
public class PigShopkeeper : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData repeatDialogue;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("Gates (evaluated in order — first uncleared blocks the store)")]
    [SerializeField] private ShopGate[] gates;

    private bool _playerInRange;
    private bool _mainDone;

    private void Update()
    {
        if (StoreManager.Instance != null && StoreManager.Instance.IsStoreOpen) return;

        bool canInteract = _playerInRange && !DialogueManager.Instance.IsInDialogue;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (!canInteract) return;

        // E key → always dialogue
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!_mainDone)
                DialogueManager.Instance.StartDialogue(mainDialogue, OnMainDialogueDone);
            else
                DialogueManager.Instance.StartDialogue(repeatDialogue);
            return;
        }

        // Left click on pig → gate check or store
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
            foreach (var hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    TryOpenStore();
                    break;
                }
            }
        }
    }

    private void OnMainDialogueDone()
    {
        _mainDone = true;
    }

    private void TryOpenStore()
    {
        // Main dialogue must be completed first
        if (!_mainDone)
        {
            DialogueManager.Instance.StartDialogue(mainDialogue, OnMainDialogueDone);
            return;
        }

        // Check gates in order — first uncleared one blocks the store
        foreach (var gate in gates)
        {
            if (gate != null && !gate.IsCleared)
            {
                gate.Activate();
                return;
            }
        }

        // All gates cleared → open store
        StoreManager.Instance.OpenStore();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}
