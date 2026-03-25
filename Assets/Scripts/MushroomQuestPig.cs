using UnityEngine;

/// <summary>
/// Attach to pig instance 3 (zone 7).
/// Player arrives with 15 mushrooms already (gate blocks entry otherwise).
/// Takes mushrooms, gives gold, plays reward dialogue. Repeat dialogue after.
/// </summary>
public class MushroomQuestPig : MonoBehaviour
{
    // Gate reads this to know if quest is already done (e.g. player returns)
    public static bool QuestDone { get; private set; }

    [Header("Quest")]
    [SerializeField] private ItemData mushroomItem;
    [SerializeField] private int      requiredCount = 15;
    [SerializeField] private int      goldReward    = 10;

    [Header("Dialogue")]
    [SerializeField] private DialogueData rewardDialogue;   // fires once — takes mushrooms, gives gold
    [SerializeField] private DialogueData repeatDialogue;   // every talk after

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("Gold Pop")]
    [SerializeField] private int goldLineIndex = 5; // 0-based index of the "here have this" line

    private bool _playerInRange;
    private bool _goldGiven;

    // Reset on each scene load so NG+ runs start fresh
    private void Awake() => QuestDone = false;

    private void OnEnable()  => DialogueManager.OnLineShown += OnLineShown;
    private void OnDisable() => DialogueManager.OnLineShown -= OnLineShown;

    private void OnLineShown(int index)
    {
        if (_goldGiven || QuestDone == false) return;
        if (index != goldLineIndex) return;

        _goldGiven = true;
        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(goldReward);

        var player = GameObject.FindWithTag("Player");
        Vector3 popPos = player != null
            ? player.transform.position + new Vector3(0f, 1.5f, 0f)
            : transform.position + new Vector3(0f, 1.5f, 0f);
        FloatingText.Spawn($"+{goldReward} Gold", popPos);
    }

    private void Update()
    {
        if (DialogueManager.Instance == null) return;

        bool canInteract = _playerInRange && !DialogueManager.Instance.IsInDialogue;
        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (canInteract && Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    private void Interact()
    {
        if (QuestDone)
        {
            if (repeatDialogue != null)
                DialogueManager.Instance.StartDialogue(repeatDialogue);
            return;
        }

        // Take mushrooms immediately
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(mushroomItem, requiredCount);

        QuestDone = true;

        // Gold fires mid-dialogue via OnLineShown when the reward line appears
        if (rewardDialogue != null)
            DialogueManager.Instance.StartDialogue(rewardDialogue);
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
