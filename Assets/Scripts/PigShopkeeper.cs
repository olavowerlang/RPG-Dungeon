using UnityEngine;

/// <summary>
/// Full NPC logic for the pig shopkeeper (MainNPCInstance2).
///
/// E key    → checks gates first; if all cleared plays repeat dialogue, otherwise activates gate
/// Click    → same gate check; once all cleared opens the store
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

        // E key → gate check first, then repeat dialogue only after dash is unlocked
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!AllGatesCleared())
                ActivateFirstUnclearedGate();
            else if (PlayerStats.Instance != null && PlayerStats.Instance.hasDash)
            {
                if (!_mainDone)
                    DialogueManager.Instance.StartDialogue(mainDialogue, () => _mainDone = true);
                else
                    DialogueManager.Instance.StartDialogue(repeatDialogue);
            }
            // gates cleared but dash not yet bought: E does nothing — go buy dash from store
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

    private bool AllGatesCleared()
    {
        foreach (var gate in gates)
            if (gate != null && !gate.IsCleared) return false;
        return true;
    }

    private void ActivateFirstUnclearedGate()
    {
        foreach (var gate in gates)
        {
            if (gate != null && !gate.IsCleared)
            {
                gate.Activate();
                return;
            }
        }
    }

    private void TryOpenStore()
    {
        if (!AllGatesCleared())
        {
            ActivateFirstUnclearedGate();
            return;
        }

        if (StoreManager.Instance == null)
        {
            Debug.LogError("StoreManager instance is null!");
            return;
        }

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