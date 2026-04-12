using UnityEngine;

/// <summary>
/// NPC2 flow:
///   1. First E → introDialogue. Sets IntroDone.
///   2. E again → panel [Talk (greyed until dash)] [Buy]
///   3. Talk first time (dash owned) → mainDialogue. Sets TalkDone → clears Zone3 gate.
///   4. Talk after → repeatDialogue.
/// NG+ variants replace each dialogue when NGPlusManager.GameCleared is true.
/// </summary>
public class PigShopkeeper : MonoBehaviour
{
    public enum NpcId { Instance2 = 2, Instance3 = 3 }

    [Header("Identity")]
    [SerializeField] private NpcId npcId = NpcId.Instance2;
    [Tooltip("If true, Talk is greyed until the player owns the dash.")]
    [SerializeField] private bool requireDashForTalk = false;

    [Header("Dialogue")]
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData repeatDialogue;

    [Header("NG+ Dialogue (index 0 = NG+,  1 = NG++,  2 = NG+++)")]
    [SerializeField] private DialogueData[] ngPlusIntroDialogues;
    [SerializeField] private DialogueData[] ngPlusMainDialogues;
    [SerializeField] private DialogueData[] ngPlusRepeatDialogues;

    [Header("UI")]
    [SerializeField] private GameObject          interactionPrompt;
    [SerializeField] private NPCInteractionPanel interactionPanel;

    // ── Static flags ───────────────────────────────────────────────────────────
    public static bool Instance2IntroDone { get; private set; }
    public static bool Instance2TalkDone  { get; private set; }
    public static bool Instance3IntroDone { get; private set; }
    public static bool Instance3TalkDone  { get; private set; }

    public static void ResetAll()
    {
        Instance2IntroDone = false;
        Instance2TalkDone  = false;
        Instance3IntroDone = false;
        Instance3TalkDone  = false;
    }

    // Backwards-compatibility alias
    public static bool MainDialogueDone => Instance2IntroDone;

    // ── Helpers ────────────────────────────────────────────────────────────────
    private bool IntroDoneFlag
    {
        get => npcId == NpcId.Instance2 ? Instance2IntroDone : Instance3IntroDone;
        set { if (npcId == NpcId.Instance2) Instance2IntroDone = value; else Instance3IntroDone = value; }
    }

    private bool TalkDoneFlag
    {
        get => npcId == NpcId.Instance2 ? Instance2TalkDone : Instance3TalkDone;
        set { if (npcId == NpcId.Instance2) Instance2TalkDone = value; else Instance3TalkDone = value; }
    }

    private DialogueData PickIntro   => NGPlusManager.PickDialogue(introDialogue,  ngPlusIntroDialogues);
    private DialogueData PickMain    => NGPlusManager.PickDialogue(mainDialogue,   ngPlusMainDialogues);
    private DialogueData PickRepeat  => NGPlusManager.PickDialogue(repeatDialogue, ngPlusRepeatDialogues);

    // ── Instance state ─────────────────────────────────────────────────────────
    private bool _playerInRange;
    private bool _panelOpen;
    private bool _wasInDialogue;
    private float _cooldown;

    // ── Unity lifecycle ────────────────────────────────────────────────────────
    private void Update()
    {
        if (StoreManager.Instance != null && StoreManager.Instance.IsStoreOpen) return;
        if (_panelOpen) return;

        bool inDialogue = DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue;
        if (_wasInDialogue && !inDialogue) _cooldown = 0.15f;
        _wasInDialogue = inDialogue;
        _cooldown -= Time.deltaTime;

        bool canInteract = _playerInRange && !inDialogue && _cooldown <= 0f;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (!canInteract) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!IntroDoneFlag)
                PlayIntro();
            else
                OpenPanel();
        }
    }

    // ── Dialogue ───────────────────────────────────────────────────────────────
    private void PlayIntro()
    {
        var d = PickIntro;
        if (DialogueManager.Instance == null || d == null) return;
        DialogueManager.Instance.StartDialogue(d, () => IntroDoneFlag = true);
    }

    // ── Panel ──────────────────────────────────────────────────────────────────
    private void OpenPanel()
    {
        if (interactionPanel == null) return;
        _panelOpen = true;

        bool talkEnabled = !requireDashForTalk
            || (PlayerStats.Instance != null && PlayerStats.Instance.hasDash);

        interactionPanel.Show(
            new[] { "Talk", "Buy" },
            new[] { talkEnabled, true },
            OnOptionSelected,
            onCancel: () => _panelOpen = false
        );
    }

    private void OnOptionSelected(int index)
    {
        _panelOpen = false;

        if (index == 0) // Talk
        {
            if (DialogueManager.Instance == null) return;

            if (!TalkDoneFlag)
            {
                var d = PickMain;
                if (d == null) return;
                DialogueManager.Instance.StartDialogue(d, () => TalkDoneFlag = true);
            }
            else
            {
                var d = PickRepeat;
                if (d == null) return;
                DialogueManager.Instance.StartDialogue(d);
            }
        }
        else if (index == 1) // Buy
        {
            StoreManager.Instance?.OpenStore();
        }
    }

    // ── Trigger ────────────────────────────────────────────────────────────────
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
