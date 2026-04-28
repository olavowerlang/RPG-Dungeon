using System;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public bool IsInDialogue { get; private set; }

    /// <summary>Fires every time a new line is shown. Passes the 0-based line index.</summary>
    public static event Action<int> OnLineShown;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continuePrompt;

    [System.Serializable]
    public struct SpeakerBox
    {
        public string       speakerName; // must match exactly what's in the DialogueData
        public GameObject   nameBox;     // the background panel sized for this speaker
    }

    [Header("Speaker Name Boxes (optional)")]
    [Tooltip("Map each speaker name to a pre-sized name box. All boxes are hidden except the active speaker's.")]
    [SerializeField] private SpeakerBox[] speakerBoxes;
    [Tooltip("Extra name box GameObjects that should always be hidden during dialogue but aren't mapped to any speaker.")]
    [SerializeField] private GameObject[] extraBoxes;

    private string _currentBoxSpeaker;

    private DialogueLine[] _lines;
    private int _currentLine;
    private bool _justOpened;
    private Action _onComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsInDialogue) return;

        if (_justOpened) { _justOpened = false; return; }

        if (Input.GetKeyDown(KeyCode.E))
            AdvanceDialogue();
    }

    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        if (IsInDialogue) return;

        _lines = data.lines;
        _currentLine = 0;
        IsInDialogue = true;
        _justOpened = true;
        _onComplete = onComplete;
        _currentBoxSpeaker = null;

        HideAllBoxes();

        dialoguePanel.SetActive(true);
        AudioManager.Instance?.PlayDialogueOpen();
        ShowLine(_currentLine);
    }

    private void AdvanceDialogue()
    {
        AudioManager.Instance?.PlayDialogueAdvance();
        _currentLine++;
        if (_currentLine >= _lines.Length)
            EndDialogue();
        else
            ShowLine(_currentLine);
    }

    private void ShowLine(int index)
    {
        string speaker = _lines[index].speakerName;
        speakerNameText.text = speaker;
        dialogueText.text    = _lines[index].text;

        bool isLast = index == _lines.Length - 1;
        continuePrompt.text = isLast ? "[E] Close" : "[E] Continue";

        // Swap name boxes only when the speaker actually changes
        if (speakerBoxes != null && speakerBoxes.Length > 0 && speaker != _currentBoxSpeaker)
        {
            _currentBoxSpeaker = speaker;
            foreach (var entry in speakerBoxes)
                if (entry.nameBox != null)
                    entry.nameBox.SetActive(entry.speakerName == speaker);
        }

        OnLineShown?.Invoke(index);
    }

    private void HideAllBoxes()
    {
        // Hide every DialogueBG2* child automatically — covers registered and unregistered boxes
        foreach (Transform child in dialoguePanel.transform)
            if (child.name.StartsWith("DialogueBG2"))
                child.gameObject.SetActive(false);

        if (extraBoxes != null)
            foreach (var box in extraBoxes)
                if (box != null) box.SetActive(false);
    }

    public void ForceEnd()
    {
        if (!IsInDialogue) return;
        EndDialogue();
    }

    private void EndDialogue()
    {
        IsInDialogue = false;
        dialoguePanel.SetActive(false);
        _currentBoxSpeaker = null;
        HideAllBoxes();
        _onComplete?.Invoke();
        _onComplete = null;
    }
}
