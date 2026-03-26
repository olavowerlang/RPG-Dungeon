using UnityEngine;

/// <summary>
/// NPC Instance 3.
/// - First E press  → mushroom quest dialogue (hands in mushrooms, gives gold).
/// - After that     → E shows [Talk][Buy] panel. Both always available.
/// CaveExitGate checks TalkDone.
/// </summary>
public class MushroomQuestPig : MonoBehaviour
{
    public static bool QuestDone { get; private set; }
    public static bool TalkDone  { get; private set; }

    [Header("Quest")]
    [SerializeField] private ItemData mushroomItem;
    [SerializeField] private int      requiredCount = 15;
    [SerializeField] private int      goldReward    = 10;

    [Header("Dialogue")]
    [SerializeField] private DialogueData rewardDialogue;
    [SerializeField] private DialogueData repeatDialogue;

    [Header("UI")]
    [SerializeField] private GameObject          interactionPrompt;
    [SerializeField] private NPCInteractionPanel interactionPanel;

    [Header("Gold Pop")]
    [SerializeField] private int goldLineIndex = 5;

    private bool _playerInRange;
    private bool _panelOpen;
    private bool _goldGiven;

    private void Awake()
    {
        QuestDone = false;
        TalkDone  = false;
    }

    private void OnEnable()  => DialogueManager.OnLineShown += OnLineShown;
    private void OnDisable() => DialogueManager.OnLineShown -= OnLineShown;

    private void OnLineShown(int index)
    {
        if (_goldGiven || !QuestDone) return;
        if (index != goldLineIndex) return;

        _goldGiven = true;
        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(goldReward);
        AudioManager.Instance?.PlayCoinReward();

        var player = GameObject.FindWithTag("Player");
        Vector3 popPos = player != null
            ? player.transform.position + new Vector3(0f, 1.5f, 0f)
            : transform.position + new Vector3(0f, 1.5f, 0f);
        FloatingText.Spawn($"+{goldReward} Gold", popPos);
    }

    private void Update()
    {
        if (StoreManager.Instance != null && StoreManager.Instance.IsStoreOpen) return;
        if (_panelOpen) return;

        bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue;
        bool canInteract = _playerInRange && !dialogueActive;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (!canInteract) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!TalkDone)
                FirstTalk();
            else
                OpenPanel();
        }
    }

    private void FirstTalk()
    {
        if (DialogueManager.Instance == null) return;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(mushroomItem, requiredCount);

        QuestDone = true;
        TalkDone  = true;

        if (rewardDialogue != null)
            DialogueManager.Instance.StartDialogue(rewardDialogue);
    }

    private void OpenPanel()
    {
        if (interactionPanel == null) return;
        _panelOpen = true;
        interactionPanel.Show(
            new[] { "Talk", "Buy" },
            new[] { true, true },
            OnOptionSelected,
            onCancel: () => _panelOpen = false
        );
    }

    private void OnOptionSelected(int index)
    {
        _panelOpen = false;
        if (index == 0) // Talk
        {
            if (DialogueManager.Instance != null && repeatDialogue != null)
                DialogueManager.Instance.StartDialogue(repeatDialogue);
        }
        else if (index == 1) // Buy
        {
            StoreManager.Instance?.OpenStore();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;

        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        if (_panelOpen)
        {
            interactionPanel?.Hide();
            _panelOpen = false;
        }
    }
}
